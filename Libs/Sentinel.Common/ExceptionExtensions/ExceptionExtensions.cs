namespace Sentinel.Common
{
    public static class ExceptionExtension
    {
        public static string MessageWithInnerException(this Exception? ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            else if (ex.InnerException != null)
            {
                return ex.InnerException.Message;
            }
            else
            {
                return ex.Message;
            }
        }
    }
}