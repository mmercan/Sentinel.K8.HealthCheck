using Microsoft.AspNetCore.Mvc.Testing;
using Sentinel.Tests.Helpers;
using Xunit;

namespace Sentinel.Worker.Sync.Tests.Helpers
{
    [CollectionDefinition("WebApplicationFactory")]
    public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactory<Program>>, ICollectionFixture<AuthTokenFixture>, ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}