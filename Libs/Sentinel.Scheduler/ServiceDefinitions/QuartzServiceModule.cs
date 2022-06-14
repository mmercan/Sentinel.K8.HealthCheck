using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Sentinel.Common.Middlewares;

namespace Libs.Sentinel.Scheduler.JobSchedules
{
    public class QuartzServiceModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {
            var options = new CrystalQuartzOptions
            {
                ErrorDetectionOptions = new CrystalQuartz.Application.ErrorDetectionOptions
                { VerbosityLevel = ErrorVerbosityLevel.Detailed }
            };
            var schedulerFactory = app.Services.GetService<ISchedulerFactory>();
            var scheduler = schedulerFactory?.GetScheduler().GetAwaiter().GetResult();
            app.UseCrystalQuartz(() => scheduler, options);
        }

        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });

            services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
                options.StartDelay = TimeSpan.FromSeconds(2);
            });
        }
    }
}