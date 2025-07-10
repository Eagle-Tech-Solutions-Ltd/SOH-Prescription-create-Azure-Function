using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Helper
{
    public interface IEmailHelper
    {
        void SetCredentials(string host, int port, bool ssl, bool defaultCredentials, string userName, string password, string fromEmail);
        void To(string email);
        void Cc(string email);
        void Bcc(string email);
        void Subject(string subject);
        void Body(string body);
        void AddAttachment(Stream fileStream, string fileName, string mediaType);
        bool Send();
    }
    //public class EmailHelper : IEmailHelper
    public class EmailHelper
    {
        private MailMessage _mailMessage;
        private SmtpClient _smtpClient;

        public EmailHelper()
        {
            _mailMessage = new MailMessage();
            _smtpClient = new SmtpClient();
        }

        public void SetCredentials(string host, int port, bool ssl, bool defaultCredentials, string userName, string password, string fromEmail)
        {
            _mailMessage.From = new MailAddress(fromEmail);

            _smtpClient = new SmtpClient
            {
                Host = host,
                Port = port,
                EnableSsl = ssl,
                UseDefaultCredentials = defaultCredentials,
                Credentials = new NetworkCredential(userName, password)
            };
        }

        public void To(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
                _mailMessage.To.Add(new MailAddress(email));

            //_mailMessage.To.Add(new MailAddress("rgujjar@orbitsoft.co.uk"));
        }

        public void Cc(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
                _mailMessage.CC.Add(email);
        }

        public void Bcc(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
                _mailMessage.Bcc.Add(email);
        }

        public void Subject(string subject)
        {
            _mailMessage.Subject = subject;
        }

        public void Body(string body)
        {
            _mailMessage.Body = body;
            _mailMessage.IsBodyHtml = true;
        }

        public void AddAttachment(Stream fileStream, string fileName, string mediaType)
        {
            if (fileStream != null && !string.IsNullOrWhiteSpace(fileName))
            {
                var attachment = new Attachment(fileStream, fileName, mediaType ?? MediaTypeNames.Application.Octet);
                _mailMessage.Attachments.Add(attachment);
            }
        }

        public bool Send()
        {
            try
            {
                _smtpClient.Send(_mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                //_mailMessage.Dispose();
                //_smtpClient.Dispose();
            }
        }
    }
}
