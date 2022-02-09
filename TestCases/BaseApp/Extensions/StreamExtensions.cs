namespace System.IO
{
    public static class StreamExtensions
    {
        public static byte[]? ToByteArray(this Stream input)
        {
            if (input == null)
                return null;

            const int bufferSize = 16 * 1024;

            var buffer = new byte[bufferSize];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
