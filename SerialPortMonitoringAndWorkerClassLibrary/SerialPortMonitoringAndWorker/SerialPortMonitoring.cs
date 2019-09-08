using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Kalikolandia
{
    public class SerialPortMonitoring
    {
        private Dictionary<SPClass, SerialPortWorker> serialPortWorkers = new Dictionary<SPClass, SerialPortWorker>();
        /// <summary>
        /// Get copy (to prevent adding own workers) of dictionary with SerialPortWorkers
        /// </summary>
        public Dictionary<SPClass, SerialPortWorker> SerialPortWorkers
        {
            get
            {
                Dictionary<SPClass, SerialPortWorker> localSerialPortWorkers = new Dictionary<SPClass, SerialPortWorker>();

                foreach (var worker in serialPortWorkers)
                {
                    localSerialPortWorkers.Add(worker.Key, worker.Value);
                }

                return localSerialPortWorkers;
            }
        }
        public string SerialPortName { get; private set; }
        public int SerialPortBaudrate { get; private set; } = 9600;
        private CancellationTokenSource CTokenSource { get; set; }
        private CancellationToken CToken
        {
            get
            {
                return CTokenSource.Token;
            }
        }

        public SerialPortMonitoring(string serialPortName)
        {
            CTokenSource = new CancellationTokenSource();

            this.SerialPortName = serialPortName;
        }

        public SerialPortMonitoring(string serialPortName, int serialPortBaudrate) : this(serialPortName)
        {
            this.SerialPortBaudrate = serialPortBaudrate;
        }

        public void CreateAllPossibleWorkersSet()
        {
            SPClass[] allSpClassValues = (SPClass[])Enum.GetValues(typeof(SPClass));

            CreateWorkerSet(allSpClassValues);
        }

        public void CreateWorkerSet(params SPClass[] SPClasses)
        {
            foreach (SPClass sPClass in SPClasses)
            {
                switch (sPClass)
                {
                    case SPClass.LOCKING:
                        if(serialPortWorkers.ContainsKey(SPClass.LOCKING) == false)
                            serialPortWorkers.Add(SPClass.LOCKING, new SerialPortLocking());
                        break;

                    case SPClass.NONE:
                    default:
                        break;
                }
            }
        }

        private void StartAllWorkers()
        {
            if (serialPortWorkers.Count < 1)
                CreateAllPossibleWorkersSet();

            for (int i = 0; i < serialPortWorkers.Count; i++)
            {
                SerialPortWorker worker = serialPortWorkers.ElementAt(i).Value;
                worker.StartThisInMonitoring();
                worker.ChangedState += Worker_ChangedState;
            }
        }

        private void Worker_ChangedState(object sender, ChangedStateEventArgs e)
        {
            SerialPortWorkerState workerState = e.WorkerState;
            Helper.NLogger.Debug($"Sender: {sender}, has sent state {workerState}");

            SerialPortWorker worker = sender as SerialPortWorker;
            if(worker != null)
            {
                SPClass workerClass = worker.SP_CLASS_TYPE;
                switch (workerState)
                {
                    case SerialPortWorkerState.STARTED:
                        break;
                    case SerialPortWorkerState.PAUSED:
                        break;
                    case SerialPortWorkerState.STOPPED:
                        RemoveWorkerFromCollection(workerClass);
                        break;
                    default:
                        break;
                }
            }
        }

        private void RemoveWorkerFromCollection(SPClass workerClass)
        {
            serialPortWorkers.Remove(workerClass);
        }

        public void StartMonitoringLoop()
        {
            try
            {
                if(CToken.IsCancellationRequested)
                    CTokenSource = new CancellationTokenSource();

                using (SerialPort serial = new SerialPort(this.SerialPortName, this.SerialPortBaudrate))
                {
                    serial.Open();

                    StartAllWorkers();

                    while (!CToken.IsCancellationRequested)
                    {
                        string newLine = serial.ReadLine();

                        string message = ReadMessage(newLine);

                        SPClass serialPortClassWhichMessageHasBeenSentTo = ReadPrefixAndChooseClass(newLine);

                        SendMessageToWorker(serialPortClassWhichMessageHasBeenSentTo, message);

                        Thread.Sleep(100);

                        Helper.NLogger.Debug("\n\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.NLogger.Error($"\n{ex.Message} \n{ex.InnerException}\n{ex.StackTrace}\n");
            }
        }

        private void SendMessageToWorker(SPClass serialPortClassWhichMessageHasBeenSentTo, string message)
        {
            if (serialPortWorkers.TryGetValue(serialPortClassWhichMessageHasBeenSentTo, out SerialPortWorker worker))
            {
                SerialPortWorkerState currentWorkerState = worker.CurrentState;

                Helper.NLogger.Debug($"Próba wysłania wiadomości: {message} do workera: {worker} ({worker})");
                Helper.NLogger.Debug($"Worker jest w trybie: {currentWorkerState}");

                if (currentWorkerState == SerialPortWorkerState.STARTED)
                {
                    Helper.NLogger.Trace("Usuwanie nadmiaru spacji...");
                    message = Regex.Replace(message, @"\s+", " ");
                    Helper.NLogger.Trace("Do workera zostanie wysłana wiadomosć do przetworzenia.");
                    worker.WorkOnReceivedMessage(message);
                }
                else if(currentWorkerState == SerialPortWorkerState.STOPPED)
                {
                    Helper.NLogger.Trace("Worker zostanie usunięty (bez wysyłania wiadmości!).");
                    RemoveWorkerFromCollection(serialPortClassWhichMessageHasBeenSentTo);
                }
                else     //SerialPortWorkerState.PAUSED
                {
                }
            }
            else
            {
                Helper.NLogger.Warn($"Wokrer o klasie: {serialPortClassWhichMessageHasBeenSentTo} nie istnieje. Wiadomość: >>{message}<< nie może zostać wysłana");
            }
        }

        /// <summary>
        /// Sprawdza prefix zawarty w wiadomości.
        /// </summary>
        /// <returns> Zwraca enuma, żeby było wiadomo która klasa ma obsługiwać zdarzenie.
        /// Jeśli nie znaleziono zgodnej klasy, zwraca SPClass.NONE! </returns>
        private SPClass ReadPrefixAndChooseClass(string msg)
        {
            string prefix;
            try
            {
                prefix = msg.Remove(msg.IndexOf(':')).Trim().ToUpper();
            }
            catch (Exception ex)
            {
                Helper.NLogger.Warn($"ReadPrefixAndChooseClass({msg}): {ex}");
                prefix = msg;
            }

            if(prefix == SerialPortLocking.PREFIX.ToUpper())
                return SPClass.LOCKING;
            else
                return SPClass.NONE;
        }

        /// <returns>Zwraca wiadomość bez prefixa.</returns>
        private string ReadMessage(string msg)
        {
            try
            {
                return msg.Substring(msg.IndexOf(':') + 1).Trim();
            }
            catch (Exception ex)
            {
                Helper.NLogger.Warn($"ReadMessage({msg}): {ex}");
                return msg;
            }
        }

        public void StopMonitoringLoop()
        {
            CTokenSource.Cancel();
        }
    }
}
