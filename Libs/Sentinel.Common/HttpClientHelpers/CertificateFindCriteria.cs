using System.Security.Cryptography.X509Certificates;

namespace Sentinel.Common.HttpClientHelpers
{
    public class CertificateFindCriteria : ICertificateFindCriteria
    {

        private X509FindType _findType;
        private object _findValue;



        public CertificateFindCriteria()
        {
            _findType = X509FindType.FindByThumbprint;
        }

        public CertificateFindCriteria(string thumbprint)
        {
            SetFindOptions(X509FindType.FindByThumbprint, thumbprint);
        }

        public CertificateFindCriteria(X509FindType findType, object findValue)
        {
            SetFindOptions(findType, findValue);
        }

        public string StoreName { get; set; } = "My";

        public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;

        public X509FindType FindType
        {
            get { return _findType; }
        }

        public object FindValue
        {
            get { return _findValue; }
        }

        public bool ValidOnly { get; set; } = false;

        public void SetFindOptions(X509FindType findType, object findValue)
        {
            _findType = findType;
            _findValue = findValue;
        }

        public override string ToString()
        {
            return $"{StoreName} StoreLocation, {_findType} {_findValue}";
        }


        public virtual X509Certificate2? FindCertificate()
        {

            using (var store = new X509Store(StoreName, StoreLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certificates = store.Certificates.Find(FindType, FindValue, ValidOnly);
                if (certificates == null || certificates.Count == 0)
                {
                    string validText = ValidOnly ? "valid " : null;
                    //throw new Exception($"Unable to find {validText} certificate {this.ToString()}");
                    return null;
                }
                return certificates[0];
            }
        }

        public static CertificateFindCriteria CertificateFindCriteriaFromConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            var findCriteria = new CertificateFindCriteria();
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0].ToLower())
                    {
                        case "storename":
                            findCriteria.StoreName = keyValue[1];
                            break;
                        case "storelocation":
                            findCriteria.StoreLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), keyValue[1]);
                            break;
                        case "findtype":
                            findCriteria.SetFindOptions((X509FindType)Enum.Parse(typeof(X509FindType), keyValue[1]), null);
                            break;
                        case "findvalue":
                            findCriteria.SetFindOptions(findCriteria.FindType, keyValue[1]);
                            break;
                        case "allowinvalidclientcertificates":
                            bool validOnly;
                            bool.TryParse(keyValue[1], out validOnly);
                            findCriteria.ValidOnly = !validOnly;
                            break;
                    }
                }

            }
            return findCriteria;
        }
    }
}