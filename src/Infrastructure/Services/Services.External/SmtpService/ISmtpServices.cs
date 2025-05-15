namespace Services.External.SmtpService
{
    /// <summary>
    /// Defines methods for sending emails using SMTP.
    /// </summary>
    public interface ISmtpServices
    {
        /// <summary>
        /// Sends an email to a single recipient.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        void SendEmail(string to, string subject, string body);

        /// <summary>
        /// Sends an email to a single recipient with CC recipients.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <param name="cc">A list of CC recipient email addresses.</param>
        void SendEmail(string to, string subject, string body, List<string> cc);

        /// <summary>
        /// Sends an email to a single recipient with CC and BCC recipients.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <param name="cc">A list of CC recipient email addresses.</param>
        /// <param name="bcc">A list of BCC recipient email addresses.</param>
        void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc);

        /// <summary>
        /// Sends an email to a single recipient with CC, BCC, and attachments.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <param name="cc">A list of CC recipient email addresses.</param>
        /// <param name="bcc">A list of BCC recipient email addresses.</param>
        /// <param name="attachments">A list of file paths for attachments.</param>
        void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc, List<string> attachments);

        /// <summary>
        /// Sends an email to a single recipient with CC, BCC, attachments, and an option to specify if the body is HTML.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <param name="cc">A list of CC recipient email addresses.</param>
        /// <param name="bcc">A list of BCC recipient email addresses.</param>
        /// <param name="attachments">A list of file paths for attachments.</param>
        /// <param name="isBodyHtml">Indicates whether the body content is HTML.</param>
        void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc, List<string> attachments, bool isBodyHtml);

        /// <summary>
        /// Sends an email to a single recipient with CC, BCC, attachments, an option to specify if the body is HTML, and a reply-to address.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <param name="cc">A list of CC recipient email addresses.</param>
        /// <param name="bcc">A list of BCC recipient email addresses.</param>
        /// <param name="attachments">A list of file paths for attachments.</param>
        /// <param name="isBodyHtml">Indicates whether the body content is HTML.</param>
        /// <param name="replyTo">The reply-to email address.</param>
        void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc, List<string> attachments, bool isBodyHtml, string replyTo);
    }
}
