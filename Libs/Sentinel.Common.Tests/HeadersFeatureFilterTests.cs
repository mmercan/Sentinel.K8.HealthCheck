using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Moq;
using Sentinel.Common.CustomFeatureFilter;
using Sentinel.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Common.Tests
{
    public class HeadersFeatureFilterTests
    {
        private ITestOutputHelper output;

        public HeadersFeatureFilterTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestFilter()
        {
            var logger = Helpers.GetLogger<HeadersFeatureFilter>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();

            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var myConfiguration = new Dictionary<string, string>
            {
                {"RequiredHeaders:0", "Internal"},
                {"RequiredHeaders:1", "External"},
            };


            IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

            FeatureFilterEvaluationContext conn = new FeatureFilterEvaluationContext { Parameters = config };


            // var config = new Mock<IConfiguration> { DefaultValue = DefaultValue.Mock, };
            // config.Setup(m => m.Get<HeadersFilterSettings>()).Returns(new HeadersFilterSettings());

            // var contextMoq = new Mock<FeatureFilterEvaluationContext> { DefaultValue = DefaultValue.Mock, };
            // contextMoq.Setup(m => m).Returns(config);

            HeadersFeatureFilter filter = new HeadersFeatureFilter(mockHttpContextAccessor.Object, logger);
            var resTask = filter.EvaluateAsync(conn);

            resTask.Wait();
            output.WriteLine("filter : " + resTask.Result.ToString());
            //filter.EvaluateAsync(contextMoq.Object);
            Assert.NotNull(resTask.Result);
        }
    }
}