using BackyLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestBacky
{
    [TestClass]
    public class TestBlockingQueue
    {
        [TestMethod]
        public void BlockingQueue_01_When_no_items_added_Should_timeout()
        {
            var queue = new BlockingQueue<int>();
            var sw = Stopwatch.StartNew();
            var item = queue.Dequeue(1000);
            sw.Stop();
            item.Timedout.ShouldBe(true);
            sw.ElapsedMilliseconds.ShouldBeInRange(1000, 1020);
        }

        [TestMethod]
        public void BlockingQueue_02_When_item_exists_Should_retun_it()
        {
            var queue = new BlockingQueue<int>();
            queue.Add(1);
            var sw = Stopwatch.StartNew();
            var item = queue.Dequeue(1000);
            sw.Stop();
            item.Timedout.ShouldBe(false);
            item.Value.ShouldBe(1);
            sw.ElapsedMilliseconds.ShouldBeInRange(0, 20);
        }

        [TestMethod]
        public void BlockingQueue_03_When_items_is_added_during_wait_Should_retun_it_before_timeout()
        {
            var queue = new BlockingQueue<int>();
            Task.Run(() =>
            {
                Thread.Sleep(50);
                queue.Add(2);
            });
            var sw = Stopwatch.StartNew();
            var item = queue.Dequeue(1000);
            sw.Stop();
            
            item.Timedout.ShouldBe(false);
            item.Value.ShouldBe(2);
            sw.ElapsedMilliseconds.ShouldBeInRange(50, 70);
        }

        [TestMethod]
        public void BlockingQueue_04_When_two_waits_and_only_one_items_is_added_One_should_get_it_and_other_should_timeout()
        {
            var queue = new BlockingQueue<int>();
            Task.Run(() =>
            {
                Thread.Sleep(250);
                queue.Add(2);
            });

            var waiter1 = Task.Run(() => queue.Dequeue(1000));
            var waiter2 = Task.Run(() => queue.Dequeue(1000));

            var sw1 = Stopwatch.StartNew();
            var sw2 = Stopwatch.StartNew();
            var compltetdTaskIndex = Task.WaitAny(waiter1, waiter2);
            sw1.Stop();

            Task.WaitAll(waiter1, waiter2);
            sw2.Stop();

            GetOrTimeoutResult<int> returnedRes, timedoutRes;
            if (compltetdTaskIndex == 0)
            {
                returnedRes = waiter1.Result;
                timedoutRes = waiter2.Result;
            }
            else
            {
                returnedRes = waiter2.Result;
                timedoutRes = waiter1.Result;
            }


            returnedRes.Timedout.ShouldBe(false);
            returnedRes.Value.ShouldBe(2);
            timedoutRes.Timedout.ShouldBe(true);
            sw1.ElapsedMilliseconds.ShouldBeInRange(250, 270);
            sw2.ElapsedMilliseconds.ShouldBeInRange(1000, 1020);
        }
    }
}
