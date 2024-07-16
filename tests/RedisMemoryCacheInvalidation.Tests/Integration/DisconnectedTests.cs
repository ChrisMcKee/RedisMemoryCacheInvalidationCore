using System;
using System.Threading.Tasks;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Integration
{
    public class DisconnectedTests
    {
        public DisconnectedTests()
        {
            InvalidationManager.NotificationBus = null;
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
        public async Task InvalidHost_ShouldNotBeConnected()
        {
            //test more disconnected scenarios
            InvalidationManager.Configure("blabblou", new InvalidationSettings());
            Assert.False(InvalidationManager.IsConnected);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await InvalidationManager.InvalidateAsync("mykey");
            });
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
        public async Task WhenNotConnected_ShouldNotPublishMessages()
        {
            //test more disconnected scenarios
            InvalidationManager.Configure("blabblou", new InvalidationSettings());

            var published = await InvalidationManager.InvalidateAsync("mykey");
            Assert.Equal(0L, published);
        }
    }
}
