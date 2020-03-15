using System;
using System.Threading.Tasks;
using ABSoftware.Core.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABSoftware.Core.Tests.Components
{
    [TestClass]
    public class ComponentTests
    {
        [TestInitialize]
        public async Task Init()
        {
            // Clear out all of the components.
            var components = await ComponentManager.GetComponents();
            while (components.Count != 0)
                await ComponentManager.UnregisterComponent(0);

            await ComponentManager.ClearCache();
            await ComponentManager.ClearAllNotifiers();
        }

        [TestMethod, TestCategory("Components")]
        public async Task RegisterComponent_OneComponent()
        {
            await ComponentManager.RegisterComponent(new DemoComponent1());

            Assert.IsTrue((await ComponentManager.GetComponents()).Count == 1);
            Assert.IsTrue((await ComponentManager.GetComponents())[0].Matches(new DemoComponent1()));
        }

        [TestMethod, TestCategory("Components")]
        public async Task GetComponentsOfType_NoComponents()
        {
            
            var results = await ComponentManager.GetComponentsOfType<DemoComponent1>();
            Assert.IsTrue(results.Count == 0);
        }
        

        [TestMethod, TestCategory("Components")]
        public async Task GetComponentsOfType_OneTypeOfComponent_GenerateCache()
        {
            // Arrange
            var item = new DemoComponent1();
            await ComponentManager.RegisterComponent(item);

            // Act
            var results = await ComponentManager.GetComponentsOfType<DemoComponent1>();

            // Assert
            Assert.IsTrue(results.Count == 1);
            Assert.IsTrue(results[0].Matches(item));
        }

        [TestMethod, TestCategory("Components")]
        public async Task GetComponentsOfType_MultipleTypesOfComponent_GetCache()
        {
            await ComponentManager.RegisterComponent(new DemoComponent1(0));
            await ComponentManager.RegisterComponent(new DemoComponent2());
            await ComponentManager.RegisterComponent(new DemoComponent1(1));
            await ComponentManager.RegisterComponent(new DemoComponent1(2));

            await ComponentManager.GetComponentsOfType<DemoComponent1>();
            var results = await ComponentManager.GetComponentsOfType<DemoComponent1>();
            Assert.IsTrue(results.Count == 3);

            for (int i = 0; i < 3; i++)
                Assert.IsTrue(results[i].Matches(new DemoComponent1(i)));
        }

        [TestMethod, TestCategory("Components")]
        public async Task GetComponentsOfType_MultipleTypesOfComponent_MultipleCache()
        {
            await ComponentManager.RegisterComponent(new DemoComponent1(0));
            await ComponentManager.RegisterComponent(new DemoComponent2());
            await ComponentManager.RegisterComponent(new DemoComponent1(1));
            await ComponentManager.RegisterComponent(new DemoComponent1(2));

            await ComponentManager.GetComponentsOfType<DemoComponent2>();
            var results = await ComponentManager.GetComponentsOfType<DemoComponent1>();
            Assert.IsTrue(results.Count == 3);

            for (int i = 0; i < 3; i++)
                Assert.IsTrue(results[i].Matches(new DemoComponent1(i)));
        }

        [TestMethod, TestCategory("Components")]
        public async Task GetComponentsOfType_OutOfDateCache()
        {
            await ComponentManager.RegisterComponent(new DemoComponent1(0));

            await ComponentManager.GetComponentsOfType<DemoComponent2>();
            await ComponentManager.RegisterComponent(new DemoComponent2());
            await ComponentManager.RegisterComponent(new DemoComponent1(1));

            await ComponentManager.GetComponentsOfType<DemoComponent2>();
            await ComponentManager.RegisterComponent(new DemoComponent1(2));

            var results = await ComponentManager.GetComponentsOfType<DemoComponent1>();
            Assert.IsTrue(results.Count == 3);

            for (int i = 0; i < 3; i++)
                Assert.IsTrue(results[i].Matches(new DemoComponent1(i)));
        }

        [TestMethod, TestCategory("Components")]
        public async Task UnregisterComponent()
        {
            var component = new DemoComponent1();
            await ComponentManager.RegisterComponent(component);
            await ComponentManager.UnregisterComponent(new DemoComponent1());

            var components = await ComponentManager.GetComponents();
            Assert.IsTrue(components.Count == 0);
        }

        [TestMethod, TestCategory("Components")]
        public async Task UnregisterComponent_UsingIndex()
        {
            var component = new DemoComponent1();
            await ComponentManager.RegisterComponent(new DemoComponent1());
            await ComponentManager.RegisterComponent(new DemoComponent2());

            await ComponentManager.UnregisterComponent(1);
            var components = await ComponentManager.GetComponents();
            Assert.IsTrue(components.Count == 1);
            Assert.IsTrue(components[0].Matches(new DemoComponent1()));
        }
        
        [TestMethod, TestCategory("Components")]
        public void Component_Matches_DoesNotMatchType()
        {
            var component1 = new DemoComponent1();
            var component2 = new DemoComponent2();

            Assert.IsFalse(component1.Matches(component2));
        }

        [TestMethod, TestCategory("Components")]
        public async Task Notify_GetsNotified_Added_Delayed() => await TestGetsNotifiedForAdd(true);

        [TestMethod, TestCategory("Components")]
        public async Task Notify_GetsNotified_Removed_Delayed() => await TestGetsNotifiedForRemove(true);

        [TestMethod, TestCategory("Components")]
        public async Task Notify_GetsNotified_Both_Delayed() => await TestGetsNotifiedForBoth(true);

        [TestMethod, TestCategory("Components")]
        public async Task Notify_GetsNotified_Added_NoDelay() => await TestGetsNotifiedForAdd(false);

        [TestMethod, TestCategory("Components")]
        public async Task Notify_GetsNotified_Removed_NoDelay() => await TestGetsNotifiedForRemove(false);

        [TestMethod, TestCategory("Components")]
        public async Task Notify_GetsNotified_Both_NoDelay() => await TestGetsNotifiedForBoth(false);

        private static async Task TestGetsNotifiedForAdd(bool delay)
        {
            bool notified = false;
            ComponentManager.NotifyWhenChanged(ComponentsChangedType.Added, typeof(DemoComponent1), delay, (e) =>
            {
                if (e.Type == ComponentsChangedType.Added && e.AddedComponents[0].GetType() == typeof(DemoComponent1) && e.RemovedComponents == null)
                    notified = true;
                else
                    Assert.Fail();
            });

            // Register a type we're not interested in.
            await ComponentManager.RegisterComponent(new DemoComponent2());
            await Task.Delay(delay ? 3000 : 100);
            Assert.IsFalse(notified);

            // Register a type we are interested in.
            await ComponentManager.RegisterComponent(new DemoComponent1());
            await Task.Delay(delay ? 3000 : 100);
            Assert.IsTrue(notified);
        }

        private static async Task TestGetsNotifiedForRemove(bool delay)
        {
            bool notified = false;
            ComponentManager.NotifyWhenChanged(ComponentsChangedType.Removed, typeof(DemoComponent1), delay, (e) =>
            {
                if (e.Type == ComponentsChangedType.Removed && e.RemovedComponents[0].GetType() == typeof(DemoComponent1) && e.AddedComponents == null)
                    notified = true;
                else
                    Assert.Fail();
            });

            // Register the type, and we shouldn't get notified.
            await ComponentManager.RegisterComponent(new DemoComponent1());
            await Task.Delay(delay ? 3000 : 100);
            Assert.IsFalse(notified);

            // Then, unregister it.
            await ComponentManager.UnregisterComponent(new DemoComponent1());
            await Task.Delay(delay ? 3000 : 100);
            Assert.IsTrue(notified);
        }

        private static async Task TestGetsNotifiedForBoth(bool delay)
        {
            var firstTime = true;
            var passed = false;

            await ComponentManager.RegisterComponent(new DemoComponent1());
            await Task.Delay(50);

            ComponentManager.NotifyWhenChanged(ComponentsChangedType.Both, typeof(DemoComponent1), true, (e) =>
            {
                if (firstTime)
                {
                    if (e.RemovedComponents[0].GetType() == typeof(DemoComponent1) && e.AddedComponents == null)
                        passed = true;

                    firstTime = false;
                }
                else
                {
                    if (e.AddedComponents[0].GetType() == typeof(DemoComponent1) && e.RemovedComponents == null)
                        passed = true;
                }
            });

            // Unregister a component, and see if we get notified.
            await ComponentManager.UnregisterComponent(new DemoComponent1());
            await Task.Delay(delay ? 3000 : 100);
            Assert.IsTrue(passed);
            passed = false;

            // Register a component, and see if we get notified again.
            await ComponentManager.RegisterComponent(new DemoComponent1());
            await Task.Delay(delay ? 3000 : 100);
            Assert.IsTrue(passed);
        }

    }
}
