using Sentinel.Scheduler.GeneralScheduler.Cron;
using Xunit;

namespace Sentinel.Scheduler.Tests
{
    public class CrontabFieldTests
    {

        [Fact]
        public void CrontabFieldShouldbeCreated()
        {
            var cr = CrontabField.Days("*");
            cr.Next(2);

            cr.GetFirst();
            cr.Contains(4);
            cr.ToString();
        }

        [Fact]
        public void CrontabFieldShouldbeCreatedNumber()
        {
            var cr = CrontabField.Days("2");
            cr.Next(2);

            cr.GetFirst();
            cr.Contains(4);
            cr.ToString();
        }

        [Fact]
        public void CrontabFieldShouldbeCreatedNumbers()
        {
            var cr = CrontabField.Days("1,2,3");
            cr.Next(2);

            cr.GetFirst();
            cr.Contains(4);
            cr.ToString();
        }


        [Fact]
        public void CrontabFieldShouldbeCreatedNumberRange()
        {
            var cr = CrontabField.Days("1-15");
            cr.Next(2);

            cr.GetFirst();
            cr.Contains(4);
            cr.ToString();
        }


        [Fact]
        public void CrontabFieldShouldbeCreatedNumberStep()
        {
            var cr = CrontabField.Days("*/30");
            cr.Next(2);

            cr.GetFirst();
            cr.Contains(4);
            cr.ToString();
        }
    }
}