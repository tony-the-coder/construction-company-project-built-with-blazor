// File: Controllers/UploadController.cs
using LehmanCustomConstruction.Data; // For ApplicationDbContext
using LehmanCustomConstruction.Data.Common; // For CustomerDocument
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Required for IFormFile

[ApiController]
[Route("api/[controller]")] // Base route: /api/upload
public class UploadController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UploadController> _logger;

    // --- Placeholder Admin ID (MUST MATCH the one in CustomerDocumentsAdmin.razor FOR DEMO) ---
    private const string PlaceholderAdminId = "683a96be-9b4a-4af7-a863-b98a88815cae"; // test4@test.com ID

    public UploadController(ApplicationDbContext dbContext, IConfiguration configuration, ILogger<UploadController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    // POST /api/upload/admin/{customerId}
    [HttpPost("admin/{customerId}")]
    [DisableRequestSizeLimit] // Allow large uploads - configure Kestrel limits if needed too
    public async Task<IActionResult> AdminUploadToCustomer([FromRoute] string customerId, [FromForm] List<IFormFile> files)
    {
        // WARNING: DEMO MODE - No real authentication check here. Using Placeholder Admin ID.
        var uploaderUserId = PlaceholderAdminId;
        _logger.LogWarning("DEMO MODE: AdminUploadToCustomer endpoint hit for Customer {CustomerId}. Using Placeholder Uploader ID {UploaderId}", customerId, uploaderUserId);

        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out _))
        {
            return BadRequest(new { Message = "Invalid Customer ID format." });
        }

        if (files == null || !files.Any())
        {
            return BadRequest(new { Message = "No files received." });
        }

        var savedDocuments = new List<object>(); // To potentially return info about saved files
        long maxFileSize = _configuration.GetValue<long>("FileUploadSettings:MaxFileSizeMB", 20) * 1024 * 1024;
        string baseUploadPath = _configuration["FileUploadSettings:BasePath"] ?? string.Empty;

        if (string.IsNullOrEmpty(baseUploadPath))
        {
            _logger.LogError("FileUploadSettings:BasePath is not configured.");
            return StatusCode(500, new { Message = "Server configuration error: Upload path not set." });
        }

        // Use the TARGET customer's ID for the directory
        string customerDirectory = Path.Combine(baseUploadPath, customerId);
        var fullDirectoryPath = Path.GetFullPath(customerDirectory);
        var fullBasePath = Path.GetFullPath(baseUploadPath);

        // Security check: Ensure target directory is within the base path
        if (!fullDirectoryPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Potential Path Traversal in Upload Target Directory: Customer {CustomerId}, Path {Path}", customerId, customerDirectory);
            return BadRequest(new { Message = "Invalid target directory." });
        }

        Directory.CreateDirectory(customerDirectory); // Ensure directory exists

        foreach (var file in files)
        {
            if (file.Length == 0 || file.Length > maxFileSize)
            {
                _logger.LogWarning("Skipping file '{FileName}' for Customer {CustomerId} due to invalid size: {FileSize}", file.FileName, customerId, file.Length);
                continue; // Skip this file, process others
            }
            // TODO: Add file type/extension validation here if needed

            string safeOriginalFileName = Path.GetFileName(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(safeOriginalFileName)}";
            string filePath = Path.Combine(customerDirectory, uniqueFileName);
            var fullFilePath = Path.GetFullPath(filePath);

            // Security check: Ensure final file path is within the target directory
            if (!fullFilePath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Potential Path Traversal in Upload File Path: Customer {CustomerId}, Path {Path}", customerId, filePath);
                continue; // Skip this file
            }

            try
            {
                _logger.LogInformation("Saving file {OriginalFileName} for Customer {CustomerId} to {FilePath} by Uploader {UploaderId}", safeOriginalFileName, customerId, filePath, uploaderUserId);
                await using FileStream fs = new(filePath, FileMode.Create);
                await file.CopyToAsync(fs);

                var newDocument = new CustomerDocument
                {
                    OriginalFileName = safeOriginalFileName,
                    StoredFileName = uniqueFileName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    FileSize = file.Length,
                    UploadTimestamp = DateTime.UtcNow,
                    TargetUserId = customerId, // File is for this customer
                    UploadedById = uploaderUserId, // Uploaded by Admin (placeholder)
                    // Description might need to be passed differently or set later
                    Description = null, // Example: No description from this basic upload
                    IsDeleted = false
                };
                _dbContext.CustomerDocuments.Add(newDocument);

                // Save DB record *after* successful file save for this file
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("DB record created for {OriginalFileName}, ID: {DocId}", safeOriginalFileName, newDocument.Id);
                savedDocuments.Add(new { FileName = safeOriginalFileName, Size = file.Length, DocumentId = newDocument.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file '{FileName}' for Customer {CustomerId}", file.FileName, customerId);
                // Attempt to clean up partially saved file if it exists
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); } catch { /* Ignore cleanup error */ }
                }
                // Depending on requirements, you might return partial success or full failure
                // For now, let's return a 500 error if any file fails
                return StatusCode(500, new { Message = $"Error processing file {file.FileName}: {ex.Message}" });
            }
        }

        if (!savedDocuments.Any())
        {
            return BadRequest(new { Message = "No valid files were processed." });
        }

        // Return success, optionally with info about saved files
        return Ok(new { Message = $"{savedDocuments.Count} file(s) uploaded successfully.", Files = savedDocuments });
    }
}