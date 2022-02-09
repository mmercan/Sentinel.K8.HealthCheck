using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace BaseApp.Middlewares
{
    public class ClientCertificateValidationMiddleware
    {
        private readonly RequestDelegate _next = null;
        private readonly ILogger _logger = null;
        private readonly ValidateCertificateSettings _options = null;
        private readonly ICertificateProvider _compareCertificateProvider = null;
        private readonly IValidateCertificate _validateCertificate = null;

        /// <summary>
        /// Default constructor
        /// </summary>        
        public ClientCertificateValidationMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            IOptions<ValidateCertificateSettings> options)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger("ClientCertMiddleware");
            _options = options?.Value;
            _validateCertificate = new ValidateCertificate(_options, _compareCertificateProvider, null);
        }

        /// <summary>
        /// Constructor with comparison certificate provider
        /// </summary>        
        public ClientCertificateValidationMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            IOptions<ValidateCertificateSettings> options,
            ICertificateProvider compareCertificateProvider)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger("ClientCertMiddleware");
            _options = options?.Value;
            _compareCertificateProvider = compareCertificateProvider;
            _validateCertificate = new ValidateCertificate(_options, _compareCertificateProvider, null);
        }

        /// <summary>
        /// Invoke middleware
        /// </summary>       
        public async Task Invoke(HttpContext context)
        {
            if (_options == null || !_options.IsValidationEnabled())
            {
                await _next?.Invoke(context);
            }
            else
            {
                //Validate the cert 
                bool isValidCert = false;
                X509Certificate2? certificate = null;

                string certHeader = context.Request.Headers["X-ARR-ClientCert"];

                if (!String.IsNullOrEmpty(certHeader))
                {
                    try
                    {
                        byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                        certificate = new X509Certificate2(clientCertBytes);

                        isValidCert = _validateCertificate.IsValidCertificate(certificate);
                         
                        {
                            _logger?.LogInformation("Certificate with thumbprint " + certificate.Thumbprint + " is not valid");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex.Message, ex);
                    }
                }
                else
                {
                    _logger?.LogDebug($"'X-ARR-ClientCert' header is missing");
                }

                if (isValidCert)
                {
                    await _next?.Invoke(context);
                }
                else
                {
                    context.Response.StatusCode = 403;
                }
            }
        }
    }
}