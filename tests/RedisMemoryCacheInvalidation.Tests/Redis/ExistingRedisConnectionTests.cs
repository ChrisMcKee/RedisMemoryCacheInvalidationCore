﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using RedisMemoryCacheInvalidation.Redis;
using StackExchange.Redis;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Redis;

public class ExistingRedisConnectionTests
{
    private Mock<IConnectionMultiplexer> mockOfMux;
    private Mock<ISubscriber> mockOfSubscriber;
    private IRedisConnection cnx;
    private Fixture fixture = new Fixture();

    public ExistingRedisConnectionTests()
    {
        //mock of subscriber
        mockOfSubscriber = new Mock<ISubscriber>();
        mockOfSubscriber.Setup(s => s.UnsubscribeAll(It.IsAny<CommandFlags>()));
        mockOfSubscriber.Setup(s => s.Subscribe(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>(), It.IsAny<CommandFlags>()));
        mockOfSubscriber.Setup(s => s.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>())).ReturnsAsync(10L);
        //mock of mux
        mockOfMux = new Mock<IConnectionMultiplexer>();
        mockOfMux.Setup(c => c.IsConnected).Returns(true);
        mockOfMux.Setup(c => c.Close(false));
        mockOfMux.Setup(c => c.GetSubscriber(It.IsAny<object>())).Returns(this.mockOfSubscriber.Object);

        cnx = new ExistingRedisConnection(mockOfMux.Object);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public async Task WhenNotConnected_ShouldDoNothing()
    {
        this.mockOfMux.Setup(c => c.IsConnected).Returns(false);

        //connected
        var connected = cnx.Connect();
        Assert.False(connected);

        //subscribe
        cnx.Subscribe("channel", (c, v) => { });

        //getconfig
        var config = await cnx.GetConfigAsync();
        Assert.Equal([], config);

        //publish
        var published = await cnx.PublishAsync("channel", "value");
        Assert.Equal(0L, published);

        cnx.UnsubscribeAll();
        cnx.Disconnect();

        mockOfMux.Verify(c => c.IsConnected, Times.AtLeastOnce);
        mockOfMux.Verify(c => c.GetSubscriber(null), Times.Never);
        mockOfMux.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenConnect_ShouldCheck_MuxIsConnected()
    {
        var connected = cnx.Connect();
        Assert.True(connected);

        mockOfMux.Verify(c => c.IsConnected, Times.Once);

        Assert.True(cnx.IsConnected);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenDisconnect_ShouldUnsubscribeAll()
    {
        cnx.Disconnect();

        mockOfSubscriber.Verify(m => m.UnsubscribeAll(It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public async Task WhenPublishAsync_ShouldPublishMessages()
    {
        var channel = fixture.Create<string>();
        var value = fixture.Create<string>();

        var published = await cnx.PublishAsync(channel, value);

        Assert.Equal(10L, published);
        mockOfSubscriber.Verify(m => m.PublishAsync(RedisChannel.Literal(channel), value, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenSubscribe_ShouldSubscribe()
    {
        var channel = fixture.Create<string>();
        Action<RedisChannel, RedisValue> action = (c, v) => { };

        cnx.Subscribe(channel, action);

        mockOfSubscriber.Verify(s => s.Subscribe(RedisChannel.Literal(channel), action, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenUnsubscribeAll_ShouldUnsubscribeAll()
    {
        cnx.UnsubscribeAll();

        mockOfSubscriber.Verify(s => s.UnsubscribeAll(It.IsAny<CommandFlags>()), Times.Once);
    }
}
