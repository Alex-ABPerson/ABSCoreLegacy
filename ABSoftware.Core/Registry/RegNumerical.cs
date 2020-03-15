using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Registry
{
    public class RegNumerical : Item
    {
        public long Num;

        public RegNumerical(char[] name) : base(name) { }

        public RegNumerical(char[] name, long num) : base(name) => Num = num;
    }
}
