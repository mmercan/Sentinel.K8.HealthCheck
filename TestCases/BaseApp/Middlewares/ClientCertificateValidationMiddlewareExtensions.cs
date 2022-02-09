using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApp.Middlewares
{
    public static class ClientCertificateValidationMiddlewareExtensions
    {
        /// <summary>
        /// Adds the client certificate validation middleware to the AspNet Core pipeline.
        /// Expects options/settings of type ValidateCertificateSettings to be specified in application configuration.
        /// The concrete type ClientCertificateMiddleware is used.
        /// </summary>       
        public static IApplicationBuilder UseClientCertificateValidationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClientCertificateValidationMiddleware>();
        }

        /// <summary>
        /// Adds the client certificate validation middleware to the AspNet Core pipeline. A custom certificate provider
        /// can be specified to provide the certificate against which the client certificate is validated.        ///  
        /// Expects options/settings of type ValidateCertificateSettings to be specified in application configuration.
        /// The concrete type ClientCertificateMiddleware is used.
        /// </summary>  
        public static IApplicationBuilder UseClientCertificateValidationMiddleware(this IApplicationBuilder builder,
            ICertificateProvider compareCertificateProvider)
        {
            return builder.UseMiddleware<ClientCertificateValidationMiddleware>(compareCertificateProvider);
        }
    }
}