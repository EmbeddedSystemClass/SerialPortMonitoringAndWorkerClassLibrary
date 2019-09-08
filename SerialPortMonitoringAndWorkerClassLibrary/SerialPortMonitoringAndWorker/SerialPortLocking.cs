using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Kalikolandia
{
    public class SerialPortLocking : SerialPortWorker
    {
        const string OK = "Access Granted!";
        const string FAIL = "Access Denied!";
        [DllImport("user32")]
        public static extern void LockWorkStation();

        public new static readonly string PREFIX = "AUTHY";

        public new readonly SPClass SP_CLASS_TYPE = SPClass.LOCKING;

        /// <summary>
        /// Opóźnienie (W SEKUNDACH!) nadawane przy ponownym uruchamianiu "SerialMonitor-a", w celu zapobiegnięcia scenariusza:
        /// User się loguje -> urządzenie COM zawiesiło się i wysyła informację, by stacja robocza została zablokowana -> powstaje pętla -> 
        /// -> user musi zrestartować lub (co gorsze) uruchamiać maszynę w trybie awarynjnym i usuwać usługę, by móc bez problemów pracować
        /// </summary>
        public int DelaySecondsBeforeMonitoringRestart
        {
            get
            {
                return _delaySecondsBeforeMonitoringRestart;
            }
            set
            {
                if (value < 10)
                    value = 10;
                else if (value > 60)
                    value = 60;

                _delaySecondsBeforeMonitoringRestart = value;
            }
        }
        private int _delaySecondsBeforeMonitoringRestart = 10;

        public SerialPortLocking()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                PauseThisInMonitoring();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Helper.NLogger.Debug($"Oczekiwanie po wyjściu z pauzy przez: {DelaySecondsBeforeMonitoringRestart} sekund");

                int delayMilisecs = DelaySecondsBeforeMonitoringRestart * 1000;
                Thread.Sleep(delayMilisecs);

                StartThisInMonitoring();
            }
        }

        public override void WorkOnReceivedMessage(string message)
        {
            base.WorkOnReceivedMessage(message);

            if (message.Contains(FAIL))
            {
                LockWorkStation();
                Helper.NLogger.Debug("Workstation has been locked!");
            }
            else
                Helper.NLogger.Debug($"Wiadomosć nie zawiera ciagu znaków: {FAIL}");
        }
    }
}
