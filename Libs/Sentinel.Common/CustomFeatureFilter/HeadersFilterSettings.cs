namespace Sentinel.Common.CustomFeatureFilter
{
    public class HeadersFilterSettings
    {
        public HeadersFilterSettings()
        {
            //RequiredHeaders = new string[1];
        }
        public string[]? RequiredHeaders { get; set; }
    }
}