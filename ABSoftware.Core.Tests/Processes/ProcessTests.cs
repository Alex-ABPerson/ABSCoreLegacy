using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ABSoftware.Core.Processes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABSoftware.Core.Tests.Processes
{
    [TestClass]
    public class ProcessTests
    {
        public ProcessThatLaunchesCode BlankProcess = new ProcessThatLaunchesCode(() => { }, (up) => { });
        [TestInitialize]
        public async Task InitMethod()
        {
            await ProcessManager.CancelAll();
            ProcessManager.StartExecution();
        }

        [TestMethod, TestCategory("Processes")]
        public async Task EnqueueProcesses_HighMediumAndLowPriority_RunsInOrder()
        {
            var orders = new List<int>();

            ProcessManager.EnqueueHighPriority(new ProcessThatLaunchesCode(() => orders.Add(0), (up) => { }));
            ProcessManager.EnqueueHighPriority(new ProcessThatLaunchesCode(() => orders.Add(1), (up) => { }));
            ProcessManager.EnqueueMediumPriority(new ProcessThatLaunchesCode(() => orders.Add(2), (up) => { }));
            ProcessManager.EnqueueMediumPriority(new ProcessThatLaunchesCode(() => orders.Add(3), (up) => { }));
            ProcessManager.EnqueueLowPriority(new ProcessThatLaunchesCode(() => orders.Add(4), (up) => { }));
            ProcessManager.EnqueueLowPriority(new ProcessThatLaunchesCode(() => orders.Add(5), (up) => { }));

            await ProcessManager.WaitForAllToComplete();

            CollectionAssert.AreEqual(orders, new List<int>() { 0, 1, 2, 3, 4, 5 });
        }

        [TestMethod, TestCategory("Processes")]
        public async Task CancelAllProcesses_GetsCancelled()
        {
            var orders = new List<int>();

            ProcessManager.EnqueueHighPriority(new ProcessThatLaunchesCode(() =>
            {
                Thread.Sleep(5000);
                orders.Add(1);
            }, (up) => { }));
            ProcessManager.EnqueueMediumPriority(new ProcessThatLaunchesCode(() => orders.Add(2), (up) => { }));
            ProcessManager.EnqueueLowPriority(new ProcessThatLaunchesCode(() => orders.Add(4), (up) => { }));

            // Wait to make sure it has definitely loaded.
            //await Task.Delay(1000);

            await Task.Delay(100);
            await ProcessManager.CancelAll();

            CollectionAssert.AreEqual(orders, new List<int>() { 1 });
        }

        [TestMethod, TestCategory("Processes")]
        public async Task CancelCurrentProcess_RunsUndo()
        {
            var orders = new List<int>();

            ProcessManager.EnqueueHighPriority(new ProcessThatLaunchesCode(() =>
            {
                Thread.Sleep(5000);
                orders.Add(1);
            }, (up) => orders.RemoveAt(0)));
           
            // Wait to make sure it has definitely loaded.
            //await Task.Delay(1000);

            await Task.Delay(100);
            await ProcessManager.CancelCurrentProcess();

            CollectionAssert.AreEqual(orders, new List<int>());
        }
    }
}
