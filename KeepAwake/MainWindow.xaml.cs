using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace KeepAwake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Thin UI shell over <see cref="KeepAwakeController"/>: it owns the tray
    /// icon, the mouse-wiggle timer, and reflects controller state into the UI.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Sensible bounds for the user-configurable wiggle interval (seconds).
        private const int MinWiggleSeconds = 5;
        private const int MaxWiggleSeconds = 3600;
        private const int DefaultWiggleSeconds = 30;

        private readonly KeepAwakeController _controller = new(new Win32SystemActivity());
        private readonly DispatcherTimer _wiggleTimer;

        private Forms.NotifyIcon? _trayIcon;
        private Drawing.Icon? _appIcon;

        private bool _initialized;
        private bool _reallyExiting;
        private bool _trayTipShown;

        public MainWindow()
        {
            InitializeComponent();

            // Interval comes from the UI (defaulting to DefaultWiggleSeconds);
            // comfortably under the shortest practical idle timeout.
            _wiggleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(DefaultWiggleSeconds) };
            _wiggleTimer.Tick += (_, _) => _controller.Tick();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadIcon();
            SetupTrayIcon();

            // Sync the controller with the initial UI state, then let the
            // handlers start reacting to user changes.
            _controller.SetKeepDisplayOn(DisplayCheck.IsChecked == true);
            _initialized = true;

            bool executionMode = _controller.Mode == KeepAwakeMode.ExecutionState;
            DisplayCheck.IsEnabled = executionMode;
            WiggleIntervalBox.IsEnabled = !executionMode;
            ApplyWiggleInterval();
            UpdateOptionHint();
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
                if (_controller.IsRunning) StopKeepAwake(); else StartKeepAwake();
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
            menu.Opening += (_, _) => toggleItem.Text = _controller.IsRunning ? "Stop" : "Start";

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
            _controller.Start();
            UpdateWiggleTimer();
            UpdateStatusUi();
        }

        private void StopKeepAwake()
        {
            _controller.Stop();
            UpdateWiggleTimer();
            UpdateStatusUi();
        }

        /// <summary>Run the wiggle timer only while actively wiggling.</summary>
        private void UpdateWiggleTimer()
        {
            if (_controller.IsRunning && _controller.Mode == KeepAwakeMode.MouseWiggle)
            {
                _wiggleTimer.Start();
            }
            else
            {
                _wiggleTimer.Stop();
            }
        }

        // ---- UI helpers ---------------------------------------------------

        private void UpdateStatusUi()
        {
            StartButton.IsEnabled = !_controller.IsRunning;
            StopButton.IsEnabled = _controller.IsRunning;

            if (_controller.IsRunning)
            {
                // Active state uses the brand accent (bdh-utils #F15025).
                StatusDot.Fill = (System.Windows.Media.Brush)FindResource("BrandAccent");
                StatusText.Text = _controller.Mode == KeepAwakeMode.MouseWiggle
                    ? "Running — wiggling the mouse"
                    : (_controller.KeepDisplayOn ? "Running — display kept on" : "Running");
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

        private void UpdateOptionHint()
        {
            OptionHint.Text = MouseWiggleRadio.IsChecked == true
                ? "Nudges the cursor a pixel on the interval above so Windows sees activity."
                : "Prevents the screen from turning off as well as the PC sleeping.";
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

        private void Method_Changed(object sender, RoutedEventArgs e)
        {
            // Radio Checked events fire during XAML init, before the controls
            // and controller are ready — ignore until Loaded has run.
            if (!_initialized) return;

            var mode = MouseWiggleRadio.IsChecked == true
                ? KeepAwakeMode.MouseWiggle
                : KeepAwakeMode.ExecutionState;

            _controller.SetMode(mode);
            DisplayCheck.IsEnabled = mode == KeepAwakeMode.ExecutionState;
            WiggleIntervalBox.IsEnabled = mode == KeepAwakeMode.MouseWiggle;
            UpdateOptionHint();
            UpdateWiggleTimer();
            UpdateStatusUi();
        }

        private void DisplayCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;

            _controller.SetKeepDisplayOn(DisplayCheck.IsChecked == true);
            UpdateStatusUi();
        }

        private void WiggleInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_initialized) return;

            // Apply only while the value is a valid in-range number; otherwise
            // leave the running interval alone until the user finishes typing.
            ApplyWiggleInterval();
        }

        private void WiggleInterval_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;

            // Normalise the box to the interval actually in effect, so an empty
            // or out-of-range entry doesn't linger on screen.
            WiggleIntervalBox.Text = ((int)_wiggleTimer.Interval.TotalSeconds)
                .ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse the interval box and, if it holds a valid in-range value,
        /// push it to the wiggle timer (clamped to [Min, Max]).
        /// </summary>
        private void ApplyWiggleInterval()
        {
            if (int.TryParse(WiggleIntervalBox.Text, out int seconds))
            {
                seconds = Math.Clamp(seconds, MinWiggleSeconds, MaxWiggleSeconds);
                _wiggleTimer.Interval = TimeSpan.FromSeconds(seconds);
            }
        }
    }
}
