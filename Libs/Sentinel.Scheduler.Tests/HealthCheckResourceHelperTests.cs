using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler.Helpers;
using Sentinel.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Scheduler.Tests
{
    public class HealthCheckResourceHelperTests
    {
        private ITestOutputHelper output;
        private Redis.RedisDictionary<ServiceV1> serviceRedisDic;

        public HealthCheckResourceHelperTests(ITestOutputHelper output)
        {
            this.output = output;
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<Redis.RedisDictionary<ServiceV1>>();
            var rediscon = RedisExtensions.GetRedisMultiplexer();
            serviceRedisDic = new Redis.RedisDictionary<ServiceV1>(rediscon, logger, "Services");
        }


        [Fact]
        public void HealthCheckResourceHelperShouldFindServiceWithlongFormat()
        {

            var hc = new HealthCheckResourceV1 { Schedule = "* * * * *", Name = "test", Namespace = "default" };
            hc.Spec = new HealthCheckResourceSpecV1 { Service = "kubernetes.default.svc.cluster.local" };
            var service = HealthCheckResourceHelper.FindServiceRelatedtoHealthCheckResourceV1(hc, serviceRedisDic);
            Assert.NotNull(service);
        }

        [Fact]
        public void HealthCheckResourceHelperShouldFindServiceShortFormat()
        {
            var hc = new HealthCheckResourceV1 { Schedule = "* * * * *", Name = "test", Namespace = "default" };
            hc.Spec = new HealthCheckResourceSpecV1 { Service = "kubernetes", Cert = "68A1711EFC66EEA676F8B165102D94697DEE342F" };
            var service = HealthCheckResourceHelper.FindServiceRelatedtoHealthCheckResourceV1(hc, serviceRedisDic);
            Assert.NotNull(service);
        }

        [Fact]
        public void HealthCheckResourceHelperShouldThrowIfServiceNull()
        {
            var hc = new HealthCheckResourceV1 { Schedule = "* * * * *", Name = "test", Namespace = "default" };

            Assert.Throws<ArgumentNullException>(() =>
            HealthCheckResourceHelper.FindServiceRelatedtoHealthCheckResourceV1(hc, serviceRedisDic)
            );
        }
    }
}