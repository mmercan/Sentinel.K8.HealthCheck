using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Common.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Sentinel.Mongo.Modules
{
    public class MongoServiceModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {

        }

        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {

        }
    }
}