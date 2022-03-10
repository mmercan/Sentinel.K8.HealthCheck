// using System;
// using System.Collections.Generic;
// using System.Security.Cryptography.X509Certificates;
// using Microsoft.Extensions.Configuration;
// using Sentinel.Common.HttpClientHelpers;
// using Xunit;
// using Xunit.Abstractions;

// namespace Sentinel.Common.Tests
// {
//     public class HttpClientCertificateHelpersTests
//     {


//         private ITestOutputHelper output;
//         private IConfiguration config;
//         public HttpClientCertificateHelpersTests(ITestOutputHelper output)
//         {
//             this.output = output;


//             var myConfiguration = new Dictionary<string, string>
//             {
//                 {"RequiredHeaders:0", "Internal"},
//                 {"RequiredHeaders:1", "External"},
//                 {"CertThumbprint","4B1C775072024EECBFE13957251661F1D82A1589"},
//                 {"CertBase64","MIICYzCCAcygAwIBAgIBADANBgkqhkiG9w0BAQUFADAuMQswCQYDVQQGEwJVUzEMMAoGA1UEChMDSUJNMREwDwYDVQQLEwhMb2NhbCBDQTAeFw05OTEyMjIwNTAwMDBaFw0wMDEyMjMwNDU5NTlaMC4xCzAJBgNVBAYTAlVTMQwwCgYDVQQKEwNJQk0xETAPBgNVBAsTCExvY2FsIENBMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQD2bZEo7xGaX2/0GHkrNFZvlxBou9v1Jmt/PDiTMPve8r9FeJAQ0QdvFST/0JPQYD20rH0bimdDLgNdNynmyRoS2S/IInfpmf69iyc2G0TPyRvmHIiOZbdCd+YBHQi1adkj17NDcWj6S14tVurFX73zx0sNoMS79q3tuXKrDsxeuwIDAQABo4GQMIGNMEsGCVUdDwGG+EIBDQQ+EzxHZW5lcmF0ZWQgYnkgdGhlIFNlY3VyZVdheSBTZWN1cml0eSBTZXJ2ZXIgZm9yIE9TLzM5MCAoUkFDRikwDgYDVR0PAQH/BAQDAgAGMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFJ3+ocRyCTJw067dLSwr/nalx6YMMA0GCSqGSIb3DQEBBQUAA4GBAMaQzt+zaj1GU77yzlr8iiMBXgdQrwsZZWJo5exnAucJAEYQZmOfyLiMD6oYq+ZnfvM0n8G/Y79q8nhwvuxpYOnRSAXFp6xSkrIOeZtJMY1h00LKp/JX3Ng1svZ2agE126JHsQ0bhzN5TKsYfbwfTwfjdWAGy6Vf1nYi/rO+ryMO"},
//                 {"CertConString","StoreName=My;StoreLocation=CurrentUser;FindType=FindByThumbprint;FindValue=4B1C775072024EECBFE13957251661F1D82A1589;AllowInvalidClientCertificates=true"}
//             };


//             config = new ConfigurationBuilder()
//             .AddInMemoryCollection(myConfiguration)
//             .Build();


//             using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
//             {
//                 store.Open(OpenFlags.ReadWrite);
//                 X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(config["CertBase64"]));
//                 store.Add(cert);
//                 store.Close();
//             }
//         }


//         [Fact]
//         public void GetCertFromcertThumbprintShouldReturn()
//         {
//             Environment.SetEnvironmentVariable("CertThumbprint", config["CertThumbprint"]);
//             var cert = HttpClientCertificateHelpers.GetCertFromcertThumbprint();
//             Assert.NotNull(cert);


//             var base64cert = HttpClientCertificateHelpers.GetCertFromcertBase64(config["CertBase64"]);
//             Assert.NotNull(base64cert);

//             var cert_1 = HttpClientCertificateHelpers.FindCert(config["CertThumbprint"]);
//             Assert.NotNull(cert_1);

//             var cert_2 = HttpClientCertificateHelpers.FindCert(config["CertConString"]);
//         }

//         [Fact]
//         public void FindCertFromConfigKeyShuldworkWithCertThumbprint()
//         {
//             var cert = HttpClientCertificateHelpers.FindCertFromConfigKey(config, "CertThumbprint");
//             Assert.NotNull(cert);

//         }



//         [Fact]
//         public void FindCertFromConfigKeyShuldworkWithBase64()
//         {
//             var cert = HttpClientCertificateHelpers.FindCertFromConfigKey(config, "CertBase64");
//             Assert.NotNull(cert);
//         }


//         [Fact]
//         public void FindCertFromConfigKeyShuldworkWithConString()
//         {
//             var cert = HttpClientCertificateHelpers.FindCertFromConfigKey(config, "CertConString");
//             Assert.NotNull(cert);
//         }

//     }
// }