using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Components
{
    /// <summary>
    /// A component that is registered in this ABSoftware application.
    /// </summary>
    public abstract class Component
    {

        /// <summary>
        /// The name this component is given.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The description this component is given.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// The internal part of matches - this bit is called AFTER the type has already been checked.
        /// </summary>
        protected abstract bool MatchesInternal(Component second);

        public bool Matches(Component second)
        {
            if (GetType().IsEquivalentTo(second.GetType()))
                return MatchesInternal(second);

            return false;
        }
    }
}
