using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Diagnostics;
using System.Threading;

namespace CircularProgress
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int tickCount = 0;
        private int maxLimit = 99;
        private bool loaded = false;
        private bool mainWindow = true;
        private static bool eval = false;
        private static object lockObj = new object();
        PerformanceCounter perfCPU = null;
        System.Windows.Threading.DispatcherTimer timer1 = new System.Windows.Threading.DispatcherTimer();
        private delegate void OneArgDelegate(int arg);
        // These are backups, in the event that App resources cannot be found...
        SolidColorBrush br1 = new SolidColorBrush(Colors.Green);
        SolidColorBrush br2 = new SolidColorBrush(Colors.Yellow);
        SolidColorBrush br3 = new SolidColorBrush(Colors.Orange);
        SolidColorBrush br4 = new SolidColorBrush(Colors.Red);

        /// <summary>
        /// An event that the MainViewModel can subscribe to.
        /// </summary>
        public static event EventHandler<Dictionary<int, string>> WindowDeactivatedEvent;

        /// <summary>
        /// Helper method for the ResourceManager
        /// </summary>
        /// <param name="tag">the parameter to find</param>
        /// <returns>the value of the parameter</returns>
        private static string LookupString(string tag) => Properties.Resources.ResourceManager.GetString(tag, System.Globalization.CultureInfo.CurrentUICulture);

        #region [Constructors]
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(bool main) : base()
        {
            InitializeComponent();

            // We're using this as a flag to properly set
            // [this.Topmost] vs [App.Current.MainWindow.Topmost]
            // when mulitple window instances exist.
            mainWindow = main;
        }
        #endregion [Constructors]

        /// <summary>
        /// Window Event
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer1.Tick += new EventHandler(timer_Tick);
            timer1.Interval = new TimeSpan(0, 0, 0, 2, 500); //2.5 second refresh rate
            timer1.Start();

            if (e.Source is Window wnd)
            {
                wnd.ShowInTaskbar = false;
                // Bottom right corner of main display...
                wnd.Top = (SystemParameters.WorkArea.Bottom - wnd.Height) - 90;
                wnd.Left = (SystemParameters.WorkArea.Right - wnd.Width) - 60;

                #region [app.config settings]
                try
                {
                    //C:\Users\[User]\AppData\Local\[AssemblyInfo.cs-CompanyName]\[AppTitle-Hash]\[Version]\user.config

                    // Get our last X coord for the window...
                    string lastX = CircularProgress.Properties.Settings.Default.LastX;
                    // Will be empty when running for the first time or from a different machine.
                    if (string.IsNullOrEmpty(lastX))
                        Properties.Settings.Default.LastX = wnd.Left.ToString();
                    else
                        wnd.Left = Convert.ToDouble(lastX);

                    // Get our last Y coord for the window...
                    string lastY = CircularProgress.Properties.Settings.Default.LastY;
                    // Will be empty when running for the first time or from a different machine.
                    if (string.IsNullOrEmpty(lastY))
                        Properties.Settings.Default.LastY = wnd.Top.ToString();
                    else
                        wnd.Top = Convert.ToDouble(lastY);

                    // Get our CPU % limit trigger...
                    string limit = CircularProgress.Properties.Settings.Default.MaxLimit;
                    // Will be empty when running for the first time or from a different machine.
                    if (string.IsNullOrEmpty(limit))
                        Properties.Settings.Default.MaxLimit = maxLimit.ToString();
                    else
                        maxLimit = Convert.ToInt32(limit);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write($"Issue reading user.config: {ex.Message}", LogLevel.Error);
                }
                #endregion [app.config settings]
            }

            // This consumes the most startup time. (WMI call)
            perfCPU = new PerformanceCounter("Processor Information", "% Processor Time", "_Total", true);

            // We want to run with minimal resource usage.
            Process pro = Process.GetCurrentProcess();
            pro.PriorityClass = ProcessPriorityClass.Idle;

            // The Topmost setting can be squir​relly.
            // Other measures are taken later to account for it.
            this.Topmost = true;
            this.Activate();

            loaded = true;
        }

        /// <summary>
        /// Timer Event
        /// </summary>
        private void timer_Tick(object sender, EventArgs e)
        {
            if (!loaded)
                return;

            int newVal = (int)perfCPU?.NextValue();

            // Perform our effect of drawing another ring with a second update...
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += delegate
            {
                // Schedule the secondary ring update in the UI thread...
                mainGrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new OneArgDelegate(UpdateUserInterface2), newVal);
                timer.Stop();
            };
            timer.Start();


            // Schedule the main ring update in the UI thread...
            mainGrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new OneArgDelegate(UpdateUserInterface), newVal);

            #region [Or invoke the current dispatcher directly]
            /*
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                rpbCPU.Value = (int)perfCPU.NextValue();
                tbCPU.Text = $"CPU: {rpbCPU.Value}%";
                this.Title = $"CPU: {rpbCPU.Value}%";
                if (rpbCPU.Value > 25)
                {
                    // Application-wide resource access...
                    rpbCPU.RingBrush = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushMedium");
                    
                    // Local window resource access...
                    //rpbCPU.RingBrush = (LinearGradientBrush)Resources["ProgressBrushMedium"];
                }
                else
                {
                    rpbCPU.RingBrush = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushLow");
                }

            });
            */
            #endregion [Or invoke the current dispatcher directly]

        }

        /// <summary>
        /// Control Event
        /// </summary>
        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                    // Save our new window position...
                    System.Windows.Threading.DispatcherTimer positionTimer = new System.Windows.Threading.DispatcherTimer();
                    positionTimer.Interval = TimeSpan.FromSeconds(5);
                    positionTimer.Tick += delegate
                    {
                        Properties.Settings.Default.LastX = this.Left.ToString();
                        Properties.Settings.Default.LastY = this.Top.ToString();
                        positionTimer.Stop();
                    };
                    positionTimer.Start();
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                    Close();
                else if (e.MiddleButton == MouseButtonState.Pressed)
                    CreateNewWindowInstance();
            }
            catch (Exception) { }
            finally
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Window Event
        /// </summary>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            try
            {
                this.Topmost = true;

                #region [Reinforce Topmost]
                if (mainWindow)
                {
                    IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle;
                    App.Current.Dispatcher.BeginInvoke(new Action(async () => await RetryTopMost(hwnd)));
                }
                else
                {
                    IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    App.Current.Dispatcher.BeginInvoke(new Action(async () => await RetryTopMost(hwnd)));
                }
                #endregion [Reinforce Topmost]

                WindowDeactivatedEvent?.Invoke(null, new Dictionary<int, string>()
                {   // You can use the int as an event identifier, or add data into the string...
                    { 1, $"Event_WindowDeactivated[{DateTime.Now.ToLongTimeString()}]" }
                });
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Window Event
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer1?.Stop();
            perfCPU?.Dispose();
        }

        /// <summary>
        /// This will run on the UI thread, so any access to control properties is safe.
        /// </summary>
        /// <param name="data">CPU usage percent</param>
        private void UpdateUserInterface(int data)
        {
            rpbCPU.Value = data;
            tbCPU.Text = $"{data}%";
            this.Title = $"CPU: {data}%";

            // Try to pin down who is causing the CPU spike...
            if ((maxLimit > 0) && (data >= maxLimit) && mainWindow) // maxLimit=0 (OFF)
            {
                lock (lockObj)
                {
                    if (!eval)
                    { // Only allow one evaluation at a time
                        var thread = new Thread(() =>
                        {
                            try
                            {
                                eval = true;
                                int coreCount = Environment.ProcessorCount;
                                // Only look at processes that have been in memory for more than 3 seconds...
                                var procs = Process.GetProcesses().Where(pr => pr.TotalProcessorTime >= TimeSpan.FromSeconds(3));
                                foreach (Process p in procs)
                                {   // Walk through the process buffer...
                                    try
                                    {   // Create a PerformanceCounter based on the process we're evaluating...
                                        using (PerformanceCounter perfTest = new PerformanceCounter("Process", "% Processor Time", p.ProcessName, true))
                                        {
                                            float tmp = perfTest.NextValue();
                                            Thread.Sleep(1);
                                            tmp = perfTest.NextValue();
                                            //https://stackoverflow.com/questions/9501645/performance-counter-cpu-usage-for-current-process-is-more-than-100
                                            tmp /= (float)coreCount;
                                            // See if any one process is greater than our maxLimit...
                                            if (tmp >= (maxLimit / coreCount))
                                            {   // NOTE: If we appear in the list, it's probably because of this current operation.
                                                Logger.Instance.Write(new string('-', 42), LogLevel.Info);
                                                Logger.Instance.Write($"  Process.: {p.ProcessName}", LogLevel.Info);
                                                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                                                    Logger.Instance.Write($"  Title...: {p.MainWindowTitle}", LogLevel.Info);
                                                Logger.Instance.Write($"  CPU.....: {tmp.ToString("0.0")}%", LogLevel.Info);
                                                Logger.Instance.Write($"  Time....: {p.TotalProcessorTime.ToTimeString()}", LogLevel.Info);
                                                Logger.Instance.Write($"  Memory..: {p.WorkingSet64.ToFileSize()}", LogLevel.Info);
                                                Logger.Instance.Write($"  Threads.: {p.Threads.Count}", LogLevel.Info);
                                                Logger.Instance.Write($"  Started.: {p.StartTime}", LogLevel.Info);
                                            }
                                        }
                                    }
                                    catch (System.ComponentModel.Win32Exception ex)
                                    {   // We may not have access
                                        Logger.Instance.Write($"Win32: {ex.Message}", LogLevel.Error);
                                    }
                                    catch (InvalidOperationException ex)
                                    {   // The process may go away while evaluating it
                                        Logger.Instance.Write($"InvOpEx: {ex.Message}", LogLevel.Error);
                                    }
                                    catch (System.IO.IOException ex)
                                    {   // Any system I/O issues
                                        Logger.Instance.Write($"IOEx: {ex.Message}", LogLevel.Error);
                                    }
                                    catch (OperationCanceledException ex)
                                    {   // If you were to run a task above
                                        Logger.Instance.Write($"OpCanEx: {ex.Message}", LogLevel.Error);
                                    }
                                    catch (System.IO.InvalidDataException ex)
                                    {   // Very rare
                                        Logger.Instance.Write($"DataEx: {ex.Message}", LogLevel.Error);
                                    }
                                    catch (PlatformNotSupportedException ex)
                                    {   // Not Windows?
                                        Logger.Instance.Write($"PltSupEx: {ex.Message}", LogLevel.Error);
                                    }
                                    catch (AggregateException aex)
                                    {   // If an agg is thrown, then break it down
                                        if (aex.InnerException is NullReferenceException)
                                            Console.WriteLine("InnerException is null!");
                                        else
                                        {
                                            aex.Handle((inner) =>
                                            {
                                                Logger.Instance.Write($"AggEx: {inner.Message}", LogLevel.Error);
                                                return true;
                                            });
                                        }
                                    }
                                    catch (Exception ex)
                                    {   // Catch-all
                                        Logger.Instance.Write($"[{ex.GetType()}]: {ex.Message}", LogLevel.Error);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {  // This is usually a process exiting before it can be evaluated.
                                Console.WriteLine($"[{ex.GetType()}]: {ex.Message}");
                            }
                            finally
                            {
                                eval = false;
                            }
                        });
                        thread.Priority = ThreadPriority.Lowest;
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
            }

            // Update our progress ring in the GUI...
            if (data >= 85)
            {
                // Local window resource access...
                //rpbCPU.RingBrush = (LinearGradientBrush)Resources["ProgressBrushExtreme"];

                // Application-wide resource access...
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushExtreme");
                if (brsh != null)
                    rpbCPU.RingBrush = brsh;
                else
                    rpbCPU.RingBrush = br4;
            }
            else if (data >= 70)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushHigh");
                if (brsh != null)
                    rpbCPU.RingBrush = brsh;
                else
                    rpbCPU.RingBrush = br4;
            }
            else if (data >= 50)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushMediumHigh");
                if (brsh != null)
                    rpbCPU.RingBrush = brsh;
                else
                    rpbCPU.RingBrush = br3;
            }
            else if (data >= 30)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushMedium");
                if (brsh != null)
                    rpbCPU.RingBrush = brsh;
                else
                    rpbCPU.RingBrush = br3;
            }
            else if (data >= 15)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushLowMedium");
                if (brsh != null)
                    rpbCPU.RingBrush = brsh;
                else
                    rpbCPU.RingBrush = br2;
            }
            else
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushLow");
                if (brsh != null)
                    rpbCPU.RingBrush = brsh;
                else
                    rpbCPU.RingBrush = br1;
            }

            // Apply the colored brush to our percent text.
            tbCPU.Foreground = rpbCPU.RingBrush;

            tickCount++;
            if (tickCount == int.MaxValue) { tickCount = 0; }
            if (tickCount % 200 == 0) 
            {   // Reset our Topmost every now and then...
                this.Topmost = true;
                if (mainWindow)
                {
                    IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle;
                    App.Current.Dispatcher.BeginInvoke(new Action(async () => await RetryTopMost(hwnd)));
                }
                else
                {
                    IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    App.Current.Dispatcher.BeginInvoke(new Action(async () => await RetryTopMost(hwnd)));
                }
            }
        }

        /// <summary>
        /// This is our second ring which will give the effect of a delayed ring update.
        /// </summary>
        /// <param name="data">CPU usage percent</param>
        private void UpdateUserInterface2(int data)
        {
            rpbCPU2.Value = data;

            if (data >= 85)
            {
                // Local window resource access...
                //rpbCPU.RingBrush = (LinearGradientBrush)Resources["ProgressBrushExtreme"];

                // Application-wide resource access...
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushExtreme");
                if (brsh != null)
                    rpbCPU2.RingBrush = brsh;
                else
                    rpbCPU2.RingBrush = br4;
            }
            else if (data >= 70)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushHigh");
                if (brsh != null)
                    rpbCPU2.RingBrush = brsh;
                else
                    rpbCPU2.RingBrush = br4;
            }
            else if (data >= 50)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushMediumHigh");
                if (brsh != null)
                    rpbCPU2.RingBrush = brsh;
                else
                    rpbCPU2.RingBrush = br3;
            }
            else if (data >= 30)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushMedium");
                if (brsh != null)
                    rpbCPU2.RingBrush = brsh;
                else
                    rpbCPU2.RingBrush = br3;
            }
            else if (data >= 15)
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushLowMedium");
                if (brsh != null)
                    rpbCPU2.RingBrush = brsh;
                else
                    rpbCPU2.RingBrush = br2;
            }
            else
            {
                var brsh = (LinearGradientBrush)Application.Current.FindResource("ProgressBrushLow");
                if (brsh != null)
                    rpbCPU2.RingBrush = brsh;
                else
                    rpbCPU2.RingBrush = br1;
            }
        }

        /// <summary>
        /// Test method (can be removed).
        /// </summary>
        private void CheckPID(int PID, string processName)
        {
            string name = string.Empty;

            foreach (var instance in new PerformanceCounterCategory("Process").GetInstanceNames())
            {
                if (instance.StartsWith(processName))
                {
                    using (var processId = new PerformanceCounter("Process", "ID Process", instance, true))
                    {
                        if (PID == (int)processId.RawValue)
                        {
                            name = instance;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Iterate through a process's thread collection.
        /// </summary>
        /// <param name="p"><see cref="System.Diagnostics.Process"/></param>
        void EnumerateThreads(System.Diagnostics.Process p)
        {
            try
            {
                foreach (ProcessThread pt in p.Threads)
                {
                    Logger.Instance.Write($"  ThreadId.: {pt.Id}", LogLevel.Info);
                    Logger.Instance.Write($"  State....: {pt.ThreadState}", LogLevel.Info);
                    Logger.Instance.Write($"  Priority.: {pt.PriorityLevel}", LogLevel.Info);
                    Logger.Instance.Write($"  Started..: {pt.StartTime}", LogLevel.Info);
                    Logger.Instance.Write($"  CPU time.: {pt.TotalProcessorTime}", LogLevel.Info);
                }
            }
            catch (InvalidOperationException ex) // The process may go away while enumerating its threads
            {
                Console.WriteLine($"Warning: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex) // We may not have access
            {
                Console.WriteLine($"Warning: {ex.Message}");
            }
            catch (Exception ex) // Remaining
            {
                Console.WriteLine($"Warning: {ex.Message}");
            }
        }

        /// <summary>
        /// We can create another independent instance without having to launch the 
        /// application again. All child windows will be ruled by the original window...
        /// meaning, if you close the original window the rest will follow.
        /// </summary>
        private void CreateNewWindowInstance()
        {
            var newWindowThread = new Thread(ThreadStartingPoint);
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        /// <summary>
        /// <see cref="System.Threading.ThreadStart"/> entry point.
        /// </summary>
        private void ThreadStartingPoint()
        {
            var tempWindow = new MainWindow(false);
            tempWindow.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        #region [Active Control Resizing]
        private double lastWidth = 1;
        private double lastHeight = 1;
        /// <summary>
        /// This is an active resizing method if you choose to let the user resize the window.
        /// NOTE: It still needs some tweaking.
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!loaded)
                return;

            mainGrid.Width = e.NewSize.Width;
            mainGrid.Height = e.NewSize.Height;

            double xChange = 1;
            double yChange = 1;

            if (e.PreviousSize.Width != 0)
                xChange = (e.NewSize.Width / e.PreviousSize.Width);

            if (e.PreviousSize.Height != 0)
                yChange = (e.NewSize.Height / e.PreviousSize.Height);

            foreach (FrameworkElement fe in mainGrid.Children)
            {
                /* Don't want to resize the grid inside the canvas, so add a check from the Grid control. */
                //if (fe is Grid == false)
                //{
                    fe.Height = fe.ActualHeight * yChange;
                    fe.Width = fe.ActualWidth * xChange;
                    //Canvas.SetTop(fe, Canvas.GetTop(fe) * yChange);
                    //Canvas.SetLeft(fe, Canvas.GetLeft(fe) * xChange);

                //}


                //This still needs some work...
                if (fe is TextBlock tb)
                {
                    if (e.NewSize.Width > lastWidth && e.NewSize.Height > lastHeight)
                    {
                        tb.FontSize += (1.1 * (xChange * yChange));
                    }
                    else if (e.NewSize.Width < lastWidth && e.NewSize.Height < lastHeight)
                    {
                        tb.FontSize -= (1.3 * (xChange * yChange));
                        if (tb.FontSize < 14)
                            tb.FontSize = 14;
                    }
                }
            }

            lastWidth = e.NewSize.Width;
            lastHeight = e.NewSize.Height;
        }
        #endregion [Active Control Resizing]

        #region [Topmost Helper]
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const int RetryTopMostDelay = 200;
        const int RetryTopMostMax = 10;
        internal const int GWL_EXSTYLE = -20;
        internal const int WS_EX_TOPMOST = 0x00000008;
        /// <summary>
        /// The code below will retry several times before giving up.
        /// This typically works using only one retry.
        /// </summary>
        /// <param name="hwnd">The main window's <see cref="IntPtr"/></param>
        /// <returns>don't worry about it</returns>
        private static async Task RetryTopMost(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return;

            for (int i = 0; i < RetryTopMostMax; i++)
            {
                await Task.Delay(RetryTopMostDelay);
                int winStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                if ((winStyle & WS_EX_TOPMOST) != 0)
                    break;
                else
                    Debug.WriteLine("NOTICE: Window is not topmost.");
                
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hwnd, int index);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        #endregion [Topmost Helper]
    }
}
