using System;
using System.Drawing;
using System.Threading;
using Kalikolandia;

namespace SPL
{
    class Program
    {
        private static TrayIconManagement trayIconManagement;
        private static SerialPortMonitoring serialPortMonitoring;
        private static SerialPortLocking locker;
        private static Thread _thread;

        static void Main(string[] args)
        {
            Console.WriteLine("Siemanko! Apka startuje!");

            trayIconManagement = new TrayIconManagement()
            {
                AppTitle = "SPL",
                BaloonTipText = "SPL rozpoczyna działanie!"
            };
            trayIconManagement.OnClickElementInMenu += OnClickInTrayMenu;
            trayIconManagement.StartTrayIcon();
            Console.WriteLine("Utworzono ikonę w tray-u");

            string port = "COM5";
            if (args.Length > 0)
                port = args[0];

            Console.WriteLine($"Port: {port}\n");
            serialPortMonitoring = new SerialPortMonitoring(port);

            serialPortMonitoring.CreateWorkerSet(SPClass.LOCKING);
            locker = (SerialPortLocking)serialPortMonitoring.SerialPortWorkers[SPClass.LOCKING];
            locker.ChangedState += Locker_ChangedState;;

            StartMonitoringThread();
        }

        private static void OnClickInTrayMenu(object sender, MenuItems item)
        {
            switch (item)
            {
                case MenuItems.START:
                    StartMonitoringThread();
                    break;
                case MenuItems.PAUSE:
                    StopMonitoring();
                    break;
                case MenuItems.STOP:
                    ExitApplication();
                    break;
                case MenuItems.UNKNOWN:
                    Console.WriteLine($"[WARNING] {item}: {sender}");
                    break;
                default:
                    break;
            }
        }

        private static void StartMonitoringThread()
        {
            _thread = new Thread(new ThreadStart(serialPortMonitoring.StartMonitoringLoop));
            _thread.Start();
        }

        private static void StopMonitoring()
        {
            serialPortMonitoring.StopMonitoringLoop();
        }

        private static void Locker_ChangedState(object sender, ChangedStateEventArgs e)
        {
            switch (e.WorkerState)
            {
                case SerialPortWorkerState.STARTED:
                    trayIconManagement.ChangeIcon(MenuItems.START);
                    break;
                case SerialPortWorkerState.PAUSED:
                    trayIconManagement.ChangeIcon(MenuItems.PAUSE);
                    break;
                case SerialPortWorkerState.STOPPED:
                    trayIconManagement.ChangeIcon(MenuItems.STOP);
                    break;
                default:
                    break;
            }
        }

        private static void ExitApplication()
        {
            Icon beginExitIcon = new Icon($"{TrayIconManagement.ICONS_LOCATION}/achtung.ico");
            trayIconManagement.ChangeIcon(beginExitIcon);

            if (serialPortMonitoring != null)
                StopMonitoring();

            trayIconManagement.DisposeTrayIcon();
            Environment.Exit(0);
        }
    }
}
