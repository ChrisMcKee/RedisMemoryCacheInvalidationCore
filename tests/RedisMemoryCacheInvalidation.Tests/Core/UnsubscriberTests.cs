﻿using System;
using Moq;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Utils;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Core
{
    public class UnsubscriberTests
    {
        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenCtorBadArgs_ShouldThrowExceptions()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var mock = new Mock<INotificationObserver<string>>();
                var s = new Unsubscriber(null, mock.Object);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var obs = new SynchronizedCollection<INotificationObserver<string>> { };
                var s = new Unsubscriber(obs, null);
            });
        }

        [Fact]
        [Trait(TestConstants.TestCategory, TestConstants.UnitTestCategory)]
        public void WhenUnsubscribe_ShouldBeDisposed()
        {
            var mock1 = new Mock<INotificationObserver<string>>();
            var mock2 = new Mock<INotificationObserver<string>>();
            var mock3 = new Mock<INotificationObserver<string>>();

            var obs = new SynchronizedCollection<INotificationObserver<string>> { mock1.Object, mock2.Object, mock3.Object };
            var unsub = new Unsubscriber(obs, mock2.Object);
            unsub.Dispose();

            Assert.Equal(2, obs.Count);
            Assert.DoesNotContain(mock2.Object, obs);
        }
    }
}
