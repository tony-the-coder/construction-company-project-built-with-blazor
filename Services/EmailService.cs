// File: Services/EmailService.cs
using LehmanCustomConstruction.Data.Common; // For EmailSettings class
using LehmanCustomConstruction.Services.Interfaces; // For IEmailService interface
using MailKit.Net.Smtp; // For SmtpClient
using MailKit.Security; // For SecureSocketOptions
using Microsoft.Extensions.Options; // For injecting IOptions<EmailSettings>
using MimeKit; // For MimeMessage, MailboxAddress
using MimeKit.Text; // For TextPart, TextFormat
using System; // For Exception, Console
using System.Threading.Tasks; // For Task

namespace LehmanCustomConstruction.Services // Create Services folder if needed
{
    /// <summary>
    /// Service responsible for sending emails using MailKit and configured SMTP settings.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger; // Added logger for better diagnostics

        /// <summary>
        /// Initializes a new instance of the EmailService.
        /// </summary>
        /// <param name="emailOptions">The configured email settings injected via IOptions.</param>
        /// <param name="logger">The logger for logging email sending activity.</param>
        public EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger)
        {
            // Get the actual settings object from the wrapper
            _emailSettings = emailOptions.Value;
            _logger = logger;

            // Optional: Add validation here to ensure critical settings are loaded
            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpHost) ||
                string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) ||
                string.IsNullOrWhiteSpace(_emailSettings.SmtpUser) ||
                string.IsNullOrWhiteSpace(_emailSettings.SmtpPass) || // Should come from secrets
                _emailSettings.SmtpPort <= 0)
            {
                _logger.LogError("EmailSettings are incomplete. Please check configuration (appsettings.json and User Secrets/Environment Variables).");
                // Consider throwing an exception here if email is absolutely critical at startup
                // throw new InvalidOperationException("EmailSettings are incomplete.");
            }
        }

        /// <summary>
        /// Sends an email asynchronously using configured SMTP settings.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="toName">The recipient's display name.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="htmlMessage">The HTML body of the email.</param>
        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(htmlMessage))
            {
                _logger.LogWarning("SendEmailAsync called with missing parameters (toEmail, subject, or htmlMessage). Aborting send.");
                return; // Don't attempt to send if essential info is missing
            }

            _logger.LogInformation("Attempting to send email. To: {RecipientEmail}, Subject: {Subject}", toEmail, subject);

            try
            {
                var email = new MimeMessage();
                // Set Sender (FROM): Uses configured name and email
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                // Set Recipient (TO): Uses provided name and email
                email.To.Add(new MailboxAddress(toName ?? string.Empty, toEmail)); // Use empty string if toName is null
                // Set Subject
                email.Subject = subject;
                // Set Body (as HTML)
                email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                // Create and configure SmtpClient
                using var smtp = new SmtpClient();

                // Determine SecureSocketOptions based on common ports (adjust if needed)
                SecureSocketOptions socketOptions = SecureSocketOptions.StartTls; // Default for 587
                if (_emailSettings.SmtpPort == 465)
                {
                    socketOptions = SecureSocketOptions.SslOnConnect;
                }
                else if (_emailSettings.SmtpPort == 25) // Port 25 often requires None or StartTls (less secure)
                {
                    socketOptions = SecureSocketOptions.StartTlsWhenAvailable; // Or SecureSocketOptions.None if TLS isn't supported/required
                }

                // Connect to the SMTP server
                _logger.LogDebug("Connecting to SMTP: {Host}:{Port} using {SocketOptions}", _emailSettings.SmtpHost, _emailSettings.SmtpPort, socketOptions);
                await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, socketOptions);
                _logger.LogDebug("Connected to SMTP.");

                // Authenticate using credentials (from secrets/environment variables)
                _logger.LogDebug("Authenticating SMTP user: {SmtpUser}", _emailSettings.SmtpUser);
                await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                _logger.LogDebug("Authenticated successfully.");

                // Send the email
                _logger.LogDebug("Sending email...");
                await smtp.SendAsync(email);
                _logger.LogInformation("Email sent successfully to {RecipientEmail}", toEmail);

                // Disconnect cleanly
                await smtp.DisconnectAsync(true);
                _logger.LogDebug("Disconnected from SMTP.");
            }
            catch (AuthenticationException authEx) // Catch specific exceptions
            {
                _logger.LogError(authEx, "SMTP Authentication failed for user {SmtpUser}. Check credentials/App Password/Less Secure App Access.", _emailSettings.SmtpUser);
                // Re-throw or handle as needed - often indicates configuration error
                throw new InvalidOperationException($"SMTP Authentication failed: {authEx.Message}", authEx);
            }
            catch (SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP Command Exception sending email to {RecipientEmail}. Status Code: {StatusCode}, Message: {ErrorMessage}", toEmail, smtpEx.StatusCode, smtpEx.Message);
                // Re-throw or handle as needed
                throw new InvalidOperationException($"SMTP Command Error: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex)
            {
                // Log generic exceptions
                _logger.LogError(ex, "An unexpected error occurred sending email to {RecipientEmail}", toEmail);
                // Decide whether to re-throw. For contact form, maybe not, so submission succeeds.
                // throw; // Uncomment if email failure should block the calling operation
            }
        }
    }
}