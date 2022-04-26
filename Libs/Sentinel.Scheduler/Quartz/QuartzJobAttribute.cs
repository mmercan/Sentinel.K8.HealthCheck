using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Quartz
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QuartzJobAttribute : Attribute
    {
        /// <summary>
        ///     Configration section for the Job
        /// </summary>
        public string ConfigurationSection { get; set; } = default!;
        /// <summary>
        /// The name element for the Job's JobKey. default is the class name
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// The group element for the Job's JobKey.
        /// </summary>
        public string Group { get; set; } = default!;

        /// <summary>
        /// The given (human-meaningful) description of the Job.
        /// </summary>
        public string Description { get; set; } = default!;

        /// <summary>
        /// The CRON expression that defines when the Trigger is fired.
        /// </summary>
        public string CronExpression { get; set; } = default!;

        /// <summary>
        /// The Trigger's priority. When more than one Trigger have the same fire time, the scheduler will fire the one with the highest priority first.
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        /// Whether or not the job should remain stored after it is orphaned. (default: false)
        /// </summary>
        public bool StoreDurably { get; set; } = true;

        /// <summary>
        /// Instructs the <see cref="Quartz.IScheduler"/> whether or not the job should be re-executed if a 'recovery' or 'fail-over' situation is encountered. (default: false)
        /// </summary>
        public bool RequestRecovery { get; set; } = default!;

        /// <summary>
        /// Is Job Enabled or not. (default: true)
        /// </summary>
        public bool Enabled { get; set; } = true;


        public int DelaySecond { get; set; } = 10;
        /// <summary>
        /// 
        /// </summary>
        public QuartzJobAttribute()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cron"></param>
        public QuartzJobAttribute(string cron)
        {
            CronExpression = cron;
        }




    }
}