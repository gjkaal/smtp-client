using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nice2Experience.Smtp.Extensions;
using Nice2Experience.Smtp.Models;

namespace Nice2Experience.Smtp
{

    public class EmailServiceClient : IEmailService
    {
        private readonly SmtpSettings _smtp;
        private readonly ILogger _log;
        private readonly IMimeMappingService _mimeMapper;

        public EmailServiceClient(IConfiguration configuration, ILoggerFactory loggerFactory, IMimeMappingService mimeMapper)
        {
            _mimeMapper = mimeMapper;
            _smtp = new SmtpSettings();
            configuration.GetSection("SMTP").Bind(_smtp);

            _log = loggerFactory != null
                ? loggerFactory.CreateLogger(nameof(EmailServiceClient))
                : null;

            _log.LogTrace($"Email config: {JsonConvert.SerializeObject(_smtp, Formatting.Indented)}");
        }

        public async Task<(string message, bool success)> SendMailContents(
           string toMailAddress,
           string toName,
           string subject,
           string htmlMessageContent,
           string style,
           params IFileAttachment[] files)
        {
            return await SendMailContents(_smtp.Sender, _smtp.FriendlyName, toMailAddress, toName, subject, htmlMessageContent, style, files);
        }

        public async Task<(string message, bool success)> SendMailContents(
            string replyToName,
            string toMailAddress,
            string toName,
            string subject,
            string htmlMessageContent,
            string style,
            params IFileAttachment[] files)
        {
            return await SendMailContents(_smtp.Sender, replyToName, toMailAddress, toName, subject, htmlMessageContent, style, files);
        }

        public async Task<(string message, bool success)> SendMailContents(
        string replyToAddress,
        string replyToName,
        string toMailAddress,
        string toName,
        string subject,
        string htmlMessageContent,
        string style,
        params IFileAttachment[] files)
        {
            var log = new StringBuilder();
            log.AppendLine($"Send email subject:'{subject}', replyTo:[{replyToAddress} {replyToName}], to:[{toMailAddress} {toName}]");
            try
            {
                var from = new MailAddress(_smtp.Sender, _smtp.FriendlyName, Encoding.UTF8);
                var to = new MailAddress(toMailAddress, toName, Encoding.UTF8);
                var mailMessage = new MailMessage(from, to);
                mailMessage.Subject = subject;
                mailMessage.SubjectEncoding = Encoding.UTF8;
                var isHtml = htmlMessageContent.Contains("<");
                if (isHtml)
                {
                    if (string.IsNullOrEmpty(style))
                    {
                        mailMessage.Body = $"<html><body>{htmlMessageContent}</body></html>";
                    }
                    else
                    {
                        mailMessage.Body = $"<html><head><style>{style}</style></head><body>{htmlMessageContent}</body></html>";
                    }
                }
                else
                {
                    mailMessage.Body = htmlMessageContent;
                }

                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.ReplyToList.Add(new MailAddress(replyToAddress, replyToName));
                mailMessage.IsBodyHtml = isHtml;
                mailMessage.AttachFiles(files, _mimeMapper);

                AddSmtpDataToUserLog(log);
                var mailSent = false;

                if (_smtp.DebugTag != null)
                {
                    if (subject.ToUpperInvariant().Contains(_smtp.DebugTag.ToUpperInvariant()))
                    {
                        log.AppendLine($"Found debug tag :{_smtp.DebugTag} in subject, Sending email message to debug server only");
                        log.AppendLine("Sending email message to debug server");
                        log.AppendLine($"Using username {_smtp.DebugHost.UserName.Substring(0, 3)}....");
                        await SendMail(_smtp.DebugHost, mailMessage).ConfigureAwait(false); ;
                        mailSent = true;
                    }
                    else
                    {
                        if (_smtp.SendActive)
                        {
                            log.AppendLine("Sending email message");
                            await SendMail(_smtp.Host, mailMessage).ConfigureAwait(false); ;
                            mailSent = true;
                        }

                        if (_smtp.DebugActive)
                        {
                            log.AppendLine("Sending email message to debug server");
                            mailMessage.Subject = "[" + _smtp.DebugTag + "]" + mailMessage.Subject;
                            await SendMail(_smtp.DebugHost, mailMessage).ConfigureAwait(false); ;
                            mailSent = true;
                        }
                    }
                }
                else
                {
                    if (_smtp.SendActive)
                    {
                        log.AppendLine("Sending email message");
                        await SendMail(_smtp.Host, mailMessage).ConfigureAwait(false); ;
                        mailSent = true;
                    }

                    if (_smtp.DebugActive)
                    {
                        log.AppendLine("Sending email message to debug server");
                        mailMessage.Subject = "[DEBUG]" + mailMessage.Subject;
                        await SendMail(_smtp.DebugHost, mailMessage).ConfigureAwait(false); ;
                        mailSent = true;
                    }
                }

                if (mailSent) return (log.ToString(), true);

                log.AppendLine("Email validation succeeded");
                log.AppendLine("SendActive and DebugActive are both set to false in config.");
                log.AppendLine("The service is not transmitting messages");
                return (log.ToString(), true);
            }
            catch (Exception e)
            {
                log.AppendLine(e.Message);
                log.AppendLine("Check if the SMTP settings in the configuration are correct.");
                if (_log != null) _log.LogError(e.ToString());
                return (log.ToString(), false);
            }
        }

        private void AddSmtpDataToUserLog(StringBuilder log)
        {
            if (_smtp.Host != null)
            {
                log.AppendLine($"Smtp  host : {_smtp.Host.HostName}, port {_smtp.Host.Port}, ssl {_smtp.Host.EnableSsl}");
            }
            else
            {
                log.AppendLine($"Smtp host not found");
            }

            if (_smtp.DebugHost != null)
            {
                log.AppendLine($"Debug host : {_smtp.DebugHost.HostName}, port {_smtp.DebugHost.Port}, ssl {_smtp.DebugHost.EnableSsl}");
            }
            else
            {
                log.AppendLine($"Debug host not found");
            }
        }

        private static async Task<bool> SendMail(SmtpHost smtpHost, MailMessage mailMessage)
        {
            using (var client = new SmtpClient(smtpHost.HostName, smtpHost.Port))
            {
                client.Credentials = new NetworkCredential(smtpHost.UserName, smtpHost.Password);
                client.EnableSsl = smtpHost.EnableSsl;
                await client.SendMailAsync(mailMessage).ConfigureAwait(false); ;
                return true;
            }
        }
    }
}
