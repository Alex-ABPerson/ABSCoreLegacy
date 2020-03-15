using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Components
{

    public class ComponentsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Whether a component was added or removed.
        /// </summary>
        public ComponentsChangedType Type;

        /// <summary>
        /// The components that were added.
        /// </summary>
        public List<Component> AddedComponents;

        /// <summary>
        /// The components that were removed.
        /// </summary>
        public List<Component> RemovedComponents;

        /// <summary>
        /// Creates event args for components that have been added.
        /// </summary>
        public ComponentsChangedEventArgs(List<Component> added, ComponentsChangedType type)
        {
            Type = type;
            AddedComponents = added;
        }

        /// <summary>
        /// Creates event args for components that have been removed.
        /// </summary>
        public ComponentsChangedEventArgs(ComponentsChangedType type, List<Component> removed)
        {
            Type = type;
            RemovedComponents = removed;
        }

        /// <summary>
        /// Creates event args for components that have been added AND removed.
        /// </summary>
        public ComponentsChangedEventArgs(List<Component> added, ComponentsChangedType type, List<Component> removed)
        {
            Type = type;
            AddedComponents = added;
            RemovedComponents = removed;
        }
    }
}
