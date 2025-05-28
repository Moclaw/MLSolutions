
using Core.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Services.External.SmtpService
{
    public class SmtpServices : ISmtpServices
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress;

        public SmtpServices(IOptions<SmtpConfiguration> options)
        {
            _fromAddress = options.Value.FromEmail;
            _smtpClient = new SmtpClient(options.Value.Host)
            {
                Credentials = new System.Net.NetworkCredential(options.Value.UserName, options.Value.Password),
                Port = options.Value.Port,
                EnableSsl = options.Value.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = options.Value.UseDefaultCredentials
            };
        }

        public void SendEmail(string to, string subject, string body) => SendEmailInternal(to, subject, body, null, null, null, false, null);

        public void SendEmail(string to, string subject, string body, List<string> cc) => SendEmailInternal(to, subject, body, cc, null, null, false, null);

        public void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc) => SendEmailInternal(to, subject, body, cc, bcc, null, false, null);

        public void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc, List<string> attachments) => SendEmailInternal(to, subject, body, cc, bcc, attachments, false, null);

        public void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc, List<string> attachments, bool isBodyHtml) => SendEmailInternal(to, subject, body, cc, bcc, attachments, isBodyHtml, null);

        public void SendEmail(string to, string subject, string body, List<string> cc, List<string> bcc, List<string> attachments, bool isBodyHtml, string replyTo) => SendEmailInternal(to, subject, body, cc, bcc, attachments, isBodyHtml, replyTo);

        private void SendEmailInternal(
            string to,
            string subject,
            string body,
            List<string>? cc,
            List<string>? bcc,
            List<string>? attachments,
            bool isBodyHtml,
            string? replyTo)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_fromAddress);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isBodyHtml;

            if (cc != null)
            {
                foreach (var ccAddress in cc)
                {
                    if (!string.IsNullOrWhiteSpace(ccAddress))
                        message.CC.Add(ccAddress);
                }
            }

            if (bcc != null)
            {
                foreach (var bccAddress in bcc)
                {
                    if (!string.IsNullOrWhiteSpace(bccAddress))
                        message.Bcc.Add(bccAddress);
                }
            }

            if (attachments != null)
            {
                foreach (var filePath in attachments)
                {
                    if (!string.IsNullOrWhiteSpace(filePath))
                        message.Attachments.Add(new Attachment(filePath));
                }
            }

            if (!string.IsNullOrWhiteSpace(replyTo))
            {
                message.ReplyToList.Add(new MailAddress(replyTo));
            }

            _smtpClient.Send(message);
        }
    }
}
