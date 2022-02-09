namespace System.Text
{
    using System.Security;
    using System.Text;

    public static class StringExtension
    {
        private static readonly Encoding Encoding = new UTF8Encoding();

        public static SecureString ToSecureString(this string plainText)
        {
            var secureString = new SecureString();
            foreach (var c in plainText)
            {
                secureString.AppendChar(c);
            }

            return secureString;
        }

        public static byte[]? ToByteArray(this string text)
        {
            if (text == null)
            {
                return null;
            }

            var textAsBytes = Encoding.GetBytes(text);
            return textAsBytes;
        }
    }
}