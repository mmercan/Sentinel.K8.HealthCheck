using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApp.Middlewares
{
    public class ValidateCertificateSettings : CertificateFindCriteria, IValidateCertificateSettings
    {
        public string Issuer { get; set; }

        public string SerialNumber { get; set; }

        public string Subject { get; set; }

        public string Thumbprint { get; set; }

        public bool VerifyChain { get; set; }

        public bool IsValidationEnabled()
        {
            return !string.IsNullOrEmpty(Thumbprint) || FindValue != null || (!string.IsNullOrEmpty(Issuer) && !string.IsNullOrEmpty(SerialNumber));
        }

        public bool EnableRevocationListCheck { get; set; }
    }
}