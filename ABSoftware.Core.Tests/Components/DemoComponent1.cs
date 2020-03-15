using ABSoftware.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Tests.Components
{
    /// <summary>
    /// The first demo component.
    /// </summary>
    public class DemoComponent1 : Component
    {
        public int DemoVar;

        public DemoComponent1() { }

        public DemoComponent1(int demoVar) => DemoVar = demoVar;

        public override string Name => "Demo Component 1";

        public override string Description => "Test component (1)";

        protected override bool MatchesInternal(Component second)
        {
            return DemoVar == ((DemoComponent1)second).DemoVar;
        }
    }
}
