using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kalikolandia
{
    static class Helper
    {
        public static Logger NLogger = CreateLogger("SerialPortMonitoring", LogLevel.Trace, true);

        /// <summary>
        /// Zwraca skonfigurowany obiekt Logger
        /// </summary>
        /// <param name="name">Nazwa loggera (nazwa pliku tekstowego czerpie tę nazwę)</param>
        /// <param name="minLogLevel">Minimalny poziom logowania do pliku</param>
        /// <param name="showAllOnConsole">Czy mają być wyświetlane informacje na konsoli (wtedy wyświetlane są wszystkie logi)</param>
        /// <returns>Logger pozwalający na zapis do pliku oraz ewentualnie do pisana na konsoli</returns>
        private static Logger CreateLogger(string name, LogLevel minLogLevel, bool showAllOnConsole = false)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                name = "NoName";

            var config = new NLog.Config.LoggingConfiguration();

            if (showAllOnConsole)
            {
                var logconsole = new NLog.Targets.ConsoleTarget("logconsole") { Layout = "${longdate} ${callsite} ${level} -> ${message}" };
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            }

            string logLocation = "/temp/";

            string fileNameAndLocation = $"{logLocation}{name}.nlog.txt";

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fileNameAndLocation, Layout = "${longdate} ${callsite} ${level} -> ${message}" };
            config.AddRule(minLogLevel, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;

            return LogManager.GetLogger(name);
        }
    }
}
