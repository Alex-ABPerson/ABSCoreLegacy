using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABSoftware.Core.Components
{
    public static class ComponentManager
    {
        // REMEMBER: EVERYTHING HERE IS EXPLAINED ON THE ABSOFTWARE DOCS.

        #region Main

        /// <summary>
        /// All of the registered components.
        /// </summary>
        static List<Component> _registeredComponents = new List<Component>();

        /// <summary>
        /// Everytime the _registeredComponents changes, the version goes higher, this version is used to determine if caches are up to date.
        /// </summary>
        static long _version;

        #endregion

        #region Getting

        public static Task<List<Component>> GetComponents()
        {
            return Task.Run(() => _registeredComponents);
        }

        /// <summary>
        /// Filters through the registered components, and only gives back registered components of a certain type.
        /// </summary>
        /// <typeparam name="T">The type of component we want back.</typeparam>
        public async static Task<List<T>> GetComponentsOfType<T>() where T : Component
        {
            var tType = typeof(T);

            // Attempt to use cache.
            var cacheAttempt = await AttemptToGetCachedComponentsOfType(tType);

            // If we successfully found cache then return that.
            if (cacheAttempt != null) return cacheAttempt as List<T>;

            // If we weren't able to use any cache, we'll generate it now.
            return await GenerateCache<T>(tType);
        }

        #endregion

        #region Registration/Unregistration

        /// <summary>
        /// Registers an ABSoftware component.
        /// </summary>
        /// <param name="component">The component to register.</param>
        public static Task RegisterComponent(Component component)
        {
            return Task.Run(() =>
            {
                _registeredComponents.Add(component);
                _version++;
                ComponentsChanged(ComponentsChangedType.Added, component);
            });
        }

        /// <summary>
        /// Unregisters an ABSoftware component.
        /// </summary>
        /// <param name="component">The component to register.</param>
        public static Task UnregisterComponent(Component component)
        {
            return Task.Run(() =>
            {
                for (var i = 0; i < _registeredComponents.Count; i++)
                    if (_registeredComponents[i].Matches(component))
                        _registeredComponents.RemoveAt(i);
                _version++;
                ComponentsChanged(ComponentsChangedType.Removed, component);
            });
        }

        /// <summary>
        /// Unregisters an ABSoftware component.
        /// </summary>
        /// <param name="component">The component to register.</param>
        public static Task UnregisterComponent(int index)
        {
            return Task.Run(() =>
            {
                var component = _registeredComponents[index];
                _registeredComponents.RemoveAt(index);
                _version++;
                ComponentsChanged(ComponentsChangedType.Removed, component);
            });
        }

        #endregion

        #region Cache

        private async static Task<List<T>> GenerateCache<T>(Type tType) where T : Component
        {
            var componentsOfType = new List<T>();
            for (var i = 0; i < _registeredComponents.Count; i++)
                if (_registeredComponents[i] is T asT)
                    componentsOfType.Add(asT);

            // Now, cache that and return back our new array.
            await AddCachedComponentsOfType(tType, componentsOfType as List<Component>);
            return componentsOfType;
        }

        /// <summary>
        /// In order to speed up performance, the result of ANY request to "GetComponentsOfType" that has already been done, is remembered so that it can be accessed if exactly the same request ON THE SAME VERSION is done.
        /// <para>long: Specifies what "version" this cache was from.</para>
        /// <para>Type: What type the request was requesting for (the "T" in the GetComponentsOfType method)</para>
        /// <para>List: The result that request gave back (the actual cached bit)</para>
        /// </summary>
        static List<Tuple<long, Type, List<Component>>> _cachedComponentsOfType = new List<Tuple<long, Type, List<Component>>>();

        ///// <summary>
        ///// Removes any areas that were marked to be removed during an operation.
        ///// </summary>
        //static void FlushCache()
        //{
        //    // If there is nothing to flush, stop.
        //    if (_markedCachesToRemove.Count == 0)
        //        return;

        //    // Flush all of the changes.
        //    for (int i = 0; i < _markedCachesToRemove.Count; i++)


        //    // Now that it's all flushed, they don't need to be marked anymore.
        //    _markedCachesToRemove.Clear();

        //}

        /// <summary>
        /// Adds cache onto the ComponentsOfType cache.
        /// </summary>
        static Task AddCachedComponentsOfType(Type type, List<Component> components)
        {
            return Task.Run(() => _cachedComponentsOfType.Add(new Tuple<long, Type, List<Component>>(_version, type, components)));
        }

        /// <summary>
        /// This method will attempt to get cached "ComponentsOfType", it outputs where it succeeded through "hasCache",
        /// and if it did, it will return the result.
        /// </summary>
        /// <returns>Whether it was successful</returns>
        static async Task<List<Component>> AttemptToGetCachedComponentsOfType(Type type)
        {

            // Go through each of the caches and determine if any of them are good.
            for (var i = 0; i < _cachedComponentsOfType.Count; i++)
            {
                // NOTE: We check for the version AFTER we check for the type only because this process should only delete cache if needs to be deleted at a given time - to save performance.

                // Check that the type is what we're looking for.
                if (_cachedComponentsOfType[i].Item2.IsEquivalentTo(type))
                    continue;

                // If the cache is old, then we can't use it, and there's only going to be one of them, so we can't continue.
                if (_cachedComponentsOfType[i].Item1 != _version)
                {
                    await RemoveItem(i--);
                    break;
                }

                // If both of those checks passed, we have valid cache here, so, let's use it.
                return _cachedComponentsOfType[i].Item3;
            }

            // If we got here, it couldn't find cache, so return back nothing.
            return null;
        }

        /// <summary>
        /// See information at the start of "cache" region, removes cache.
        /// </summary>
        static Task RemoveItem(int index)
        {
            return Task.Run(() => _cachedComponentsOfType.RemoveAt(index));
        }

        /// <summary>
        /// Clears out anything that has been cached in the background.
        /// </summary>
        public static Task ClearCache()
        {
            return Task.Run(() => _cachedComponentsOfType.Clear());
        }

        #endregion

        #region Changed "Event"

        static TimeSpan _delayPeriod = new TimeSpan(2000);

        /// <summary>
        /// Whether we're currently in the process of notifying.
        /// </summary>
        static bool _currentlyNotifying;

        /// <summary>
        /// Whether we're currently in the delay period.
        /// </summary>
        static bool _currentlyInDelay;

        // Store all of the places we need to notify.
        static Dictionary<Type, List<ComponentManagerNotifyDetails>> _notifyForAddedComponents = new Dictionary<Type, List<ComponentManagerNotifyDetails>>();
        static Dictionary<Type, List<ComponentManagerNotifyDetails>> _notifyForRemovedComponents = new Dictionary<Type, List<ComponentManagerNotifyDetails>>();
        static Dictionary<Type, List<ComponentManagerNotifyDetails>> _notifyForBoth = new Dictionary<Type, List<ComponentManagerNotifyDetails>>();

        // Store all of the components we're about to notify.
        static List<KeyValuePair<Type, List<Component>>> _addedComponents;
        static List<KeyValuePair<Type, List<Component>>> _removedComponents;

        // Store whether we've just changed one of the above.
        static bool _changedAddedOrRemovedComponents;

        public static Task ClearAllNotifiers()
        {
            return Task.Run(() =>
            {
                _notifyForAddedComponents.Clear();
                _notifyForRemovedComponents.Clear();
                _notifyForBoth.Clear();
            });
        }

        /// <summary>
        /// Runs code everytime components are added or removed.
        /// </summary>
        public static void NotifyWhenChanged(ComponentsChangedType type, Type typeToNotifyFor, bool delay, Action<ComponentsChangedEventArgs> code)
        {
            // Generate the notify details.
            var notifyDetails = new ComponentManagerNotifyDetails(code, delay);

            // Determine which dictionary is correct.
            var dictionary = DetermineDictionary(type);

            // Then, determine whether that type is already in that dictionary, and if it isn't, add it now.
            if (!dictionary.ContainsKey(typeToNotifyFor))
                dictionary.Add(typeToNotifyFor, new List<ComponentManagerNotifyDetails>());

            // Finally, add to the list in the dictionary.
            dictionary[typeToNotifyFor].Add(notifyDetails);
        }

        /// <summary>
        /// Runs when components are changed.
        /// </summary>
        static void ComponentsChanged(ComponentsChangedType type, Component component)
        {
            Task.Run(() =>
            {
                if (_notifyForAddedComponents.Count == 0 && _notifyForRemovedComponents.Count == 0 && _notifyForBoth.Count == 0)
                    return;

                // If we're already notifying, add on top of what we've already got.
                if (_currentlyNotifying)
                    AddToNotifyProcess(type, component);

                // If we aren't currently in the process of notifying, we'll start up that process.
                else
                    StartNotifyProcess(type, component);
            });
        }

        /// <summary>
        /// Starts up the notify process, this only runs when there are no other notifications being sent out.
        /// </summary>
        static async void StartNotifyProcess(ComponentsChangedType type, Component component)
        {
            // We're now notifying.
            _currentlyNotifying = true;

            // Reset the "AddComponents" or "RemoveComponents".
            _addedComponents = new List<KeyValuePair<Type, List<Component>>>();
            _removedComponents = new List<KeyValuePair<Type, List<Component>>>();

            // Add the component into the correct one.
            AddComponentToCorrectDictionary(type, component);

            // Next, launch all of the events that don't have a delay.
            NotifyEventsWithoutDelay(type, component);

            // Then, wait the delay period, and if things changed within that period, continue waiting.
            await WaitDelayPeriod();

            // Finally, send out the notifications!
            SendNotifications();

            _currentlyNotifying = false;
        }

        /// <summary>
        /// When we are already in a notify process, and the components have changed, this will add more items to the delay process.
        /// </summary>
        static async void AddToNotifyProcess(ComponentsChangedType type, Component component)
        {
            // We're about to make a change.
            _changedAddedOrRemovedComponents = true;

            // Notify everything that isn't part of the delay.
            NotifyEventsWithoutDelay(type, component);

            // Wait until we are in a delay.
            while (!_currentlyInDelay && _currentlyNotifying)
                await Task.Delay(10);

            //// If we haven't entered a delay and the notifying process has ended, that means we were just a little bit too late, so we'll start a new process.
            //if (!CurrentlyNotifying)
            //{
            //    await StartNotifyProcess(type, component);
            //    return;
            //}

            // Otherwise, add the component.
            AddComponentToCorrectDictionary(type, component);
        }

        static async Task WaitDelayPeriod()
        {
            // We're now in the delay part.
            _currentlyInDelay = true;

            // Keep on waiting until things haven't changed.
            do
            {
                _changedAddedOrRemovedComponents = false;
                await Task.Delay(_delayPeriod);
            } while (_changedAddedOrRemovedComponents);

            // We're no longer in the delay part.
            _currentlyInDelay = false;
        }

        static void SendNotifications()
        {
            // First, join up the places we need to notify to the components that have been added/removed.
            var added = GenerateLink(_notifyForAddedComponents, _addedComponents);
            var removed = GenerateLink(_notifyForRemovedComponents, _removedComponents);
            var both = GenerateLinkForBoth(_notifyForBoth, _addedComponents, _removedComponents);

            // Then, send out all of the components!
            SendComponents(added, ComponentsChangedType.Added);
            SendComponents(removed, ComponentsChangedType.Removed);
            SendComponentsForBoth(both);
        }

        private static List<Tuple<List<ComponentManagerNotifyDetails>, List<Component>>> GenerateLink(Dictionary<Type, List<ComponentManagerNotifyDetails>> notifiers, List<KeyValuePair<Type, List<Component>>> components)
        {
            var res = new List<Tuple<List<ComponentManagerNotifyDetails>, List<Component>>>();

            // Go through all of the different types of components.
            for (var i = 0; i < components.Count; i++)
            {
                // If there are any places that are interested in this type of component, then add them, linking the two.
                if (notifiers.TryGetValue(components[i].Key, out List<ComponentManagerNotifyDetails> events))
                    res.Add(new Tuple<List<ComponentManagerNotifyDetails>, List<Component>>(events, components[i].Value));
            }

            return res;
        }

        static List<Tuple<List<ComponentManagerNotifyDetails>, List<Component>, List<Component>>> GenerateLinkForBoth(Dictionary<Type, List<ComponentManagerNotifyDetails>> notifiers, List<KeyValuePair<Type, List<Component>>> added, List<KeyValuePair<Type, List<Component>>> removed)
        {
            var res = new List<Tuple<List<ComponentManagerNotifyDetails>, List<Component>, List<Component>>>();
            var removedClone = removed.ToList();

            // Go through all of the different types of added components, and everytime we encounter the same type for the removed components, remove it so we don't send duplicates!
            for (var i = 0; i < added.Count; i++)
            {
                var key = added[i].Key;

                // Look to see if this exists in the removed items, if so, get and remove it in order to prevent duplicates!
                var removedItemIndex = removedClone.FindIndex(c => c.Key == key);
                var successfullyRemovedItem = removedItemIndex != -1;
                var removedItem = successfullyRemovedItem ? removedClone[removedItemIndex].Value : null;
                if (successfullyRemovedItem)
                    removedClone.RemoveAt(removedItemIndex);

                // Check if there are any places interested in this type, and if so, add them to the new result.
                if (notifiers.TryGetValue(key, out var events))
                    res.Add(new Tuple<List<ComponentManagerNotifyDetails>, List<Component>, List<Component>>(events, added[i].Value, removedItem));
            }

            // Finally, go through the (probably shortened) removed items just in case there were any types in there that weren't in the added list.
            // We don't need to check if the added contained this type since we know it won't because otherwise it would have removed this item.
            for (var i = 0; i < removedClone.Count; i++)
            {
                var key = removedClone[i].Key;

                // Check if there are any places interested in this type, and if so, add them to the new result.
                if (notifiers.TryGetValue(key, out var events))
                    res.Add(new Tuple<List<ComponentManagerNotifyDetails>, List<Component>, List<Component>>(events, null, removedClone[i].Value));
            }

            return res;
        }

        static void SendComponents(List<Tuple<List<ComponentManagerNotifyDetails>, List<Component>>> linked, ComponentsChangedType type)
        {
            for (var i = 0; i < linked.Count; i++)
                for (var j = 0; j < linked[i].Item1.Count; j++)
                    linked[i].Item1[j].CodeToRun(type == ComponentsChangedType.Added ? new ComponentsChangedEventArgs(linked[i].Item2, type) : new ComponentsChangedEventArgs(type, linked[i].Item2));
        }

        static void SendComponentsForBoth(List<Tuple<List<ComponentManagerNotifyDetails>, List<Component>, List<Component>>> linked)
        {
            for (var i = 0; i < linked.Count; i++)
                for (var j = 0; j < linked[i].Item1.Count; j++)
                    linked[i].Item1[j].CodeToRun(new ComponentsChangedEventArgs(linked[i].Item2, ComponentsChangedType.Both, linked[i].Item3));
        }

        static void NotifyEventsWithoutDelay(ComponentsChangedType type, Component component)
        {
            // Determine which things we need to notify.
            if (!DetermineDictionary(type).TryGetValue(component.GetType(), out var events))
                return;

            var arr = new List<Component>() { component };

            // Go through all of them and set them off.
            for (var i = 0; i < events.Count; i++)
                if (!events[i].Delay)
                    events[i].CodeToRun(type == ComponentsChangedType.Added ? new ComponentsChangedEventArgs(arr, type) : new ComponentsChangedEventArgs(type, arr));
        }

        static Dictionary<Type, List<ComponentManagerNotifyDetails>> DetermineDictionary(ComponentsChangedType type)
        {
            switch (type)
            {
                case ComponentsChangedType.Added:
                    return _notifyForAddedComponents;
                case ComponentsChangedType.Removed:
                    return _notifyForRemovedComponents;
                case ComponentsChangedType.Both:
                    return _notifyForBoth;
            }

            throw new Exception("MAJOR Error in ABSoftware Core, Component Manager");
        }

        private static void AddComponentToCorrectDictionary(ComponentsChangedType type, Component component)
        {
            List<KeyValuePair<Type, List<Component>>> arr;
            var compType = component.GetType();

            // Determine which array is correct.
            if (type == ComponentsChangedType.Added)
                arr = _addedComponents;
            else
                arr = _removedComponents;

            // Next, check to see if there already a spot for this type of component.
            var item = arr.FindIndex(i => i.Key == compType);

            // If there isn't one yet, we'll add it now.
            if (item == -1)
            {
                arr.Add(new KeyValuePair<Type, List<Component>>(compType, new List<Component>()));
                item = arr.Count - 1;
            }

            // Finally, add the component to the array.
            arr[item].Value.Add(component);

        }

        #endregion
    }
}
