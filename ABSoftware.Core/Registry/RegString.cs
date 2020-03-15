using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Registry
{
    public class RegString : Item
    {
        public char[] Text;

        public RegString(char[] name) : base(name) { }

        public RegString(char[] name, char[] text) : base(name) => Text = text;
    }
}
