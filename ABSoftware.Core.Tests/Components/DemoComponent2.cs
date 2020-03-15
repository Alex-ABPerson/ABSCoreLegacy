using ABSoftware.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Tests.Components
{
    public class DemoComponent2 : Component
    {
        public override string Name => "Demo Component 2";

        public override string Description => "Test component (2)";

        protected override bool MatchesInternal(Component second)
        {
            return true;
        }
    }
}
