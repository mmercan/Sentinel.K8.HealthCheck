using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Sentinel.Worker.Sync.Tests.Helpers
{
    [CollectionDefinition("WebApplicationFactory")]
    public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactory<Startup>>, ICollectionFixture<AuthTokenFixture>, ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}