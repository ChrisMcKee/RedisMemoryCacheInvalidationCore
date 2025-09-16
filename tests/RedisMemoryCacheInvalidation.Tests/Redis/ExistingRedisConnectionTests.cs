using System;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using RedisMemoryCacheInvalidation.Redis;
using StackExchange.Redis;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Redis;

public class ExistingRedisConnectionTests
{
    private readonly Mock<IConnectionMultiplexer> _mockOfMux;
    private readonly Mock<ISubscriber> _mockOfSubscriber;
    private readonly IRedisConnection _cnx;
    private readonly Fixture _fixture = new Fixture();

    public ExistingRedisConnectionTests()
    {
        //mock of subscriber
        _mockOfSubscriber = new Mock<ISubscriber>();
        _mockOfSubscriber.Setup(s => s.UnsubscribeAll(It.IsAny<CommandFlags>()));
        _mockOfSubscriber.Setup(s => s.Subscribe(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>(), It.IsAny<CommandFlags>()));
        _mockOfSubscriber.Setup(s => s.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>())).ReturnsAsync(10L);
        //mock of mux
        _mockOfMux = new Mock<IConnectionMultiplexer>();
        _mockOfMux.Setup(c => c.IsConnected).Returns(true);
        _mockOfMux.Setup(c => c.Close(false));
        _mockOfMux.Setup(c => c.GetSubscriber(It.IsAny<object>())).Returns(this._mockOfSubscriber.Object);

        _cnx = new ExistingRedisConnection(_mockOfMux.Object);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public async Task WhenNotConnected_ShouldDoNothing()
    {
        this._mockOfMux.Setup(c => c.IsConnected).Returns(false);

        //connected
        var connected = _cnx.Connect();
        Assert.False(connected);

        //subscribe
        _cnx.Subscribe("channel", (c, v) => { });

        //getconfig
        var config = await _cnx.GetConfigAsync();
        Assert.Equal([], config);

        //publish
        var published = await _cnx.PublishAsync("channel", "value");
        Assert.Equal(0L, published);

        _cnx.UnsubscribeAll();
        _cnx.Disconnect();

        _mockOfMux.Verify(c => c.IsConnected, Times.AtLeastOnce);
        _mockOfMux.Verify(c => c.GetSubscriber(null), Times.Never);
        _mockOfMux.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenConnect_ShouldCheck_MuxIsConnected()
    {
        var connected = _cnx.Connect();
        Assert.True(connected);

        _mockOfMux.Verify(c => c.IsConnected, Times.Once);

        Assert.True(_cnx.IsConnected);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenDisconnect_ShouldUnsubscribeAll()
    {
        _cnx.Disconnect();

        _mockOfSubscriber.Verify(m => m.UnsubscribeAll(It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public async Task WhenPublishAsync_ShouldPublishMessages()
    {
        var channel = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        var published = await _cnx.PublishAsync(channel, value);

        Assert.Equal(10L, published);
        _mockOfSubscriber.Verify(m => m.PublishAsync(RedisChannel.Literal(channel), value, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenSubscribe_ShouldSubscribe()
    {
        var channel = _fixture.Create<string>();
        Action<RedisChannel, RedisValue> action = (c, v) => { };

        _cnx.Subscribe(channel, action);

        _mockOfSubscriber.Verify(s => s.Subscribe(RedisChannel.Literal(channel), action, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenUnsubscribeAll_ShouldUnsubscribeAll()
    {
        _cnx.UnsubscribeAll();

        _mockOfSubscriber.Verify(s => s.UnsubscribeAll(It.IsAny<CommandFlags>()), Times.Once);
    }
}
