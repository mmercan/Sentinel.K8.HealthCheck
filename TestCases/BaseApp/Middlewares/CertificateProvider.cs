using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace BaseApp.Middlewares
{
    public class CertificateProvider : ICertificateProvider
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger _logger;

        public CertificateProvider(IConfiguration configuration)
            : this(configuration, null)
        {
        }

        public CertificateProvider(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public virtual X509Certificate2 FindCertificate(ICertificateFindCriteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            using (var store = new X509Store(criteria.StoreName, criteria.StoreLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certificates = store.Certificates.Find(criteria.FindType, criteria.FindValue, criteria.ValidOnly);
                if (certificates == null || certificates.Count == 0)
                {
                    string validText = criteria.ValidOnly ? "valid " : null;
                    LogOrThrowError($"Unable to find {validText}certificate {criteria}");
                    return null;
                }
                return certificates[0];
            }
        }

        public virtual X509Certificate2 GetCertificateByThumbprint(string storeName, string thumbprint, bool validOnly = true)
        {
            return FindCertificate(storeName, StoreLocation.LocalMachine, X509FindType.FindByThumbprint, thumbprint, validOnly);
        }

        public virtual X509Certificate2 GetCertificateByThumbprint(string storeName, StoreLocation storeLocation, string thumbprint, bool validOnly = true)
        {
            return FindCertificate(storeName, storeLocation, X509FindType.FindByThumbprint, thumbprint, validOnly);
        }

        public virtual X509Certificate2 FindCertificate(string storeName, X509FindType findType, object findValue, bool validOnly = true)
        {
            return FindCertificate(storeName, StoreLocation.LocalMachine, X509FindType.FindByThumbprint, findValue, validOnly);
        }

        public virtual X509Certificate2 FindCertificate(string storeName, StoreLocation storeLocation, X509FindType findType, object findValue, bool validOnly = true)
        {
            var criteria = new CertificateFindCriteria(findType, findValue)
            {
                StoreName = storeName,
                StoreLocation = storeLocation,
                ValidOnly = validOnly
            };

            return FindCertificate(criteria);
        }

        public virtual X509Certificate2 GetCertificateFromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return new X509Certificate2(fileName);
        }

        public virtual X509Certificate2 GetCertificateFromFile(string fileName, string password)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return new X509Certificate2(fileName, password);
        }

        public virtual X509Certificate2 GetCertificateFromFile(string fileName, SecureString password)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return new X509Certificate2(fileName, password);
        }

        public virtual X509Certificate2 GetCertificateFromCertFile(string fileName)
        {
            X509Certificate certificate = X509Certificate.CreateFromCertFile(fileName);
            if (certificate == null)
            {
                LogOrThrowError($"Unable to open Cert file {fileName}");
                return null;
            }
            X509Certificate2 result = certificate as X509Certificate2;
            if (result == null)
                LogOrThrowError($"Unable to create X509Certificate2 from Cert file {fileName}");

            return result;
        }

        public virtual X509Certificate2 GetCertificateFromSignedFile(string fileName)
        {
            X509Certificate certificate = X509Certificate.CreateFromSignedFile(fileName);
            if (certificate == null)
            {
                LogOrThrowError($"Unable to open Signed file {fileName}");
                return null;
            }
            X509Certificate2 result = certificate as X509Certificate2;
            if (result == null)
                LogOrThrowError($"Unable to create X509Certificate2 from Signed file {fileName}");

            return result;
        }

        protected void LogOrThrowError(string message)
        {
            if (_logger == null)
                throw new SecurityException(message);

            _logger.LogError(message);
        }

        public virtual X509Certificate2 ReadCertificate(Stream stream)
        {
            if (stream == null)
                return null;

            return new X509Certificate2(stream.ToByteArray());
        }

        public virtual X509Certificate2 ReadCertificate(Stream stream, string password)
        {
            if (stream == null)
                return null;

            return new X509Certificate2(stream.ToByteArray(), password);
        }

        public virtual X509Certificate2 ReadCertificate(Stream stream, SecureString password)
        {
            if (stream == null)
                return null;

            return new X509Certificate2(stream.ToByteArray(), password);
        }

        public virtual async Task<X509Certificate2> ReadCertificateAsync(Func<Task<Stream>> streamFunc, string name)
        {
            return ReadCertificate(await streamFunc().ConfigureAwait(false));
        }

        public virtual async Task<X509Certificate2> ReadCertificateAsync(Func<Task<Stream>> streamFunc, string password, string name)
        {
            return ReadCertificate(await streamFunc().ConfigureAwait(false), password);
        }

        public virtual async Task<X509Certificate2> ReadCertificateAsync(Func<Task<Stream>> streamFunc, SecureString password, string name)
        {
            return ReadCertificate(await streamFunc().ConfigureAwait(false), password);
        }

        public virtual X509Certificate2 FromBase64String(string rawString)
        {
            return new X509Certificate2(Convert.FromBase64String(rawString));
        }

        public virtual X509Certificate2 FromBase64String(string rawString, string password)
        {
            return new X509Certificate2(Convert.FromBase64String(rawString), password);
        }

        public virtual X509Certificate2 FromBase64String(string rawString, SecureString password)
        {
            return new X509Certificate2(Convert.FromBase64String(rawString), password);
        }

        public virtual X509Certificate2 FromBase64String(Func<string> rawStringFunc, string name)
        {
            return FromBase64String(rawStringFunc?.Invoke());
        }

        public virtual X509Certificate2 FromBase64String(Func<string> rawStringFunc, string password, string name)
        {
            return FromBase64String(rawStringFunc?.Invoke(), password);
        }

        public virtual X509Certificate2 FromBase64String(Func<string> rawStringFunc, SecureString password, string name)
        {
            return FromBase64String(rawStringFunc?.Invoke(), password);
        }

        public virtual async Task<X509Certificate2> FromBase64StringAsync(Func<Task<string>> rawStringAsyncFunc, string name)
        {
            return FromBase64String(await rawStringAsyncFunc().ConfigureAwait(false));
        }

        public virtual async Task<X509Certificate2> FromBase64StringAsync(Func<Task<string>> rawStringAsyncFunc, string password, string name)
        {
            return FromBase64String(await rawStringAsyncFunc?.Invoke(), password);
        }

        public virtual async Task<X509Certificate2> FromBase64StringAsync(Func<Task<string>> rawStringAsyncFunc, SecureString password, string name)
        {
            return FromBase64String(await rawStringAsyncFunc?.Invoke(), password);
        }

        public virtual X509Certificate2 FromConfiguration(string key)
        {
            return FromBase64String(() => _configuration?[key], key);
        }

        public virtual async Task<X509Certificate2> FromConfigurationAsync(string key)
        {
            return await FromBase64StringAsync(() => GetBase64CertificateFromConfigurationAsync(key), key).ConfigureAwait(false);
        }

        public virtual async Task<X509Certificate2> FromConfigurationAsync(string key, string password)
        {
            return await FromBase64StringAsync(() => GetBase64CertificateFromConfigurationAsync(key), password, key).ConfigureAwait(false);
        }

        public virtual async Task<X509Certificate2> FromConfigurationAsync(string key, SecureString password)
        {
            return await FromBase64StringAsync(() => GetBase64CertificateFromConfigurationAsync(key), password, key).ConfigureAwait(false);
        }

        protected virtual Task<string> GetBase64CertificateFromConfigurationAsync(string key)
        {
            if (_configuration == null)
                return null;

            string base64Cert = _configuration[key];

            return Task.FromResult(base64Cert); //? await _configuration.GetSecretAsync(key).ConfigureAwait(false) : base64Cert;
        }
    }
}