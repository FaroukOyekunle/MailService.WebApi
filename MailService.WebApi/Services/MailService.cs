using MailKit.Security;
using MailKit.Net.Smtp;
using MailService.WebApi.Models;
using MailService.WebApi.Settings;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailService.WebApi.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        //The basic idea is to create an object of MimeMessage (a class from Mimekit )and send it using a SMTPClient instance (Mailkit).
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            //Line 3 – 6 Creates a new object of MimeMessage and adds in the Sender,
            //To Address and Subject to this object.
            //We will be filling the message related data (subject, body) from the mailRequest and the data we get from our JSON File.

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();

            //Line 3 – 6 Creates a new object of MimeMessage and adds in the Sender,
            //To Address and Subject to this object.
            //We will be filling the message related data (subject, body) from the mailRequest and the data we get from our JSON File.


            if (mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            //Line 24 – Here we the HTML part of the email from the Body property of the request.
            builder.HtmlBody = mailRequest.Body;

            //Line 25 – Finally, add the attachment and HTML Body to the Body of the Email.
            //Add the other required information.

            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            //Line 29 – Send the Message using the smpt’s SendMailAsync Method.

            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public async Task SendWelcomeEmailAsync(WelcomeRequest request)
        {
            string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WelcomeTemplate.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[username]", request.UserName).Replace("[email]", request.ToEmail);
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(request.ToEmail));
            email.Subject = $"Welcome {request.UserName}";
            var builder = new BodyBuilder();
            builder.HtmlBody = MailText;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}