using Sentinel.Models.K8s.LabelSelectors;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Collections.Generic;

namespace Sentinel.Models.Tests
{
    public class LabelSelectorsTests
    {

        private readonly ITestOutputHelper _output;

        public LabelSelectorsTests(ITestOutputHelper output) => _output = output;


        [Fact]
        public void EqualsSelectorShouldReturnString()
        {
            var name = new List<string> { "testname", "operator-deployment" };
            var sel1 = new EqualsSelector("operator-deployment", name.ToArray());
            _output.WriteLine("EqualsSelector" + sel1.ToExpression());

        }


        [Fact]
        public void ExistsSelectorShouldReturnString()
        {
            var sel1 = new ExistsSelector("operator-deployment");
            var exp = sel1.ToExpression();
            _output.WriteLine("ExistsSelector : " + exp);

        }


        [Fact]
        public void NotEqualsSelectorShouldReturnString()
        {
            var label = "operator-deployment";
            var values = new List<string> { "testname", "operator" };
            var sel1 = new NotEqualsSelector(label, values.ToArray());
            _output.WriteLine("NotEqualsSelector : " + sel1.ToExpression());

        }

        [Fact]
        public void NotExistsSelectorShouldReturnString()
        {
            var label = "operator-deployment";

            var sel1 = new NotExistsSelector(label);
            _output.WriteLine("NotExistsSelector : " + sel1.ToExpression());

        }
    }
}