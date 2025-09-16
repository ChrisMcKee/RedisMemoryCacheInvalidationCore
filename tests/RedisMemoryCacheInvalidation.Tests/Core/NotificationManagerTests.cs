using System.Linq;
using AutoFixture;
using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Core;

public class NotificationManagerTests
{
    private Fixture _fixture = new Fixture();
    private readonly string _topicKey;
    public NotificationManagerTests()
    {
        _topicKey = _fixture.Create<string>();
    }
    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenSubscribed_ShouldPass()
    {
        var mockOfObserver = new Mock<INotificationObserver<string>>();
        var notifier = new NotificationManager();
        var res = notifier.Subscribe(_topicKey, mockOfObserver.Object);

        Assert.NotNull(res);
        Assert.Single(notifier.SubscriptionsByTopic.Values);
        Assert.Contains(mockOfObserver.Object, notifier.SubscriptionsByTopic.Values.SelectMany(e => e));
        Assert.IsType<Unsubscriber>(res);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenSubscribedTwice_ShouldBeSubscriberOnlyOnce()
    {
        var mockOfObserver = new Mock<INotificationObserver<string>>();
        var notifier = new NotificationManager();
        var res1 = notifier.Subscribe(_topicKey, mockOfObserver.Object);
        var res2 = notifier.Subscribe(_topicKey, mockOfObserver.Object);

        Assert.NotNull(res1);
        Assert.NotSame(res1, res2);
        Assert.Single(notifier.SubscriptionsByTopic.Values);
        Assert.Contains(mockOfObserver.Object, notifier.SubscriptionsByTopic.Values.SelectMany(e => e));
        Assert.IsType<Unsubscriber>(res1);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
    public void WhenSameTopic_ShouldNotifyAll()
    {
        var mockOfObserver1 = new Mock<INotificationObserver<string>>();
        var mockOfObserver2 = new Mock<INotificationObserver<string>>();
        var notifier = new NotificationManager();
        var res1 = notifier.Subscribe(_topicKey, mockOfObserver1.Object);
        var res2 = notifier.Subscribe(_topicKey, mockOfObserver2.Object);

        notifier.Notify(_topicKey);

        Assert.True(notifier.SubscriptionsByTopic.Values.SelectMany(e => e).Any());
    }
}
