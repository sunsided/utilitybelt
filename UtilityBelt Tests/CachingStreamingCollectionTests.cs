using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UtilityBelt.Collections;

namespace UtilityBelt.Tests
{
    [TestFixture]
    public class CachingStreamingCollectionTests
    {
        [Test]
        public void SimpleEnumeration()
        {
            var collection = new CachingStreamingCollection<int>();
            Assert.That(collection.HasData, Is.False);
            Assert.That(collection.AddingDone, Is.False);

            // Add a couple of data
            using(var adder = collection.BeginAdding())
            {
                adder.Add(1);
                adder.Add(2, 3);
                adder.Add(new [] {4, 5, 6});

                Assert.That(collection.AddingDone, Is.False);
            }

            Assert.That(collection.AddingDone, Is.True);
            Assert.That(collection.HasData, Is.True);

            // Iterate
            int startValue = 1;
            foreach (var i in collection)
            {
                Assert.That(i, Is.EqualTo(startValue++));
            }

            // Iterate again
            startValue = 1;
            foreach (var i in collection)
            {
                Assert.That(i, Is.EqualTo(startValue++));
            }
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void AddAfterCloseFails()
        {
            var collection = new CachingStreamingCollection<int>();
            using (var adder = collection.BeginAdding())
            {
                adder.Add(1);
                adder.Finish();
                adder.Add(2);
            }

        }

        [Test]
        public void MultiThreadedReadOut([Values(10, 20, 100)] int count, [Values(0, 2, 10)] int delayMs)
        {
            var collection = new CachingStreamingCollection<int>();

            AutoResetEvent signal = new AutoResetEvent(false);

            // Add a couple of data
            Task.Factory.StartNew(() =>
                                      {
                                          using (var adder = collection.BeginAdding())
                                          {
                                              for (int i = 0; i < count; i++)
                                              {
                                                  Trace.WriteLine("Writing: " + i);
                                                  adder.Add(i);
                                                  Thread.Sleep(delayMs);
                                              }
                                          }

                                          // Done
                                          signal.Set();
                                      });

            // Iterate
            int startValue = 0;
            foreach (var i in collection)
            {
                Trace.WriteLine("Reader 1: " + i);
                Assert.That(i, Is.EqualTo(startValue++));
            }

            // Iterate
            startValue = 0;
            foreach (var i in collection)
            {
                Trace.WriteLine("Reader 2: " + i);
                Assert.That(i, Is.EqualTo(startValue++));
            }

            // Wait for completion
            signal.WaitOne();
        }
    }
}
