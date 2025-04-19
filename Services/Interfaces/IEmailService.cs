// File: Services/Interfaces/IEmailService.cs
using System.Threading.Tasks; // Required for Task

namespace LehmanCustomConstruction.Services.Interfaces // Create Services/Interfaces folders if they don't exist
{
    /// <summary>
    /// Defines the contract for a service that sends emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="toName">The recipient's display name (optional, used in 'To' field).</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="htmlMessage">The HTML content of the email body.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlMessage);
    }
}