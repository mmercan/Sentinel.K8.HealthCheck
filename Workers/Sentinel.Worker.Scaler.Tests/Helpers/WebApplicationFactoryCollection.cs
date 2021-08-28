using Microsoft.AspNetCore.Mvc.Testing;
using Sentinel.Tests.Helpers;
using Xunit;

namespace Sentinel.Worker.Scaler.Tests.Helpers
{
    [CollectionDefinition("WebApplicationFactory")]
    public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactory<Startup>>, ICollectionFixture<AuthTokenFixture>, ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}