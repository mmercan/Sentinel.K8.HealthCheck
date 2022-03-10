using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApp.Middlewares
{
    public interface IValidateCertificateSettings : ICertificateFindCriteria
    {
        string Thumbprint { get; }
        string Subject { get; }
        string Issuer { get; }
        string SerialNumber { get; }
        bool VerifyChain { get; }
        bool IsValidationEnabled();
        bool EnableRevocationListCheck { get; }
    }
}