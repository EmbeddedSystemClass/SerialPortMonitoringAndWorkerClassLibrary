using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SPL
{
    public enum MenuItems
    {
        START,
        PAUSE,
        STOP,
        SHOW_CONSOLE,
        HIDE_CONSOLE,
        UNKNOWN
    }

    public class TrayIconManagement
    {
        public delegate void TrayIconEventHandler(object sender, MenuItems item);
        public event TrayIconEventHandler OnClickElementInMenu;

        public const string ICONS_LOCATION = "./Icons";
        public readonly string ACHTUNG_ICON = $"{ICONS_LOCATION}/achtung.ico";
        readonly string STARTUP_ICON = $"{ICONS_LOCATION}/startup.ico";
        readonly string START_ICON = $"{ICONS_LOCATION}/start.ico";
        readonly string PAUSE_ICON = $"{ICONS_LOCATION}/pause.ico";
        readonly string STOP_ICON = $"{ICONS_LOCATION}/stop.ico";  //In case lag or problems with close app

        const string START_TEXT = "Start";
        const string PAUSE_TEXT = "Pause";
        const string STOP_TEXT = "Stop & Exit";
        const string SHOW_CONSOLE_TEXT = "Show Console";
        const string HIDE_CONSOLE_TEXT = "Hide Console";


        private ConsoleMannager consoleMannager = new ConsoleMannager();
        private NotifyIcon _icon;
        private ContextMenu context;

        public string AppTitle { get; set; } = "MyApp";
        public string BaloonTipText { get; set; } = "Hejka! Twoja apka działa!";
        public int BaloonTipTimeout { get; set; } = 2000;


        public void StartTrayIcon()
        {
            Thread notifyThread = new Thread(
                delegate ()
                {
                    _icon = new NotifyIcon()
                    {
                        Icon = new Icon(STARTUP_ICON),
                        ContextMenu = CreateContextMenu(),
                        Text = AppTitle,
                        Visible = true
                    };

                    _icon.BalloonTipTitle = AppTitle;
                    _icon.BalloonTipIcon = ToolTipIcon.Info;
                    _icon.BalloonTipText = BaloonTipText;
                    _icon.ShowBalloonTip(BaloonTipTimeout);

                    Application.Run();
                }
            );

            notifyThread.Start();
            Thread.Sleep(2000);
        }

        private ContextMenu CreateContextMenu()
        {
            context = new ContextMenu();

            MenuItem startItem = new MenuItem(START_TEXT);
            startItem.Click += OnMenuItemClick;

            MenuItem pauseItem = new MenuItem(PAUSE_TEXT);
            pauseItem.Click += OnMenuItemClick;

            MenuItem stopItem = new MenuItem(STOP_TEXT);
            stopItem.Click += OnMenuItemClick;

            MenuItem showConsoleItem = new MenuItem(SHOW_CONSOLE_TEXT);
            showConsoleItem.Click += OnMenuItemClick_ShowConsole;
            MenuItem hideConsoleItem = new MenuItem(HIDE_CONSOLE_TEXT);
            hideConsoleItem.Click += OnMenuItemClick_HideConsole;


            context.MenuItems.Add((int)MenuItems.START, startItem);
            context.MenuItems.Add((int)MenuItems.PAUSE, pauseItem);
            context.MenuItems.Add((int)MenuItems.STOP, stopItem);

            context.MenuItems.Add((int)MenuItems.SHOW_CONSOLE, showConsoleItem);
            context.MenuItems.Add((int)MenuItems.HIDE_CONSOLE, hideConsoleItem);

            return context;
        }

        private void OnMenuItemClick_ShowConsole(object sender, EventArgs e)
        {
            consoleMannager.ShowConsole();
        }

        private void OnMenuItemClick_HideConsole(object sender, EventArgs e)
        {
            consoleMannager.HideConsole();
        }

        private void OnMenuItemClick(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            if(menuItem != null)
            {
                MenuItems item = MenuItems.UNKNOWN;

                switch (menuItem.Text)
                {
                    case START_TEXT:
                        item = MenuItems.START;
                        break;
                    case PAUSE_TEXT:
                        item = MenuItems.PAUSE;
                        break;
                    case STOP_TEXT:
                        item = MenuItems.STOP;
                        break;
                }

                ChangeIcon(item);
                OnClickElementInMenu?.Invoke(sender, item);
            }
        }

        public void ChangeIcon(MenuItems item)
        {
            if(_icon == null)
            {
                Console.WriteLine("Brak możliwości zmiany ikony. _icon is null");
            }

            switch (item)
            {
                case MenuItems.START:
                    _icon.Icon = new Icon(START_ICON);
                    break;
                case MenuItems.PAUSE:
                    _icon.Icon = new Icon(PAUSE_ICON);
                    break;
                case MenuItems.STOP:
                    _icon.Icon = new Icon(STOP_ICON);
                    break;
                default:
                    break;
            }
        }

        public void ChangeIcon(Icon icon)
        {
            _icon.Icon = icon;
        }

        public void DisposeTrayIcon()
        {
            consoleMannager.ShowConsole();
            Thread.Sleep(500);
            Application.Exit();
            _icon.Dispose();
        }
    }
}
