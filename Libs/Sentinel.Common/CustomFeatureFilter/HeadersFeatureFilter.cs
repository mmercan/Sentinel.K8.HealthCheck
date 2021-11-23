using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Sentinel.Common.CustomFeatureFilter
{
    [FilterAlias("Headers")] // How we will refer to the filter in configuration
    public class HeadersFeatureFilter : IFeatureFilter
    {
        // Used to access HttpContext
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HeadersFeatureFilter> _logger;
        private readonly string claimType = "";
        public HeadersFeatureFilter(IHttpContextAccessor httpContextAccessor, ILogger<HeadersFeatureFilter> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }



        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            return Task.Run(() =>
            {
                // Get the ClaimsFilterSettings from configuration
                var settings = context.Parameters.Get<HeadersFilterSettings>();

                // Retrieve the current user (ClaimsPrincipal)
                var user = _httpContextAccessor?.HttpContext?.User;

                var headerKeys = _httpContextAccessor?.HttpContext?.Request.Headers.Keys.ToList();

                // Only enable the feature if the user has ALL the required claims
                var isEnabled = false;
                if (headerKeys != null && settings != null && settings.RequiredHeaders != null)
                {
                    isEnabled = settings.RequiredHeaders
                       .All(header => headerKeys.Contains(header));
                }
                else
                {
                    _logger.LogDebug("HeadersFeatureFilter Null parameter");
                }
                var hasclaim = user?.HasClaim(claim => claim.Type == claimType);
                _logger.LogDebug("hasclaim :" + hasclaim.ToString() + " " + user?.Identity?.Name + " isEnabled : " + isEnabled.ToString() + " " + headerKeys?.FirstOrDefault());

                return isEnabled;
            });
        }
    }
}