using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sentinel.Comms.EMail.SMTP
{
    public class SMTPMailService : IEmailSenderService
    {
        private readonly IOptions<SMTPMailServiceSettings> options;
        private ILogger<SMTPMailService> _logger;

        public SMTPMailService(IOptions<SMTPMailServiceSettings> options, ILogger<SMTPMailService> logger)
        {
            this.options = options;
            _logger = logger;
        }

        public Task Send(string to, string subject, string body)
        {

            if (options != null && options.Value != null && options.Value.Server != null)
            {
                _logger.LogDebug("mail server : " + options.Value.Server);
                var client = new SmtpClient(options.Value.Server, this.options.Value.Port);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(this.options.Value.UserName, this.options.Value.Password);
                // client.EnableSsl = true;

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(this.options.Value.UserName);
                mailMessage.To.Add(to);
                mailMessage.Body = body;
                mailMessage.Subject = subject;

                client.Send(mailMessage);
            }
            else
            {
                _logger.LogDebug("Error on Mail Server options");
                if (options == null) _logger.LogDebug("options Null");
                else if (options.Value == null) _logger.LogDebug("options.Value Null");
                else
                {
                    if (options.Value.Server == null) _logger.LogDebug("options.Value.Server Null");
                    if (options.Value.Port == 0) _logger.LogDebug("options.Value.Port 0");
                    if (options.Value.UserName == null) _logger.LogDebug("options.Value.Port Null");
                    if (options.Value.Password == null) _logger.LogDebug("options.Value.Port Null");
                    else _logger.LogDebug("I don't know what is Null ");
                }
            }

            return Task.CompletedTask;
        }
    }
}