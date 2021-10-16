using Sentinel.Scheduler.GeneralScheduler.Cron;

namespace Sentinel.Scheduler.Tests
{
    public class CrontabFieldTests
    {


        public void CrontabFieldShouldbeCreated()
        {
            var cr = CrontabField.Days("*");
            cr.Next(2);

            cr.GetFirst();
            cr.Contains(4);
            cr.ToString();
        }
    }
}