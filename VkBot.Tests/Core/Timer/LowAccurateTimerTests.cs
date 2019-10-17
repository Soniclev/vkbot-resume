using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VkBot.Core.Timer;
using Xunit;

namespace VkBot.Tests.Core.Timer
{
    public class LowAccurateTimerTests
    {
        public static void TestTimer(ITimer timer, int sleepTime, int numbersExecute)
        {
            var executeTimes = 0;
            Action<CancellationToken> action = (c) =>
            {
                executeTimes++;
            };
            timer.AddNumberTimes(action, TimeSpan.FromMilliseconds(sleepTime / 1.5d), JobExecutionType.OnlyOneInstance, numbersExecute);
            Thread.Sleep(numbersExecute * sleepTime);
            timer.Remove(action);
            Assert.InRange(executeTimes, numbersExecute * 3 / 4, numbersExecute * 4 / 3);

            Thread.Sleep(numbersExecute * sleepTime);
            Assert.InRange(executeTimes, numbersExecute * 3 / 4, numbersExecute * 4 / 3);

            action = (c) =>
            {
                executeTimes++;
                if (c.IsCancellationRequested)
                    return;
                Thread.Sleep(1000);
            };
            executeTimes = 0;
            timer.AddNumberTimes(action, TimeSpan.FromMilliseconds(sleepTime / 1.5d), JobExecutionType.AllowSeveralInstances, numbersExecute);
            Thread.Sleep(numbersExecute * sleepTime);
            timer.Remove(action);
            Assert.InRange(executeTimes, numbersExecute * 3 / 4, numbersExecute * 4 / 3);

            Thread.Sleep(numbersExecute * sleepTime);
            Assert.InRange(executeTimes, numbersExecute * 3 / 4, numbersExecute * 4 / 3);

            action = (c) =>
            {
                Thread.Sleep(sleepTime * (numbersExecute + 1));
                if (c.IsCancellationRequested)
                    return;
                executeTimes++;
            };
            executeTimes = 0;
            timer.AddNumberTimes(action, TimeSpan.FromMilliseconds(sleepTime / 1.5d), JobExecutionType.KillPreviousInstance, numbersExecute);
            Thread.Sleep(numbersExecute * sleepTime);
            timer.Remove(action);
            Assert.Equal(0, executeTimes);

            Thread.Sleep(numbersExecute * sleepTime * 2);
            Assert.Equal(1, executeTimes);
        }

        [Collection("Collection 1")]
        public class OneTimesExecute
        {
            [Fact(Skip = "Не работает на билд сервере")]
            public void OneTimesExecuteTest()
            {
                var executeTimes = 0;
                ITimer timer = new LowAccurateTimer();
                Action<CancellationToken> action = (c) =>
                {
                    executeTimes++;
                };
                timer.AddOneTime(action, TimeSpan.FromMilliseconds(100));
                Thread.Sleep(120);
                Assert.Equal(1, executeTimes);
            }
        }

        [Collection("Collection 1")]
        public class TwoTimesExecute
        {
            [Fact(Skip = "Не работает на билд сервере")]
            public void TwoTimesExecuteTest()
            {
                ITimer timer = new LowAccurateTimer();
                TestTimer(timer, 20, 2);
            }
        }

        [Collection("Collection 1")]
        public class InfinityTimesExecute
        {
            [Fact(Skip = "Не работает на билд сервере")]
            public void InfinityTimesExecuteTest()
            {
                const int executeNumber = 12;
                const int sleepTime = 100;

                var executeTimes = 0;
                ITimer timer = new LowAccurateTimer();
                Action<CancellationToken> action = (c) => { executeTimes++; };
                timer.AddNumberTimes(action, TimeSpan.FromMilliseconds(sleepTime), 0);
                Thread.Sleep(sleepTime * executeNumber);
                timer.Remove(action);
                // надеемся, что он будет вызван хотя бы 75% от общего количества раз
                Assert.InRange(executeTimes, executeNumber * 3 / 4, executeNumber);
            }
        }
    }
}
