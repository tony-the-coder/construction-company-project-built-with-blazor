// File: Controllers/UploadController.cs
using LehmanCustomConstruction.Data;
using LehmanCustomConstruction.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UploadController> _logger;
    private const string PlaceholderDemoUserId = "683a96be-9b4a-4af7-a863-b98a88815cae";

    public UploadController(ApplicationDbContext dbContext, IConfiguration configuration, ILogger<UploadController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("admin/{customerId}")]
    [DisableRequestSizeLimit]
    // [Authorize(Roles="Admin")] // REMOVED FOR DEMO
    public async Task<IActionResult> AdminUploadToCustomer([FromRoute] string customerId, [FromForm] List<IFormFile> files)
    {
        var uploaderUserId = PlaceholderDemoUserId;
        var targetUserId = customerId;
        _logger.LogWarning("DEMO MODE: AdminUploadToCustomer for Customer {TargetUserId}. Using Placeholder Uploader ID {UploaderId}", targetUserId, uploaderUserId);
        return await ProcessUpload(files, targetUserId, uploaderUserId, "Admin Upload");
    }

    [HttpPost("customer")]
    [DisableRequestSizeLimit]
    // [Authorize] // REMOVED FOR DEMO
    public async Task<IActionResult> CustomerUpload([FromForm] List<IFormFile> files)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isDemoFallback = false;
        if (string.IsNullOrEmpty(userId)) { userId = PlaceholderDemoUserId; isDemoFallback = true; _logger.LogWarning("DEMO MODE FALLBACK: CustomerUpload using Placeholder User ID {UserId}", userId); }
        if (string.IsNullOrEmpty(userId)) { _logger.LogError("CustomerUpload failed: Could not determine User ID."); return Unauthorized(new { Message = "User identity could not be determined." }); }
        string targetUserId = userId; string uploaderUserId = userId;
        string logPrefix = isDemoFallback ? "Customer Upload (Demo Fallback)" : "Customer Upload";
        return await ProcessUpload(files, targetUserId, uploaderUserId, logPrefix);
    }

    private async Task<IActionResult> ProcessUpload(List<IFormFile> files, string targetUserId, string uploaderUserId, string logContext)
    {
        if (!Guid.TryParse(targetUserId, out _) || !Guid.TryParse(uploaderUserId, out _)) { return BadRequest(new { Message = "Invalid User ID format." }); }
        if (files == null || !files.Any()) { return BadRequest(new { Message = "No files received." }); }
        var savedDocuments = new List<object>();
        long maxFileSize = _configuration.GetValue<long>("FileUploadSettings:MaxFileSizeMB", 20) * 1024 * 1024;
        string baseUploadPath = _configuration["FileUploadSettings:BasePath"] ?? string.Empty;
        if (string.IsNullOrEmpty(baseUploadPath)) { _logger.LogError("BasePath not configured."); return StatusCode(500, new { Message = "Server config error: Upload path not set." }); }
        string userDirectory = Path.Combine(baseUploadPath, targetUserId); // <<< Variable is userDirectory
        var fullDirectoryPath = Path.GetFullPath(userDirectory); var fullBasePath = Path.GetFullPath(baseUploadPath);
        if (!fullDirectoryPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase)) { _logger.LogError("Path Traversal Attempt: Target {TargetUserId}, Path {Path}", targetUserId, userDirectory); return BadRequest(new { Message = "Invalid target directory." }); }

        try
        {
            // <<< CORRECTED: Use userDirectory variable >>>
            Directory.CreateDirectory(userDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory: {UserDirectory}", userDirectory);
            return StatusCode(500, new { Message = "Server error creating directory." });
        }

        foreach (var file in files)
        {
            if (file.Length == 0 || file.Length > maxFileSize) { /* ... skip ... */ continue; }
            string safeOriginalFileName = Path.GetFileName(file.FileName); string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(safeOriginalFileName)}"; string filePath = Path.Combine(userDirectory, uniqueFileName); var fullFilePath = Path.GetFullPath(filePath);
            if (!fullFilePath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase)) { /* ... skip ... */ continue; }
            try
            {
                _logger.LogInformation("({Context}) Saving {OriginalFileName}...", logContext, safeOriginalFileName);
                await using FileStream fs = new(filePath, FileMode.Create); await file.CopyToAsync(fs);
                var newDocument = new CustomerDocument { /* ... properties ... */ FileSize = file.Length, TargetUserId = targetUserId, UploadedById = uploaderUserId, /* ... */ };
                _dbContext.CustomerDocuments.Add(newDocument); await _dbContext.SaveChangesAsync();
                _logger.LogInformation("({Context}) DB record created ID: {DocId}", logContext, newDocument.Id);
                savedDocuments.Add(new { FileName = safeOriginalFileName, Size = file.Length, DocumentId = newDocument.Id });
            }
            catch (Exception ex) { /* ... error handling ... */ return StatusCode(500, new { Message = $"Error processing file {file.FileName}: {ex.Message}" }); }
        } // End foreach
        if (!savedDocuments.Any()) { return BadRequest(new { Message = "No valid files were processed." }); }
        return Ok(new { Message = $"{savedDocuments.Count} file(s) uploaded successfully.", Files = savedDocuments });
    }
}