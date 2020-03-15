using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Registry
{
    /// <summary>
    /// A group in the ABSoftware registry.
    /// </summary>
    public class Group : Item
    {
        /// <summary>
        /// The items inside this group.
        /// </summary>
        public List<Item> InnerItems = new List<Item>();

        public Group(char[] name) : base(name) { }
    }
}
