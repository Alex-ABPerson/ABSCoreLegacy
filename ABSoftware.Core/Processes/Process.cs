using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Processes
{
    public abstract class Process
    {
        internal List<object> UndoParameters;

        public bool WasCancelled { get; internal set; }

        /// <summary>
        /// The name of this process.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Launches when this process is started.
        /// </summary>
        public abstract Task Run();

        /// <summary>
        /// If this process can be undone, this will do that, passing in all of the parameters needed.
        /// </summary>
        public virtual Task Undo(List<object> undoParameters) => null;

        Task SetProcessUndoParameters(List<object> undoParameters) => Task.Run(() => UndoParameters = undoParameters);
    }
}
