using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Comms.EMail;
using Sentinel.Comms.EMail.SMTP;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SMTPMailServiceExtension
    {
        public static IServiceCollection AddMailService(
       this IServiceCollection serviceCollection,
       SMTPMailServiceSettings options)
        {
            serviceCollection.Configure<SMTPMailServiceSettings>(o => o = options);
            serviceCollection.AddSingleton<IEmailSenderService, SMTPMailService>();
            return serviceCollection;
        }



        public static IServiceCollection AddMailService(
        this IServiceCollection serviceCollection,
       IConfiguration options)
        {
            serviceCollection.Configure<SMTPMailServiceSettings>(options);
            serviceCollection.AddSingleton<IEmailSenderService, SMTPMailService>();
            return serviceCollection;
        }

    }
}