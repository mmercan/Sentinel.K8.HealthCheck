using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Sentinel.Common.HttpClientHelpers
{
    public static class HttpClientCertificateHelpers
    {
        public static X509Certificate2? GetCertFromcertThumbprint()
        {
            var certThumbprint = Environment.GetEnvironmentVariable("CertThumbprint");
            if (string.IsNullOrEmpty(certThumbprint))
            {
                return null;
            }
            var clientCert = FindCert(certThumbprint);

            return clientCert;
        }


        public static X509Certificate2? FindCertFromConfigKey(IConfiguration configuration, string certKey)
        {
            var certValue = configuration[certKey];
            if (certValue == null)
                return null;
            return FindCertFromString(certValue);
        }

        public static X509Certificate2? FindCertFromString(string certString)
        {
            if (certString.Length == 40)
            {
                var cert = FindCert(certString);
                if (cert == null)
                {
                    throw new Exception("Certificate not found");
                }
                return cert;
            }
            else if (certString.Contains("StoreName"))
            {

                var certCritearia = CertificateFindCriteria.CertificateFindCriteriaFromConnectionString(certString);
                var cert = certCritearia.FindCertificate();

                if (cert == null)
                {
                    throw new Exception("Certificate not found : " + certCritearia.ToString());
                }
                return cert;
            }
            else
            {
                return GetCertFromcertBase64(certString);
            }
        }

        public static X509Certificate2? GetCertFromcertBase64(string cert64)
        {

            var certbyte = Convert.FromBase64String(cert64);
            X509Certificate2 cert = new X509Certificate2(certbyte);

            return cert;
        }

        public static X509Certificate2? FindCert(string thumbprint)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var clientCert = FindCert(store, thumbprint);
            store.Dispose();
            return clientCert;
        }

        public static X509Certificate2? FindCert(X509Store store, string thumbprint)
        {
            return store.Certificates.OfType<X509Certificate2>().FirstOrDefault(x =>
                 x.Thumbprint.Equals(thumbprint, StringComparison.CurrentCultureIgnoreCase));

            // foreach (var cert in store.Certificates)
            //     if (cert.Thumbprint.Equals(thumbprint, StringComparison.CurrentCultureIgnoreCase))
            //         return cert;
            // return null;
        }


    }
}