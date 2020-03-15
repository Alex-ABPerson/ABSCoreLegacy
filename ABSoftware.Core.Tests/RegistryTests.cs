using ABSoftware.Core.Registry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABSoftware.Core.Tests
{
    [TestClass]
    public class RegistryTests
    {
        private bool TestArr(List<List<char>> arr1, List<List<char>> arr2)
        {
            if (arr1.Count != arr2.Count) return false;
            for (int i = 0; i < arr1.Count; i++)
                if (!arr1[i].SequenceEqual(arr2[i]))
                    return false;

            return true;

        }

        [TestInitialize]
        public void Init() => ABSRegistry.ClearRegistry();

        [TestMethod]
        [TestCategory("Registry")]
        public async Task SplitPath_RootOnly_EmptyPath()
        {
            var expected = new List<List<char>>();

            CollectionAssert.AreEqual(expected, await ABSRegistry.SplitPathAsync("/"));
            CollectionAssert.AreEqual(expected, await ABSRegistry.SplitPathAsync(new char[] { '/' }));
            CollectionAssert.AreEqual(expected, await ABSRegistry.SplitPathAsync("\\"));
            CollectionAssert.AreEqual(expected, await ABSRegistry.SplitPathAsync(new char[] { '\\' }));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task SplitPath_Nothing_EmptyPath()
        {
            var expected = new List<List<char>>();

            CollectionAssert.AreEqual(expected, await ABSRegistry.SplitPathAsync(""));
            CollectionAssert.AreEqual(expected, await ABSRegistry.SplitPathAsync(new char[0]));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task SplitPath_GroupInRoot()
        {
            var expected = new List<List<char>>()
            {
                new List<char>() { 'A', 'B', 'P', 'a', 'i', 'n', 't' }
            };

            Assert.IsTrue(TestArr(expected, await ABSRegistry.SplitPathAsync("ABPaint")));
            Assert.IsTrue(TestArr(expected, await ABSRegistry.SplitPathAsync("/ABPaint")));
            Assert.IsTrue(TestArr(expected, await ABSRegistry.SplitPathAsync("\\ABPaint")));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task SplitPath_MultipleParts()
        {
            var expected = new List<List<char>>()
            {
                new List<char>() { 'A', 'B', 'P', 'a', 'i', 'n', 't' },
                new List<char>() { 'N', 'e', 'x', 't' },
                new List<char>() { 'A', 'n', 'o', 't', 'h', 'e', 'r' }
            };

            Assert.IsTrue(TestArr(expected, await ABSRegistry.SplitPathAsync("ABPaint/Next/Another")));
            Assert.IsTrue(TestArr(expected, await ABSRegistry.SplitPathAsync("/ABPaint/Next/Another")));
            Assert.IsTrue(TestArr(expected, await ABSRegistry.SplitPathAsync("ABPaint\\Next\\Another")));
            Assert.IsTrue(TestArr(expected, await ABSRegistry.SplitPathAsync("\\ABPaint\\Next\\Another")));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task LocateInRoot_ValidPath()
        {
            await ABSRegistry.AddItemToRootAsync(new Group("ABPaint".ToCharArray()));

            CollectionAssert.AreEqual((await ABSRegistry.LocateInRootAsync("ABPaint")).Name, "ABPaint".ToCharArray());
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task LocateInRoot_InvalidPath()
        {
            Assert.AreEqual(await ABSRegistry.LocateInRootAsync("(None)"), null);
        }


        [TestMethod]
        [TestCategory("Registry")]
        public async Task LocateInGroup_ValidPath()
        {
            var group = new Group("ABPaint".ToCharArray());
            group.InnerItems = new List<Item>()
            {
                new Group("Another".ToCharArray())
            };

            CollectionAssert.AreEqual((await ABSRegistry.LocateInGroupAsync("Another", group)).Name, "Another".ToCharArray());
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task LocateInGroup_InvalidPath()
        {
            var group = new Group("ABPaint".ToCharArray())
            {
                InnerItems = new List<Item>()
                {
                    new Group("Another".ToCharArray())
                }
            };

            Assert.AreEqual(await ABSRegistry.LocateInGroupAsync("Another2", group), null);
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task LocateFromPath_JustRoot()
        {
            var rootGroup = new Group("ABPaint".ToCharArray());
            await ABSRegistry.AddItemToRootAsync(rootGroup);

            Assert.AreEqual(rootGroup, await ABSRegistry.LocateFromPathAsync("/ABPaint"));
        }


        [TestMethod]
        [TestCategory("Registry")]
        public async Task LocateFromPath_MultipleParts()
        {
            var secondGroup = new Group("Another".ToCharArray());
            var rootGroup = new Group("ABPaint".ToCharArray())
            {
                InnerItems = new List<Item>()
                {
                    secondGroup
                }
            };
            await ABSRegistry.AddItemToRootAsync(rootGroup);

            CollectionAssert.AreEqual(secondGroup.Name, (await ABSRegistry.LocateFromPathAsync("/ABPaint/Another")).Name);
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task GetItems_FromRoot()
        {
            var rootArr = new Group[]
            {
                new Group("Test1".ToCharArray()),
                new Group("Test2".ToCharArray()),
                new Group("Test3".ToCharArray())
            };

            for (int i = 0; i < rootArr.Length; i++)
                await ABSRegistry.AddItemToRootAsync(rootArr[i]);

            CollectionAssert.AreEqual(rootArr, await ABSRegistry.GetItemsAsync("/"));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task GetItems_MultipleParts()
        {
            var inner = new Group("Inner".ToCharArray())
            {
                InnerItems = new List<Item>()
                {
                    new Group("Final1".ToCharArray()),
                    new Group("Final2".ToCharArray()),
                }
            };

            var rootArr = new Group[]
            {
                new Group("Test1".ToCharArray())
                {
                    InnerItems = new List<Item>()
                    {
                        inner
                    }
                },
                new Group("Test2".ToCharArray()),
                new Group("Test3".ToCharArray())
            };

            for (int i = 0; i < rootArr.Length; i++)
                await ABSRegistry.AddItemToRootAsync(rootArr[i]);

            CollectionAssert.AreEqual(inner.InnerItems, await ABSRegistry.GetItemsAsync("Test1/Inner"));
            CollectionAssert.AreEqual(inner.InnerItems, await ABSRegistry.GetItemsAsync("/Test1/Inner"));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task GetItems_InvalidPath()
        {
            var rootArr = new Group[]
            {
                new Group("Test1".ToCharArray()),
                new Group("Test2".ToCharArray()),
                new Group("Test3".ToCharArray())
            };

            for (int i = 0; i < rootArr.Length; i++)
                await ABSRegistry.AddItemToRootAsync(rootArr[i]);

            try
            {
                // This should throw an exception, meaning the "Assert.Fail" never gets hit.
                await ABSRegistry.GetItemsAsync("Test4");
                Assert.Fail();
            } catch (Exception) { }
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task CreateGroup_AddToRoot()
        {
            var groupName = "Test";

            // Add the group.
            await ABSRegistry.CreateGroupAsync(groupName);

            // Make sure that group exists.
            CollectionAssert.AreEqual(groupName.ToCharArray(), ABSRegistry.RegistryItems[0].Name);
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task CreateGroup_InOtherGroup()
        {
            // Set up the registry.
            await ABSRegistry.AddItemToRootAsync(new Group("Test1".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test2".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test3".ToCharArray()));

            // Add the group.
            await ABSRegistry.CreateGroupAsync("Test1/NewGroup");

            // Make sure that group exists.
            CollectionAssert.AreEqual("NewGroup".ToCharArray(), (ABSRegistry.RegistryItems[0] as Group).InnerItems[0].Name);
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task Exists_InRoot_DoesExist()
        {
            // Set up the registry.
            await ABSRegistry.AddItemToRootAsync(new Group("Test1".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test2".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test3".ToCharArray()));

            Assert.IsTrue(await ABSRegistry.ExistsAsync("Test1"));
            Assert.IsTrue(await ABSRegistry.ExistsAsync("/Test1"));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task Exists_InRoot_DoesNotExist()
        {
            Assert.IsFalse(await ABSRegistry.ExistsAsync("Test1"));
            Assert.IsFalse(await ABSRegistry.ExistsAsync("/Test1"));
        }


        [TestMethod]
        [TestCategory("Registry")]
        public async Task Exists_MultipleParts_DoesNotExist()
        {
            Assert.IsFalse(await ABSRegistry.ExistsAsync("Test1/Another"));
            Assert.IsFalse(await ABSRegistry.ExistsAsync("/Test1/Another"));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task Exists_RootItself_DoesExist()
        {
            Assert.IsTrue(await ABSRegistry.ExistsAsync("/"));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task Exists_MultipleParts_DoesExist()
        {

            var rootArr = new Group[]
            {
                new Group("Test1".ToCharArray())
                {
                    InnerItems = new List<Item>()
                    {
                        new Group("Inner".ToCharArray())
                    }
                },
                new Group("Test2".ToCharArray()),
                new Group("Test3".ToCharArray())
            };

            for (int i = 0; i < rootArr.Length; i++)
                await ABSRegistry.AddItemToRootAsync(rootArr[i]);

            Assert.IsTrue(await ABSRegistry.ExistsAsync("Test1/Inner"));
            Assert.IsTrue(await ABSRegistry.ExistsAsync("/Test1/Inner"));
        }


        [TestMethod]
        [TestCategory("Registry")]
        public async Task AddItemToRoot()
        {
            var groupName = "Test".ToCharArray();

            // Add the group.
            await ABSRegistry.AddItemToRootAsync(new Group(groupName));

            // Make sure that group exists.
            CollectionAssert.AreEqual(groupName, ABSRegistry.RegistryItems[0].Name);
        }


        [TestMethod]
        [TestCategory("Registry")]
        public async Task AddItemToRoot_AlreadyExists()
        {
            var groupName = "Test".ToCharArray();

            // Add the group.
            await ABSRegistry.AddItemToRootAsync(new Group(groupName));


            try
            {
                await ABSRegistry.AddItemToRootAsync(new Group(groupName));

                // An exception should have been thrown, so it shouldn't get here.
                Assert.Fail();
            }
            catch (Exception) { }
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task AddItemAt_AddToRoot()
        {
            var groupName = "Test".ToCharArray();

            // Add the item.
            await ABSRegistry.AddItemAtAsync("", new Group(groupName));

            // Make sure that group exists.
            CollectionAssert.AreEqual(groupName, ABSRegistry.RegistryItems[0].Name);
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task AddItemAt_InOtherGroup()
        {
            var groupName = "NewGroup".ToCharArray();

            // Set up the registry.
            await ABSRegistry.AddItemToRootAsync(new Group("Test1".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test2".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test3".ToCharArray()));

            // Add the item.
            await ABSRegistry.AddItemAtAsync("Test1", new Group(groupName));

            // Make sure that group exists.
            CollectionAssert.AreEqual(groupName, (ABSRegistry.RegistryItems[0] as Group).InnerItems[0].Name);
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task AddItemAt_InOtherGroup_NotGroup()
        {
            var groupName = "NewGroup".ToCharArray();

            // Set up the registry.
            await ABSRegistry.AddItemToRootAsync(new Group("Test1".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test2".ToCharArray()));

            // Add the item.
            await ABSRegistry.AddItemAtAsync("Test1", new Group(groupName));

            // Make sure that group exists.
            CollectionAssert.AreEqual(groupName, (ABSRegistry.RegistryItems[0] as Group).InnerItems[0].Name);
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task AddItemAt_InOtherGroup_AlreadyExists()
        {
            // Set up the registry.
            await ABSRegistry.AddItemToRootAsync(new Group("Test1".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test2".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test3".ToCharArray()));

            // Add the item.
            await ABSRegistry.AddItemAtAsync("Test1", new Group("NewGroup".ToCharArray()));

            try
            {
                await ABSRegistry.AddItemAtAsync("Test1", new Group("NewGroup".ToCharArray()));

                // An exception should have been thrown.
                Assert.Fail();
            }
            catch (Exception) { }
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task Delete_InRoot()
        {
            // Set up the registry.
            await ABSRegistry.AddItemToRootAsync(new Group("Test1".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test2".ToCharArray()));
            await ABSRegistry.AddItemToRootAsync(new Group("Test3".ToCharArray()));

            // Delete the item.
            await ABSRegistry.DeleteAsync("Test2");

            // It shouldn't exist anymore.
            Assert.IsFalse(await ABSRegistry.ExistsAsync("Test2"));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task Delete_InItem()
        {
            var rootArr = new Group[]
            {
                new Group("Test1".ToCharArray())
                {
                    InnerItems = new List<Item>()
                    {
                        new Group("Inner".ToCharArray())
                    }
                },
                new Group("Test2".ToCharArray()),
                new Group("Test3".ToCharArray())
            };

            for (int i = 0; i < rootArr.Length; i++)
                await ABSRegistry.AddItemToRootAsync(rootArr[i]);

            // Delete the item.
            await ABSRegistry.DeleteAsync("Test1/Inner");

            // It shouldn't exist anymore.
            Assert.IsTrue(await ABSRegistry.ExistsAsync("Test1"));
            Assert.IsFalse(await ABSRegistry.ExistsAsync("Test1/Inner"));
        }

        [TestMethod]
        [TestCategory("Registry")]
        public async Task Delete_ItemWithInnerItems()
        {
            var rootArr = new Group[]
            {
                new Group("Test1".ToCharArray())
                {
                    InnerItems = new List<Item>()
                    {
                        new Group("Inner".ToCharArray())
                    }
                },
                new Group("Test2".ToCharArray()),
                new Group("Test3".ToCharArray())
            };

            for (int i = 0; i < rootArr.Length; i++)
                await ABSRegistry.AddItemToRootAsync(rootArr[i]);

            // Delete the item.
            await ABSRegistry.DeleteAsync("Test1");

            // It shouldn't exist anymore.
            Assert.IsFalse(await ABSRegistry.ExistsAsync("Test1"));
            Assert.IsFalse(await ABSRegistry.ExistsAsync("Test1/Inner"));
        }
    }
}
