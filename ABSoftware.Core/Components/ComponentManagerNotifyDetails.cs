using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Components
{
    /// <summary>
    /// Details about components to notify.
    /// </summary>
    public class ComponentManagerNotifyDetails
    {
        /// <summary>
        /// The code to run when this gets notified.
        /// </summary>
        public Action<ComponentsChangedEventArgs> CodeToRun;

        /// <summary>
        /// Whether to add a short delay.
        /// </summary>
        public bool Delay;

        public ComponentManagerNotifyDetails(Action<ComponentsChangedEventArgs> codeToRun, bool delay)
        {
            CodeToRun = codeToRun;
            Delay = delay;
        }
    }
}
