using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Common.Middlewares;
using Microsoft.AspNetCore.Builder;
namespace Sentinel.Mongo.ServiceDefinitions
{
    public class MongoServiceDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {

        }

        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {

        }
    }
}