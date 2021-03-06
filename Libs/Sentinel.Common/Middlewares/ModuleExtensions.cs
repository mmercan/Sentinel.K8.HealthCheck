using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sentinel.Common.Middlewares
{
    public static class ModuleExtensions
    {
        // public static void DefineEndpoints(this IEndpointDefinition endpointDefinition, WebApplication app)
        // {
        //     endpointDefinition.DefineServices(app.Services);
        //     endpointDefinition.DefineEndpoints(app);
        // }

        public static void AddServiceDefinitions(this IServiceCollection services, ConfigurationManager configuration, params Type[] scanMarkers)
        {

            var endpointDefinitions = new List<IModule>();
            foreach (var marker in scanMarkers)
            {
                endpointDefinitions.AddRange(marker.Assembly.ExportedTypes
                .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(Activator.CreateInstance).Cast<IModule>()
                );
            }
            foreach (var endpointDefinition in endpointDefinitions)
            {
                endpointDefinition.RegisterServices(services, configuration);
            }
            services.AddSingleton(endpointDefinitions as IReadOnlyCollection<IModule>);
        }

        public static void UseEndpointDefinitions(this WebApplication app)
        {
            ILogger? logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("EndpointDefinitionExtensions");


            var definitions = app.Services.GetService<IReadOnlyCollection<IModule>>();
            if (definitions == null) return;
            foreach (var endpointDefinition in definitions)
            {
                if (logger != null)
                {
                    logger.LogInformation($"Defining endpoint {endpointDefinition.GetType().Name}");
                }

                endpointDefinition.MapEndpoints(app);
            }

        }
    }

}
