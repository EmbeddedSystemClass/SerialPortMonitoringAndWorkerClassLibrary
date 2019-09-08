using System;

namespace Kalikolandia
{
    public class ChangedStateEventArgs : EventArgs
    {
        public SerialPortWorkerState WorkerState { get; private set; }

        public ChangedStateEventArgs(SerialPortWorkerState workerState)
        {
            WorkerState = workerState;
        }
    }
}