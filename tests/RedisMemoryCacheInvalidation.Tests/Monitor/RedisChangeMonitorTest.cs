using System;
using AutoFixture;
using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Monitor;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Monitor;

public class RedisChangeMonitorTest
{
    private readonly string _notifKey;

    private readonly Fixture _fixture = new Fixture();
    private readonly Mock<INotificationManager<string>> _mockOfBus;
    private readonly Mock<IDisposable> _mockOfDispose;

    INotificationManager<string> Bus => _mockOfBus.Object;

    public RedisChangeMonitorTest()
    {
        _mockOfDispose = new Mock<IDisposable>();
        _mockOfBus = new Mock<INotificationManager<string>>();
        _mockOfBus.Setup(t => t.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Returns(this._mockOfDispose.Object);

        _notifKey = _fixture.Create<string>();
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenCtorWithoutBusBadArgs_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => { var monitor = new RedisChangeMonitor(null, _notifKey); });
        Assert.Throws<ArgumentNullException>(() => { var monitor = new RedisChangeMonitor(this.Bus, null); });
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenCtor_ShouldHaveUniqueId()
    {
        var monitor1 = new RedisChangeMonitor(this.Bus, _notifKey);
        Assert.NotNull(monitor1);
        Assert.True(monitor1.UniqueId.Length > 0);

        var monitor2 = new RedisChangeMonitor(this.Bus, _notifKey);
        Assert.NotNull(monitor2);
        Assert.True(monitor2.UniqueId.Length > 0);

        Assert.NotSame(monitor1.UniqueId, monitor2.UniqueId);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenCtor_ShouldBeRegistered()
    {
        var monitor = new RedisChangeMonitor(this.Bus, _notifKey);
        this._mockOfBus.Verify(e => e.Subscribe(_notifKey, monitor), Times.Once);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenExceptionInCtor_ShouldBeDisposed()
    {
        this._mockOfBus.Setup(e => e.Subscribe(It.IsAny<string>(), It.IsAny<INotificationObserver<string>>())).Throws<InvalidOperationException>();
        var monitor = new RedisChangeMonitor(this.Bus, _notifKey);

        Assert.True(monitor.IsDisposed);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenChanged_ShouldBeDisposed()
    {
        var monitor = new RedisChangeMonitor(this.Bus, _notifKey);
        monitor.Notify(_notifKey);

        Assert.True(monitor.IsDisposed);

        this._mockOfBus.Verify(e => e.Subscribe(_notifKey, monitor), Times.Once);
        this._mockOfDispose.Verify(e => e.Dispose(), Times.Once);
    }
}
