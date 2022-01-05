using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinel.Common.AuthServices;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthAppBuilderExtensions
    {
        public static IApplicationBuilder UseAllAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            return app.UseMiddleware<AllAuthenticationMiddleware>();
        }
    }
}