using System;

namespace Kalikolandia
{
    public enum SerialPortWorkerState
    {
        STARTED,
        PAUSED,
        STOPPED
    }
    public enum SPClass
    {
        NONE,
        LOCKING
    }

    public abstract class SerialPortWorker : ISerialPortWorker
    {

        public delegate void ChangedStateEventHandler(object sender, ChangedStateEventArgs e);
        public event ChangedStateEventHandler ChangedState;

        /// <summary>
        /// Prefix, który determinuje, gdzie jest kierowana odczytywana wiadomość z SerialPort
        /// </summary>
        public static readonly string PREFIX;

        public readonly SPClass SP_CLASS_TYPE = SPClass.NONE;
        public SerialPortWorkerState CurrentState { get; protected set; }
        

        private void ChangeStateThisInMonitoring(SerialPortWorkerState workerState)
        {
            CurrentState = workerState;
            ChangedState?.Invoke(this, new ChangedStateEventArgs(CurrentState));
            Helper.NLogger.Debug($"Zmieniono stan na: {CurrentState}");
        }

        public virtual void StartThisInMonitoring()
        {
            ChangeStateThisInMonitoring(SerialPortWorkerState.STARTED);
        }

        public virtual void PauseThisInMonitoring()
        {
            ChangeStateThisInMonitoring(SerialPortWorkerState.PAUSED);
        }

        public virtual void StopThisInMonitoring()
        {
            ChangeStateThisInMonitoring(SerialPortWorkerState.STOPPED);
        }

        public virtual void WorkOnReceivedMessage(string message)
        {
            Helper.NLogger.Debug($"WorkOnReceivedMessage otrzymał wiadomosć: {message}");
        }
    }
}
