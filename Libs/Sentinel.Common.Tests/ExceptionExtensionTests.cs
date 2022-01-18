using System;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Common.Tests
{
    public class ExceptionExtensionTests
    {
        private ITestOutputHelper output;

        public ExceptionExtensionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestExceptionExtensionInnerException()
        {
            var q = new Exception("test", new Exception("test2"));
            var r = q.InnerException;

            var message = ExceptionExtension.MessageWithInnerException(q);
            Assert.Equal("test2", message);
        }

#pragma warning disable CS8604
        [Fact]
        public void TestExceptionExtensionwhenNull()
        {
            Exception? q = null;
            var message = ExceptionExtension.MessageWithInnerException(q);
            Assert.Equal(string.Empty, message);
        }


        [Fact]
        public void TestExceptionExtensionwhenExpecion()
        {
            var q = new Exception("test");
            var message = ExceptionExtension.MessageWithInnerException(q);
            Assert.Equal("test", message);
        }
    }
}