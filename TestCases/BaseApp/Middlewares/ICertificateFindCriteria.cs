using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BaseApp.Middlewares
{
    public interface ICertificateFindCriteria
    {
        string StoreName { get; set; }
        StoreLocation StoreLocation { get; set; }
        X509FindType FindType { get; }
        object FindValue { get; }
        bool ValidOnly { get; set; }
        void SetFindOptions(X509FindType findType, object findValue);
    }
}