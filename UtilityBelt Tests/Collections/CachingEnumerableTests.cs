using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UtilityBelt.Collections;

namespace UtilityBelt.Tests.Collections
{
    /// <summary>
    /// Tests for the <see cref="CachingEnumerable{T}"/> type
    /// </summary>
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
                var builder = new StringBuilder();
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
            var startSignal = new ManualResetEvent(false);
            Func<string> enumerationFunc = () =>
                {
                    // block until test is ready
                    startSignal.WaitOne();

                    // Randomize sleep
                    var random = new Random();

                    // enumerate
                    var builder = new StringBuilder();
                    foreach (var item in enumerable)
                    {
                        builder.Append(item);
                        Thread.Sleep(random.Next(0, 10));
                    }

                    return builder.ToString();
                };

            // run multiple tasks and wait for join
            const int count = 5;
            var tasks = new Task<string>[count];
            for (int i=0; i<count; ++i)
            {
                tasks[i] = Task<string>.Factory.StartNew(enumerationFunc, TaskCreationOptions.LongRunning);
            }

            startSignal.Set();
            Assert.That(Task.WaitAll(tasks, TimeSpan.FromSeconds(60)), Is.True);

            // compare
            foreach (var task in tasks)
            {
                Assert.That(task.Result, Is.EqualTo(testString));
            }
        }
    }
}
