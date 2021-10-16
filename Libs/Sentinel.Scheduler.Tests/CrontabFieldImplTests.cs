using Sentinel.Scheduler.GeneralScheduler.Cron;

namespace Sentinel.Scheduler.Tests
{
    public class CrontabFieldImplTests
    {
        public void CrontabFieldImplShouldCreateHour()
        {
            var imp = CrontabFieldImpl.Hour;


            CrontabFieldImpl.FromKind(CrontabFieldKind.Day);


            CrontabFieldImpl.FromKind(CrontabFieldKind.DayOfWeek);
        }
    }
}