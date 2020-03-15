using ABSoftware.Core.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Tests.Processes
{
    public class ProcessThatLaunchesCode : Process
    {
        private Action _runCode;
        private Action<List<object>> _undoCode;

        public ProcessThatLaunchesCode(Action runCode, Action<List<object>> undoCode)
        {
            _runCode = runCode;
            _undoCode = undoCode;
        }

        public override string Name => "Test Process";

        public override Task Run() => Task.Run(() => _runCode());

        public override Task Undo(List<object> undoParameters) => Task.Run(() => _undoCode(undoParameters));
    }
}
