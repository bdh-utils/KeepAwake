using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace KeepAwake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Keeps the PC awake via SetThreadExecutionState, and lives in the system
    /// tray when the window is closed.
    /// </summary>
    public partial class MainWindow : Window
    {
        private Forms.NotifyIcon? _trayIcon;
        private Drawing.Icon? _appIcon;

        private bool _keepDisplayOn = true;
        private bool _isRunning;
        private bool _reallyExiting;
        private bool _trayTipShown;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadIcon();
            SetupTrayIcon();
            _keepDisplayOn = DisplayCheck.IsChecked == true;
            UpdateStatusUi();
        }

        // ---- Icon / tray setup --------------------------------------------

        private void LoadIcon()
        {
            var uri = new Uri("pack://application:,,,/Resources/KeepAwake.ico", UriKind.Absolute);

            // Window/taskbar icon (WPF).
            Icon = BitmapFrame.Create(uri);

            // Tray icon (WinForms) needs a System.Drawing.Icon.
            var info = System.Windows.Application.GetResourceStream(uri);
            if (info != null)
            {
                _appIcon = new Drawing.Icon(info.Stream);
            }
        }

        private void SetupTrayIcon()
        {
            var menu = new Forms.ContextMenuStrip();

            var openItem = new Forms.ToolStripMenuItem("Open KeepAwake");
            openItem.Font = new Drawing.Font(openItem.Font, Drawing.FontStyle.Bold);
            openItem.Click += (_, _) => ShowWindow();
            menu.Items.Add(openItem);

            var toggleItem = new Forms.ToolStripMenuItem("Start");
            toggleItem.Click += (_, _) =>
            {
                if (_isRunning) StopKeepAwake(); else StartKeepAwake();
            };
            menu.Items.Add(toggleItem);

            menu.Items.Add(new Forms.ToolStripSeparator());

            var aboutItem = new Forms.ToolStripMenuItem("About");
            aboutItem.Click += (_, _) => ShowAbout();
            menu.Items.Add(aboutItem);

            var exitItem = new Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (_, _) => ExitApplication();
            menu.Items.Add(exitItem);

            // Keep the toggle label in sync each time the menu opens.
            menu.Opening += (_, _) => toggleItem.Text = _isRunning ? "Stop" : "Start";

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = _appIcon,
                Visible = true,
                Text = "KeepAwake — Stopped",
                ContextMenuStrip = menu
            };
            _trayIcon.DoubleClick += (_, _) => ShowWindow();
        }

        // ---- Keep-awake engine --------------------------------------------

        private void StartKeepAwake()
        {
            if (_isRunning) return;
            _isRunning = true;
            ApplyExecutionState();
            UpdateStatusUi();
        }

        private void StopKeepAwake()
        {
            if (!_isRunning) return;
            _isRunning = false;
            // Drop the requirement so the PC may sleep again.
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            UpdateStatusUi();
        }

        /// <summary>
        /// Tells Windows to keep the system (and optionally the display) awake
        /// until we clear the requirement. ES_CONTINUOUS makes the request
        /// persist on this thread rather than being a one-shot reset.
        /// </summary>
        private void ApplyExecutionState()
        {
            var flags = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;
            if (_keepDisplayOn)
            {
                flags |= EXECUTION_STATE.ES_DISPLAY_REQUIRED;
            }
            SetThreadExecutionState(flags);
        }

        // ---- UI helpers ---------------------------------------------------

        private void UpdateStatusUi()
        {
            StartButton.IsEnabled = !_isRunning;
            StopButton.IsEnabled = _isRunning;

            if (_isRunning)
            {
                // Active state uses the brand accent (bdh-utils #F15025).
                StatusDot.Fill = (System.Windows.Media.Brush)FindResource("BrandAccent");
                StatusText.Text = _keepDisplayOn ? "Running — display kept on" : "Running";
                if (_trayIcon != null) _trayIcon.Text = "KeepAwake — Running";
            }
            else
            {
                // Idle state uses the brand muted token (bdh-utils #5A5E5A).
                StatusDot.Fill = (System.Windows.Media.Brush)FindResource("BrandMuted");
                StatusText.Text = "Stopped";
                if (_trayIcon != null) _trayIcon.Text = "KeepAwake — Stopped";
            }
        }

        // ---- Window / tray interaction ------------------------------------

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
            Activate();
            Topmost = true;   // bring to front…
            Topmost = false;  // …without staying pinned.
        }

        private void ExitApplication()
        {
            _reallyExiting = true;
            StopKeepAwake();

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
            _appIcon?.Dispose();

            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // The close button minimises to the tray instead of quitting.
            if (!_reallyExiting)
            {
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;

                if (!_trayTipShown && _trayIcon != null)
                {
                    _trayIcon.ShowBalloonTip(2000, "KeepAwake",
                        "Still running in the tray. Right-click the icon to exit.",
                        Forms.ToolTipIcon.Info);
                    _trayTipShown = true;
                }
                return;
            }

            base.OnClosing(e);
        }

        // ---- Control event handlers ---------------------------------------

        private void StartButton_Click(object sender, RoutedEventArgs e) => StartKeepAwake();

        private void StopButton_Click(object sender, RoutedEventArgs e) => StopKeepAwake();

        private void AboutLink_Click(object sender, RoutedEventArgs e) => ShowAbout();

        private void ShowAbout()
        {
            var about = new AboutWindow { Owner = IsVisible ? this : null };
            about.ShowDialog();
        }

        private void DisplayCheck_Changed(object sender, RoutedEventArgs e)
        {
            _keepDisplayOn = DisplayCheck.IsChecked == true;

            // Re-apply immediately so the change takes effect while running.
            if (_isRunning)
            {
                ApplyExecutionState();
                UpdateStatusUi();
            }
        }

        // ---- Win32 interop ------------------------------------------------

        [Flags]
        private enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002
        }

        [DllImport("kernel32.dll")]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
    }
}
