using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABSoftware.Core.Processes
{
    /// <summary>
    /// Manages all current processes.
    /// </summary>
    public static class ProcessManager
    {
        #region Currently Processing
        private static bool _currentlyProcessing;
        public delegate void CurrentlyProcessingChangedEventHandler();


        /// <summary>
        /// Whether we're currently processing or not.
        /// </summary>
        public static bool CurrentlyProcessing
        {
            get => _currentlyProcessing;
            private set
            {
                _currentlyProcessing = value;
                CurrentlyProcessingChanged();
            }
        }

        /// <summary>
        /// Launches when <see cref="CurrentlyProcessing"/> changes.
        /// </summary>
        public static CurrentlyProcessingChangedEventHandler CurrentlyProcessingChanged = () => { };

        #endregion

        #region Main Variables

        static int TotalNumberOfProcessesLeft => HighPriorityProcesses.Count + MediumPriorityProcesses.Count + LowPriorityProcesses.Count;

        /// <summary>
        /// All of the high-priority processes.
        /// </summary>
        public static Queue<Process> HighPriorityProcesses { get; private set; } = new Queue<Process>();

        /// <summary>
        /// All of the medium-priority processes.
        /// </summary>
        public static Queue<Process> MediumPriorityProcesses { get; private set; } = new Queue<Process>();

        /// <summary>
        /// All of the low-priority processes.
        /// </summary>
        public static Queue<Process> LowPriorityProcesses { get; private set; } = new Queue<Process>();

        public static List<Process> PreviouslyRanProcesses { get; private set; } = new List<Process>();

        static bool _runningExecutionThread;
        static bool _cancelTask;

        // If the main thread got the message in time, then it has successfully cancelled the task.
        static bool _cancelTaskSuccessful;

        // If the main thread didn't quite get the message in time, then it has failed to cancel.
        static bool _cancelTaskFailed;

        #endregion

        #region Enqueue

        public static void EnqueueHighPriority(Process process) => Enqueue(process, HighPriorityProcesses);

        public static void EnqueueMediumPriority(Process process) => Enqueue(process, MediumPriorityProcesses);

        public static void EnqueueLowPriority(Process process) => Enqueue(process, LowPriorityProcesses);

        private static void Enqueue(Process process, Queue<Process> queueToGoInto)
        {
            queueToGoInto.Enqueue(process);
        }

        #endregion

        #region Starting/Stopping

        /// <summary>
        /// Starts up the execution thread, which will run indefinitely until "StopExecution" is called.
        /// </summary>
        public static void StartExecution() => MainExecutionThread();

        /// <summary>
        /// Stop the execution thread - NOT RECOMMENDED!
        /// </summary>
        public static void StopExecution() => _runningExecutionThread = false;

        #endregion

        #region Execution

        static void MainExecutionThread()
        {
            // If we're already running the main execution thread, stop here.
            if (_runningExecutionThread)
                return;

            // Start the execution thread.
            _runningExecutionThread = true;
            Task.Run(async () =>
            {
                // Keep going until this thread is stopped.
                while (_runningExecutionThread)
                {
                    // If we've gotten a signal that a task has been cancelled, and we didn't successfully handle it, then we failed.
                    if (_cancelTask && !_cancelTaskSuccessful)
                    {
                        _cancelTaskFailed = true;

                        // If we failed, we also need to stop the execution thread.
                        StopExecution();
                        break;
                    }

                    // If there isn't anything to process, wait a bit, and try again.
                    if (TotalNumberOfProcessesLeft == 0)
                    {
                        await Task.Delay(1);
                        continue;
                    }

                    // However, if there was, start processing.
                    CurrentlyProcessing = true;
                    Process currentProcess = null;

                    // Load them up, loading the higher priority processes first.
                    if (HighPriorityProcesses.Count > 0)
                        currentProcess = HighPriorityProcesses.Dequeue();
                    else if (MediumPriorityProcesses.Count > 0)
                        currentProcess = MediumPriorityProcesses.Dequeue();
                    else if (LowPriorityProcesses.Count > 0)
                        currentProcess = LowPriorityProcesses.Dequeue();

                    // Run the process.
                    await currentProcess.Run();

                    // If there was a request to cancel the task, then undo the task and don't do anything else.
                    if (_cancelTask)
                    {
                        await UndoProcess(currentProcess);
                        _cancelTaskSuccessful = true;
                        continue;
                    }

                    // Otherwise, add it as one of the previously ran tasks.
                    PreviouslyRanProcesses.Add(currentProcess);

                    // Finally, if there aren't anymore left, we'll set "CurrentlyProcessing" to false again.
                    if (TotalNumberOfProcessesLeft == 0)
                        CurrentlyProcessing = false;
                }

            });
        }

        #endregion

        #region Cancellation

        public static async Task CancelAll()
        {
            // Clear out any upcoming processes.
            HighPriorityProcesses.Clear();
            MediumPriorityProcesses.Clear();
            LowPriorityProcesses.Clear();

            // Cancel the one that is running.
            await CancelCurrentProcess();
        }

        public static async Task CancelCurrentProcess()
        {
            // If we aren't even processing right now, do nothing.
            if (!CurrentlyProcessing)
                return;

            // Ask the main thread for a response when cancelling a process.
            _cancelTask = true;

            // Wait for a reply that states that the main thread either successfully cancelled the task or was too late.
            await WaitForCancelTaskSuccessfulOrFailed();
            _cancelTask = false;

            // If we were successful, then we can stop here.
            if (_cancelTaskSuccessful)
            {
                _cancelTaskSuccessful = false;
                return;
            }

            // Otherwise, the process has completely gone through, but, we can get it out of the history.
            var process = PreviouslyRanProcesses.Last();

            // Then, undo it.
            await UndoProcess(process);

            // And, finally, remove it, and start the execution loop back up (because it exits when it fails).
            PreviouslyRanProcesses.RemoveAt(PreviouslyRanProcesses.Count - 1);
            StartExecution();

        }

        #endregion

        #region Undo

        public static Task UndoProcess(Process process) => process.Undo(process.UndoParameters);

        #endregion

        #region Waiting

        static async Task WaitForCancelTaskSuccessfulOrFailed()
        {
            while (!_cancelTaskFailed && !_cancelTaskSuccessful)
                await Task.Delay(1);
        }

        public static async Task WaitUntilStartRunning()
        {
            while (!CurrentlyProcessing)
                await Task.Delay(1);
        }

        public static async Task WaitForAllToComplete()
        {
            while (TotalNumberOfProcessesLeft != 0)
                await Task.Delay(1);
        }

        #endregion
    }
}
