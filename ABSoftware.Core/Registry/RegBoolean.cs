using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Registry
{
    public class RegBoolean : Item
    {
        public bool Bool;

        public RegBoolean(char[] name) : base(name) { }

        public RegBoolean(char[] name, bool bl) : base(name) => Bool = bl;
    }
}
