using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABSoftware.Core.Registry
{
    /// <summary>
    /// A class to handle the ABSoftware registry.
    /// </summary>
    public static class ABSRegistry
    {
        #region Variables

        /// <summary>
        /// The items in the registry.
        /// </summary>
        public static List<Item> RegistryItems { get; private set; } = new List<Item>();

        #endregion

        #region General Methods

        /// <summary>
        /// Adds a new item to the root of the ABSoftware registry.
        /// </summary>
        public static async Task AddItemToRootAsync(Item item)
        {
            // If that item already exists, then we can't add it.
            if (await LocateInRootAsync(item.Name) != null)
                throw new Exception("That item already exists.");

            // Otherwise, add the item!
            RegistryItems.Add(item);
        }

        /// <summary>
        /// Adds a new item at the specified path of the ABSoftware registry.
        /// </summary>
        public static Task AddItemAtAsync(string path, Item item)
        {
            return AddItemAtAsync(path.ToCharArray(), item);
        }

        /// <summary>
        /// Adds a new item at the specified path of the ABSoftware registry.
        /// </summary>
        public static async Task AddItemAtAsync(char[] path, Item item)
        {
            // First, split up that path.
            var split = await SplitPathAsync(path);

            // If that path is just the root, then we can add it to the root.
            if (split.Count == 0)
            {
                await AddItemToRootAsync(item);
                return;
            }

            // Next, locate the item that path refers to.
            var parentPath = await LocateFromPathInternal(split, false);

            // If that item isn't a group, then we can't add an item inside it!
            if (!(parentPath.Item1 is Group group))
                throw new Exception("The given path is not a group.");

            // Then, check to make sure that the item doesn't already exist.
            if (await LocateInGroupAsync(item.Name, group) != null)
                throw new Exception("That item already exists.");

            // Finally, addd the item there.
            group.InnerItems.Add(item);
        }

        /// <summary>
        /// Create a new group at the specified path in the ABSoftware registry.
        /// </summary>
        public static Task CreateGroupAsync(string path)
        {
            return CreateGroupAsync(path.ToCharArray());
        }

        /// <summary>
        /// Create a new group at the specified path in the ABSoftware registry.
        /// </summary>
        public static async Task CreateGroupAsync(char[] path)
        {
            // First, split up the path.
            var split = await SplitPathAsync(path);

            // If we're just creating a group in root, then we can just add it straight there.
            if (split.Count == 1)
            {
                await AddItemToRootAsync(new Group(split[0].ToArray()));
                return;
            }

            // Next, navigate to the place the group is going to go into.
            var navigatedPath = await LocateFromPathInternal(split, true);

            // Also, check to make sure that the item doesn't already exist.
            if (await LocateInGroupAsync(navigatedPath.Item1.Name, navigatedPath.Item1 as Group) != null)
                throw new Exception("That item already exists.");

            // Then, create the new group based on the name.
            var newGroup = new Group(navigatedPath.Item2.ToArray());

            // Finally, put that new group into the correct path.
            (navigatedPath.Item1 as Group).InnerItems.Add(newGroup);

        }

        /// <summary>
        /// Gets all of the items in a path asyncronously.
        /// </summary>
        /// <param name="path">The path we're looking at.</param>
        public static Task<List<Item>> GetItemsAsync(string path)
        {
            return GetItemsInternal(path.ToCharArray());
        }

        /// <summary>
        /// Gets all of the items in a path asyncronously.
        /// </summary>
        /// <param name="path">The path we're looking at.</param>
        public static Task<List<Item>> GetItemsAsync(char[] path)
        {
            return GetItemsInternal(path);
        }

        static async Task<List<Item>> GetItemsInternal(char[] path)
        {
            // First, split the path up.
            var splitPath = await SplitPathAsync(path);

            // If the path is just the root (which means there is no path at all), then return that.
            if (splitPath.Count == 0)
                return RegistryItems;

            // If not, locate the item.
            var item = (await LocateFromPathInternal(splitPath, false)).Item1;

            // Then, check if it is actually group, if so, just return the items in it!
            if (item is Group group)
                return group.InnerItems;

            // Otherwise, that's wrong.
            throw new Exception("The path did not point to a group. If you want to just get the item at a certain path, please use 'LocateFromPath'");
        }

        /// <summary>
        /// Checks if a certain item (at the given path) exists.
        /// </summary>
        /// <param name="path">The path we're looking at.</param>
        public static Task<bool> ExistsAsync(string path)
        {
            return ExistsAsync(path.ToCharArray());
        }

        /// <summary>
        /// Checks if a certain item (at the given path) exists.
        /// </summary>
        /// <param name="path">The path we're looking at.</param>
        public static Task<bool> ExistsAsync(char[] path)
        {
            return ExistsInternal(path);
        }

        static async Task<bool> ExistsInternal(char[] path)
        {
            // First, split up the path.
            var split = await SplitPathAsync(path);

            // If we're just checking the root (/), then that definitely exists.
            if (split.Count == 0)
                return true;

            // Start off by looking inside the root, and if that part failed, then this path doesn't exist.
            var previousItem = await LocateInRootAsync(split[0].ToArray());
            if (previousItem == null)
                return false;

            // Attempt to navigate through the rest of the groups in this path, if we fail at any of them, return false.
            for (var i = 1; i < split.Count; i++)
            {
                // If the previous item wasn't a group, and we're trying to go inside that, then this path doesn't exist.
                if (!(previousItem is Group group))
                    return false;

                // Now that we've checked that, we can attempt to navigate into the previous group.
                previousItem = await LocateInGroupAsync(split[i].ToArray(), group);

                // If we failed to navigate into the previous group, then this path doesn't exist.
                if (previousItem == null)
                    return false;
            }

            // If we got here, then that path did exist.
            return true;

        }
        /// <summary>
        /// Deletes an item in the registry.
        /// </summary>
        /// <param name="path">The path we're looking at.</param>
        public static Task DeleteAsync(string path)
        {
            return DeleteAsync(path.ToCharArray());
        }

        /// <summary>
        /// Deletes an item in the registry.
        /// </summary>
        /// <param name="path">The path we're looking at.</param>
        public static Task DeleteAsync(char[] path)
        {
            return DeleteInternal(path);
        }

        static async Task DeleteInternal(char[] path)
        {
            // First, split up the path.
            var split = await SplitPathAsync(path);

            // If we're deleting a file in the root, then just go through each of the root items until we find it.
            if (split.Count == 1)
            {
                for (var i = 0; i < RegistryItems.Count; i++)
                    if (RegistryItems[i].Name.SequenceEqual(split[0]))
                    {
                        RegistryItems.RemoveAt(i);
                        return;
                    }
            }

            // Locate the item, ignoring the last part (since that's the part we'll be deleted).
            var located = await LocateFromPathInternal(split, true);

            // If the parent path isn't a group, then that's a problem.
            if (!(located.Item1 is Group group))
                throw new Exception("Invalid path given.");

            // Next, navigate through all of the items in the parent of the item we're going to delete.
            // Then, work out what index the item is, and remove it!
            for (var i = 0; i < group.InnerItems.Count; i++)
                if (group.InnerItems[i].Name.SequenceEqual(located.Item2))
                {
                    group.InnerItems.RemoveAt(i);
                    return;
                }

            // If we got here, then it didn't get deleted, meaning it didn't exist.
            throw new Exception("Invalid path given.");
        }

        #endregion

        #region Path Handling

        /// <summary>
        /// Splits a path down into each parts (e.g. /AB/CD would become [['A', 'B'], ['C', 'D']])
        /// </summary>
        public static Task<List<List<char>>> SplitPathAsync(string path)
        {
            return SplitPathAsync(path.ToCharArray());
        }

        /// <summary>
        /// Splits a path down into each parts (e.g. /AB/CD would become [['A', 'B'], ['C', 'D']])
        /// </summary>
        public static Task<List<List<char>>> SplitPathAsync(char[] path)
        {
            return Task.Run(() => SplitPathInternal(path));
        }

        static List<List<char>> SplitPathInternal(char[] path)
        {
            var i = 0;
            var result = new List<List<char>>();

            // If there's nothing to the path, just stop and return a completely blank path.
            if (path.Length == 0)
                return new List<List<char>>();

            // Add the very start item.
            result.Add(new List<char>());

            // If the very first character is a slash, we can just skip that, since we just added the item above.
            if (path[0] == '/' || path[0] == '\\')
                i = 1;

            // How this will work is: We will continue to "build up" text given to us until we encounter a slash (or reach the end).
            // When we do encounter a slash, we will put that "build up" in as a new entry in the result (well, it's already in there), and move onto the next one.
            for (; i < path.Length; i++)
            {
                if (path[i] == '/' || path[i] == '\\')
                    result.Add(new List<char>());
                else
                    result[result.Count - 1].Add(path[i]);
            }

            // Finally, if we have a blank item at the end, remove it.
            if (result[result.Count - 1].Count == 0)
                result.RemoveAt(result.Count - 1);

            return result;
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Looks for an item with a certain name in the root (/)
        /// NOTE: RETURNS NULL IF ITEM NOT FOUND.
        /// </summary>
        public static Task<Item> LocateInRootAsync(string name)
        {
            return LocateInRootAsync(name.ToCharArray());
        }

        /// <summary>
        /// Looks for an item with a certain name in the root (/)
        /// NOTE: RETURNS NULL IF ITEM NOT FOUND.
        /// </summary>
        public static Task<Item> LocateInRootAsync(char[] name)
        {
            return Task.Run(() => LocateInRootInternal(name));
        }

        static Item LocateInRootInternal(char[] name)
        {
            for (var i = 0; i < RegistryItems.Count; i++)
                if (RegistryItems[i].Name.SequenceEqual(name))
                    return RegistryItems[i];

            return null;
        }

        /// <summary>
        /// Looks for an item with a certain name in a certain group.
        /// NOTE: RETURNS NULL IF ITEM NOT FOUND.
        /// </summary>
        public static Task<Item> LocateInGroupAsync(string name, Group group)
        {
            return LocateInGroupAsync(name.ToCharArray(), group);
        }

        /// <summary>
        /// Looks for an item with a certain name in a certain group.
        /// NOTE: RETURNS NULL IF ITEM NOT FOUND.
        /// </summary>
        public static Task<Item> LocateInGroupAsync(char[] name, Group group)
        {
            return Task.Run(() => LocateInGroupInternal(name, group));
        }

        static Item LocateInGroupInternal(char[] name, Group group)
        {
            for (var i = 0; i < group.InnerItems.Count; i++)
                if (group.InnerItems[i].Name.SequenceEqual(name))
                    return group.InnerItems[i];

            return null;
        }

        /// <summary>
        /// Looks for an item at a certain path.
        /// NOTE: RETURNS NULL IF ITEM NOT FOUND.
        /// </summary>
        /// <param name="path">The path to look at.</param>
        public static Task<Item> LocateFromPathAsync(string path)
        {
            return LocateFromPathAsync(path.ToCharArray());
        }

        /// <summary>
        /// Looks for an item at a certain path.
        /// NOTE: RETURNS NULL IF ITEM NOT FOUND.
        /// </summary>
        /// <param name="path">The path to look at.</param>
        public static async Task<Item> LocateFromPathAsync(char[] path)
        {
            return (await LocateFromPathInternal(await SplitPathAsync(path), false)).Item1;
        }

        /// <summary>
        /// The main part of LocateFromPath.
        /// The most important method in here as it is used by a lot of the other features as well.
        /// </summary>
        /// <param name="pathParts">What you get from <see cref="SplitPath(char[])"/></param>
        /// <param name="ignoreLastPart">Tells this to return the second-to-last part and output the name of the last part (that's the second item). Used for methods that add/create items</param>
        async static Task<Tuple<Item, List<char>>> LocateFromPathInternal(List<List<char>> pathParts, bool ignoreLastPart)
        {
            const string DoesNotExistText = "That item does not exist (or part of the path is invalid). Please remember to always use 'Exists' before you run this.";

            // First, let's navigate to the very first item that's in the root (since the root is special).
            var rootItem = await LocateInRootAsync(pathParts[0].ToArray());

            // Next, check that it path exists, if not, throw an exception.
            if (rootItem == null)
                throw new Exception(DoesNotExistText);

            // Then, go through the rest, if any of them don't exist, or we try to naviagte inside something that's not a group, just throw the exception.
            var previousItem = rootItem;
            for (var i = 1; i < pathParts.Count; i++)
            {
                // If the previous item wasn't a group, then we can't go inside any further, which we're attempting to do.
                if (!(previousItem is Group prevAsGroup))
                    throw new Exception(DoesNotExistText);

                // If we need to ignore the last part, and this is the last part, then we'll do what we need to do.
                if (ignoreLastPart && i == pathParts.Count - 1)
                    return new Tuple<Item, List<char>>(previousItem, pathParts[i]);

                // Now that we've made sure it is actually a group, we can now navigate down inside this group.
                previousItem = await LocateInGroupAsync(pathParts[i].ToArray(), prevAsGroup);

                // And, if that didn't work, we also have a problem.
                if (previousItem == null)
                    throw new Exception(DoesNotExistText);
            }

            // Finally, return the last item we landed on.
            return new Tuple<Item, List<char>>(previousItem, null);
        }

        #endregion

        #region Extra Methods

        public static void ClearRegistry()
        {
            RegistryItems.Clear();
        }

        #endregion
    }
}
