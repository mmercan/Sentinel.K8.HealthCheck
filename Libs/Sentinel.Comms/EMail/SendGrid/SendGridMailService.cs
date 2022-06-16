using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Sentinel.Comms.EMail.SendGrid
{
    public class SendGridMailService : IEmailSenderService
    {
        private readonly IOptions<SendGridMailServiceSettings> options;
        private ILogger<SendGridMailService> _logger;

        public SendGridMailService(IOptions<SendGridMailServiceSettings> options, ILogger<SendGridMailService> logger)
        {
            this.options = options;
            _logger = logger;
        }

        public async Task Send(string to, string subject, string body)
        {

            if (options != null && options.Value != null && options.Value.Key != null && options.Value.From != null)
            {
                var apiKey = options.Value.Key;
                var client = new SendGridClient(apiKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(options.Value.From),
                    Subject = subject,
                    PlainTextContent = body
                };
                msg.AddTo(new EmailAddress(to));
                Response response = await client.SendEmailAsync(msg);
                _logger.LogDebug("SendGrid response: " + response?.StatusCode);
            }
            else
            {
                _logger.LogDebug("Error on Mail Server options");
                if (options == null) _logger.LogDebug("options Null");
                else if (options.Value == null) _logger.LogDebug("options.Value Null");
                else
                {
                    if (options?.Value?.Key == null) _logger.LogDebug("options.Value.Key Null");
                    if (options?.Value?.From == null) _logger.LogDebug("options.Value.From Null");
                }
            }
        }
    }
}