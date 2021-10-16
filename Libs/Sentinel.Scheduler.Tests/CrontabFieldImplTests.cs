using Sentinel.Scheduler.GeneralScheduler.Cron;
using Xunit.Abstractions;

namespace Sentinel.Scheduler.Tests
{
    public class CrontabFieldImplTests
    {


        private readonly ITestOutputHelper _output;

        public CrontabFieldImplTests(ITestOutputHelper output)
        {
            this._output = output;
        }
        public void CrontabFieldImplShouldCreateHour()
        {
            var imp = CrontabFieldImpl.Hour;


            CrontabFieldImpl.FromKind(CrontabFieldKind.Day);


            CrontabFieldImpl.FromKind(CrontabFieldKind.DayOfWeek);
        }
    }
}