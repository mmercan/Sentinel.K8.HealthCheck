using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sentinel.Worker.Scheduler.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ProgramShouldStart()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(10 * 1000);


            Task.Run(cancellationToken: source.Token, action: () =>
             {
                 var args = new string[1] { "" };
                 Program.Main(args);
             });
        }
    }
}