using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Registry
{
    /// <summary>
    /// An item in the ABSoftware registry.
    /// </summary>
    public abstract class Item
    {

        /// <summary>
        /// The name for this item.
        /// </summary>
        public char[] Name;

        public Item(char[] name) => Name = name;
    }
}
