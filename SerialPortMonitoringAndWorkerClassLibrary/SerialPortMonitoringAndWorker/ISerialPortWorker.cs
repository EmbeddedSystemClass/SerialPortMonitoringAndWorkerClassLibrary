namespace Kalikolandia
{
    interface ISerialPortWorker
    {
        void StartThisInMonitoring();

        void PauseThisInMonitoring();

        void StopThisInMonitoring();

        void WorkOnReceivedMessage(string message);
    }
}