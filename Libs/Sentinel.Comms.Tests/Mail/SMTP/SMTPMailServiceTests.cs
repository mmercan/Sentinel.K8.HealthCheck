using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sentinel.Comms.EMail;
using Sentinel.Comms.EMail.SMTP;
using Xunit;
using Xunit.Abstractions;


namespace Sentinel.Comms.Tests.Mail.SMTP
{

    public class SMTPMailServiceTests
    {
        private readonly ITestOutputHelper output;
        private readonly IEmailSenderService _emailSenderService;
        IConfiguration config;
        private readonly Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection;

        public SMTPMailServiceTests(ITestOutputHelper output)
        {
            this.output = output;
            NullLogger<SMTPMailService> logger = new NullLogger<SMTPMailService>();
            var myConfiguration = new Dictionary<string, string>
            {
                {"SMTP:Server","127.0.0.1"},
                {"SMTP:Port","8090"},
                {"SMTP:UserName","test12123@test.com"},
                {"SMTP:Password",""}
            };


            config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .AddEnvironmentVariables()
            .Build();

            serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            var provider = serviceCollection.BuildServiceProvider();

            var sMTPMailServiceSettings = new SMTPMailServiceSettings
            {
                Server = "127.0.0.1",
                Port = 8090,
                UserName = "test12123@test.com",
                Password = ""
            };

            var opts = Options.Create<SMTPMailServiceSettings>(sMTPMailServiceSettings);


            _emailSenderService = new SMTPMailService(opts, logger);
        }


        [Fact]
        public void SendSMTPEmail()
        {
            _emailSenderService.Send("test@test.com", "Hello", "This is a Message");

        }

        [Fact]
        public void CreateEmailServiceFromSMTP()
        {
            // SMTPMailServiceExtension.AddMailService()
            serviceCollection.AddMailService(config.GetSection("SMTP"));
            var sp = serviceCollection.BuildServiceProvider();
            var emailservice = sp.GetService<IEmailSenderService>();
            Assert.NotNull(emailservice);
            var tsk = emailservice?.Send("test@test.com", "Hello", "This is a Message");
            tsk?.Wait();
        }
    }
}