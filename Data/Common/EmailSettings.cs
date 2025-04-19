// File: Data/Common/EmailSettings.cs
namespace LehmanCustomConstruction.Data.Common // Adjust if your 'Common' models folder is elsewhere
{
    /// <summary>
    /// Represents the configuration settings required for sending emails.
    /// Populated from appsettings.json and user secrets/environment variables.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// The display name shown in the 'From' field of the email.
        /// Example: "Lehman Custom Construction"
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// The email address emails will be sent FROM. 
        /// This account needs permissions on the SMTP server.
        /// Example: "no-reply@lehmancustomconstruction.com"
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// The internal email address(es) to send admin notifications to.
        /// Can be a single address or multiple separated by comma/semicolon if your email service handles it.
        /// Example: "info@lehmancustomconstruction.com"
        /// </summary>
        public string AdminNotificationEmail { get; set; } = string.Empty;

        /// <summary>
        /// The hostname or IP address of the SMTP server.
        /// Example: "smtp.gmail.com" or Hostinger's SMTP address
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// The port number for the SMTP server.
        /// Common ports: 587 (TLS), 465 (SSL), 25 (Less secure, often blocked)
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// The username for authenticating with the SMTP server.
        /// Often the same as SenderEmail. Stored securely (User Secrets/Environment Variable).
        /// </summary>
        public string SmtpUser { get; set; } = string.Empty;

        /// <summary>
        /// The password or App Password for authenticating with the SMTP server.
        /// Stored securely (User Secrets/Environment Variable).
        /// </summary>
        public string SmtpPass { get; set; } = string.Empty;
    }
}