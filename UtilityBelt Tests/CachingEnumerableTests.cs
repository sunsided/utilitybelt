using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UtilityBelt.Collections;

namespace UtilityBelt.Tests
{
    [TestFixture]
    public class CachingEnumerableTests
    {
        /// <summary>
        /// Tests simple streaming of data
        /// </summary>
        [Test]
        public void TestSimpleStreaming()
        {
            const string testString = "CachingEnumerable<T> works just as expected";
            
            // get the cache
            var enumerable = testString.GetCachingEnumerator();

            for (int i = 0; i < 2; ++i)
            {
                // enumerate
                StringBuilder builder = new StringBuilder();
                foreach (var item in enumerable)
                {
                    builder.Append(item);
                }

                // compare
                Assert.That(builder.ToString(), Is.EqualTo(testString));
            }
        }

        /// <summary>
        /// Tests simple streaming of data
        /// </summary>
        [Test]
        public void TestParallelStreaming()
        {
            const string testString = "CachingEnumerable<T> works just as expected";

            // get the cache
            var enumerable = testString.GetCachingEnumerator();

            // formulate function under test
            ManualResetEvent startSignal = new ManualResetEvent(false);
            Func<string> enumerationFunc = () =>
                {
                    // block until test is ready
                    startSignal.WaitOne();

                    // Randomize sleep
                    Random random = new Random();

                    // enumerate
                    StringBuilder builder = new StringBuilder();
                    foreach (var item in enumerable)
                    {
                        builder.Append(item);
                        Thread.Sleep(random.Next(0, 10));
                    }

                    return builder.ToString();
                };

            // run multiple tasks and wait for join
            const int count = 5;
            Task<string>[] tasks = new Task<string>[count];
            for (int i=0; i<count; ++i)
            {
                tasks[i] = Task<string>.Factory.StartNew(enumerationFunc, TaskCreationOptions.LongRunning);
            }

            startSignal.Set();
            Task.WaitAll(tasks);

            // compare
            foreach (var task in tasks)
            {
                Assert.That(task.Result, Is.EqualTo(testString));
            }
        }
    }
}
