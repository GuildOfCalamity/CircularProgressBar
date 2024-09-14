using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CircularProgress
{
    public static class Extensions
    {
        #region [Misc Extensions]
        public static string NameOf(this object o)
        {
            return $"{o.GetType().Name} --> {o.GetType().BaseType.Name}";
            // Similar: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name
        }
        #endregion


        #region [Media and Image Extensions]
        /// <summary>
        /// Image helper method
        /// </summary>
        /// <param name="uriPath"></param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.BitmapImage ReturnImageSource(this string uriPath)
        {
            try
            {
                System.Windows.Media.Imaging.BitmapImage holder = new System.Windows.Media.Imaging.BitmapImage();
                holder.BeginInit();
                holder.UriSource = new Uri(uriPath); //new Uri("pack://application:,,,/AssemblyName;component/Resources/logo.png");
                holder.EndInit();
                return holder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReturnImageSource(ERROR): {ex.Message}");
                return null;
            }
        }

        /*
        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a <see cref="System.Windows.Media.ImageSource"/>
        /// </summary>
        /// <param name="image"><see cref="System.Drawing.Image"/></param>
        /// <returns><see cref="System.Windows.Media.ImageSource"/></returns>
        public static System.Windows.Media.ImageSource ConvertImage(this System.Drawing.Image image)
        {
            try
            {
                if (image != null)
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }
            return null;
        }
        */

        public static RadialGradientBrush ChangeBackgroundColor(Color c1, Color c2)
        {
            var gs1 = new GradientStop(c1, 0);
            var gs2 = new GradientStop(c2, 1);
            var gsc = new GradientStopCollection { gs1, gs2 };
            var lgb = new RadialGradientBrush
            {
                GradientStops = gsc
            };

            return lgb;
        }

        public static LinearGradientBrush ChangeBackgroundColor(Color c1, Color c2, Color c3)
        {
            var gs1 = new GradientStop(c1, 0);
            var gs2 = new GradientStop(c2, 0.5);
            var gs3 = new GradientStop(c3, 1);
            var gsc = new GradientStopCollection { gs1, gs2, gs3 };
            var lgb = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = gsc
            };
            return lgb;
        }

        /// <summary>
        /// Gets a contrasting color based on the current color.
        /// var blue = System.Drawing.Color.Orange.GetContrastingColor();
        /// </summary>
        /// <param name="value"><see cref="System.Windows.Media.Color"/></param>
        /// <returns><see cref="System.Windows.Media.Color"/></returns>
        public static Color GetContrastingColor(this System.Windows.Media.Color value)
        {
            var d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double a = 1 - (0.299 * value.R + 0.587 * value.G + 0.114 * value.B) / 255;

            if (a < 0.5)
                d = 0;   // bright colors - black font
            else
                d = 255; // dark colors - white font

            return System.Windows.Media.Color.FromRgb((byte)d, (byte)d, (byte)d);
        }
        #endregion [Media and Image Extensions]


        #region [WPF Dialog Helpers]
        public static void CreateDialog(string msg, string caption = "Dialog", Window owner = null, bool modal = true)
        {
            var w = new Window();

            var sp = new StackPanel
            {
                Background = Brushes.Transparent
            };

            var tb = new TextBlock
            {
                Background = sp.Background,
                FontSize = 24,
                Margin = new Thickness(20),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                FontWeight = FontWeights.Light,
                Text = msg
            };

            sp.Children.Add(tb);

            w.Content = sp;
            w.Title = caption;
            if (owner != null)
            {
                w.Owner = owner;
                w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            w.WindowStyle = WindowStyle.ToolWindow;
            w.Background = ChangeBackgroundColor(Colors.Maroon, Color.FromRgb(140, 50, 9), Colors.DarkRed);
            w.Height = 250;
            w.Width = 700;

            //var m = LogicalTreeHelper.FindLogicalNode(w, "AlwaysOnTop") as MenuItem;
            //m.IsChecked = true;

            if (modal)
                w.ShowDialog();
            else
                w.Show();
        }

        /// <summary>
        /// Only call from main UI Thread
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="caption"></param>
        /// <param name="IsWarning"></param>
        /// <param name="owner"></param>
        /// <param name="modal"></param>
        /// <param name="shadows"></param>
        public static void ShowDialog(string msg, string caption = "Notice", bool IsWarning = false, Window owner = null, bool modal = true, bool shadows = true)
        {
            try
            {
                //The Border class in WPF represents a Border element. The code snippet listed in Listing 2 is C# code that creates a Border, sets its properties, and places it around a Canvas element.
                //Note: Border can have only one child element. If you need to place border around multiple elements, you must place a border around each element.
                Border border = new Border();
                border.Width = 700;
                border.Height = 220;
                if (IsWarning)
                    border.Background = ChangeBackgroundColor(Color.FromRgb(130, 40, 10), Color.FromRgb(100, 0, 0));
                else
                    border.Background = ChangeBackgroundColor(Color.FromRgb(40, 40, 40), Color.FromRgb(10, 10, 10));
                border.BorderThickness = new Thickness(2);
                border.BorderBrush = new SolidColorBrush(Colors.LightGray);
                border.CornerRadius = new CornerRadius(8);
                border.HorizontalAlignment = HorizontalAlignment.Stretch;
                border.VerticalAlignment = VerticalAlignment.Stretch;
                Canvas cnvs = new Canvas();
                cnvs.VerticalAlignment = VerticalAlignment.Stretch;
                cnvs.HorizontalAlignment = HorizontalAlignment.Stretch;
                //System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                //rect.Width = 200;
                //rect.Height = 200;
                //rect.Fill = new SolidColorBrush(Colors.Black);
                //rect.StrokeThickness = 10d;
                //cvns.Children.Add(rect);

                var sp = new StackPanel
                {
                    Background = Brushes.Transparent,
                    Height = border.Height,
                    Width = border.Width
                };

                var tbx = new TextBox()
                {
                    Background = sp.Background,
                    FontSize = 20,
                    AcceptsReturn = true,
                    BorderThickness = new Thickness(0),
                    MaxHeight = border.Height / 2,
                    MinHeight = border.Height / 2,
                    MaxWidth = border.Width / 1.111,
                    MinWidth = border.Width / 1.111,
                    Margin = new Thickness(10, 24, 10, 10),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    FontWeight = FontWeights.Regular,
                    Text = msg
                };

                var btn = new Button()
                {
                    Width = 150,
                    Height = 40,
                    Content = "Close",
                    FontSize = 20,
                    FontWeight = FontWeights.Regular,
                    Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    Margin = new Thickness(10, 10, 10, 10),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = sp.Background
                };
                if (shadows)
                {
                    btn.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 210, G = 210, B = 210 },
                        Direction = 310,
                        ShadowDepth = 1,
                        Opacity = 1,
                        BlurRadius = 2
                    };
                }

                sp.Children.Add(tbx);
                sp.Children.Add(btn);
                cnvs.Children.Add(sp);
                border.Child = cnvs;
                if (shadows)
                {
                    border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 100, G = 100, B = 100 },
                        Direction = 310,
                        ShadowDepth = 6,
                        Opacity = 1,
                        BlurRadius = 10
                    };
                }

                // Create window to hold content
                var w = new Window();
                w.WindowStyle = WindowStyle.None;
                w.AllowsTransparency = true;
                w.Background = Brushes.Transparent;
                w.VerticalAlignment = VerticalAlignment.Center;
                w.HorizontalAlignment = HorizontalAlignment.Center;
                w.Height = border.Height + 20; // add padding for shadow effect
                w.Width = border.Width + 20; // add padding for shadow effect

                // Apply content to new window
                w.Content = border;

                if (string.IsNullOrEmpty(caption))
                    caption = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                w.Title = caption;

                if (owner != null)
                {
                    w.Owner = owner;
                    w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                // Setup a delegate for the window loaded event
                w.Loaded += (s, e) =>
                {
                    // We could add a timer here to self-close
                };

                // Setup a delegate for the window mouse-down event
                w.MouseDown += (ss, ee) =>
                {
                    if (ee.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                    {
                        w.DragMove();
                    }
                    else if (ee.RightButton == System.Windows.Input.MouseButtonState.Pressed)
                    {
                        // There could be a formatting sitch where the close button
                        // is pushed off the window, so provide a backup close method.
                        w.Close();
                    }
                };

                // Setup a delegate for the close button click event
                btn.Click += (sss, eee) =>
                {
                    w.Close();
                };

                // Show our constructed window
                if (modal)
                    w.ShowDialog();
                else
                    w.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"> ShowDialog(ERROR): {ex.Message}");
            }
        }

        /// <summary>
        /// Can be called from any thread.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="caption"></param>
        /// <param name="IsWarning"></param>
        /// <param name="shadows"></param>
        public static void ShowDialogThreadSafe(string msg, string caption = "Notice", bool IsWarning = false, bool shadows = true)
        {
            try
            {
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Threading.Dispatcher.CurrentDispatcher));

                    //The Border class in WPF represents a Border element. The code snippet listed in Listing 2 is C# code that creates a Border, sets its properties, and places it around a Canvas element.
                    //Note: Border can have only one child element. If you need to place border around multiple elements, you must place a border around each element.
                    Border border = new Border();
                    border.Width = 700;
                    border.Height = 220;
                    if (IsWarning)
                        border.Background = ChangeBackgroundColor(Color.FromRgb(130, 40, 10), Color.FromRgb(100, 0, 0));
                    else
                        border.Background = ChangeBackgroundColor(Color.FromRgb(40, 40, 40), Color.FromRgb(10, 10, 10));
                    border.BorderThickness = new Thickness(2);
                    border.BorderBrush = new SolidColorBrush(Colors.LightGray);
                    border.CornerRadius = new CornerRadius(8);
                    border.HorizontalAlignment = HorizontalAlignment.Stretch;
                    border.VerticalAlignment = VerticalAlignment.Stretch;
                    Canvas cnvs = new Canvas();
                    cnvs.VerticalAlignment = VerticalAlignment.Stretch;
                    cnvs.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                    //rect.Width = 200;
                    //rect.Height = 200;
                    //rect.Fill = new SolidColorBrush(Colors.Black);
                    //rect.StrokeThickness = 10d;
                    //cvns.Children.Add(rect);

                    // StackPanel setup
                    var sp = new StackPanel
                    {
                        Background = Brushes.Transparent,
                        Orientation = Orientation.Vertical,
                        Height = border.Height,
                        Width = border.Width
                    };

                    // TextBox setup
                    var tbx = new TextBox()
                    {
                        Background = sp.Background,
                        FontSize = 20,
                        AcceptsReturn = true,
                        BorderThickness = new Thickness(0),
                        MaxHeight = border.Height / 2,
                        MinHeight = border.Height / 2,
                        MaxWidth = border.Width / 1.111,
                        MinWidth = border.Width / 1.111,
                        Margin = new Thickness(10, 24, 10, 10),
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        FontWeight = FontWeights.Regular,
                        Text = msg
                    };

                    // Button setup
                    var btn = new Button()
                    {
                        Width = 150,
                        Height = 40,
                        Content = "Close",
                        FontSize = 20,
                        FontWeight = FontWeights.Regular,
                        Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                        Margin = new Thickness(10, 10, 10, 10),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Background = sp.Background
                    };
                    if (shadows)
                    {
                        btn.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = new Color { A = 255, R = 210, G = 210, B = 210 },
                            Direction = 310,
                            ShadowDepth = 1,
                            Opacity = 1,
                            BlurRadius = 2
                        };
                    }

                    // Image stuff here
                    /*
                    var ib = new System.Windows.Media.ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://application:,,,/Icons/logo.png")));
                    btn.Background = ib;
                    var img = new Image()
                    {
                        Width = 60, 
                        VerticalAlignment = VerticalAlignment.Top, 
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Source = ReturnImageSource(@"pack://application:,,,/Icons/logo.png"),
                    };
                    sp.Children.Add(img);
                    */

                    sp.Children.Add(tbx);
                    sp.Children.Add(btn);
                    cnvs.Children.Add(sp);
                    border.Child = cnvs;
                    if (shadows)
                    {
                        border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = new Color { A = 255, R = 100, G = 100, B = 100 },
                            Direction = 310,
                            ShadowDepth = 6,
                            Opacity = 1,
                            BlurRadius = 10
                        };
                    }

                    // Create window to hold content
                    var w = new Window();
                    w.WindowStyle = WindowStyle.None;
                    w.AllowsTransparency = true;
                    w.Background = Brushes.Transparent;
                    w.VerticalAlignment = VerticalAlignment.Center;
                    w.HorizontalAlignment = HorizontalAlignment.Center;
                    w.Height = border.Height + 20; // add padding for shadow effect
                    w.Width = border.Width + 20; // add padding for shadow effect

                    // Apply content to new window
                    w.Content = border;

                    if (string.IsNullOrEmpty(caption))
                        caption = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                    w.Title = caption;

                    w.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    // Setup a delegate for the window loaded event
                    w.Loaded += (s, e) =>
                    {
                        // We could add a timer here to self-close
                    };

                    // Setup a delegate for the window closed event
                    w.Closed += (s, e) =>
                    {
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                    };

                    // Setup a delegate for the close button click event
                    btn.Click += (s, e) =>
                    {
                        w.Close();
                    };

                    // Setup a delegate for the window mouse-down event
                    w.MouseDown += (s, e) =>
                    {
                        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                        {
                            w.DragMove();
                        }
                        else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
                        {
                            // There could be a formatting sitch where the close button
                            // is pushed off the window, so provide a backup close method.
                            w.Close();
                        }
                    };


                    // Show our constructed window. We're not on the
                    // main UI thread, so we shouldn't use "w.ShowDialog()"
                    w.Show();

                    // Make sure the essential WPF duties are taken care of.
                    System.Windows.Threading.Dispatcher.Run();
                });

                // You can only show a dialog in a STA thread.
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
                thread.Join();

            }
            catch (Exception ex) { Debug.WriteLine($"Couldn't Show Dialog: {ex.Message}"); }
        }
        #endregion [WPF Dialog Helpers]


        #region [Dispatcher Extensions]
        /// <summary>
        /// Invokes the specified <paramref name="action"/> on the given <paramref name="dispatcher"/>.
        /// </summary>
        /// <param name="dispatcher">The dispatcher on which the <paramref name="action"/> executes.</param>
        /// <param name="action">The <see cref="Action"/> to execute.</param>
        /// <param name="priority">The <see cref="DispatcherPriority"/>.  Defaults to <see cref="DispatcherPriority.ApplicationIdle"/></param>
        public static void InvokeAction(this System.Windows.Threading.Dispatcher dispatcher, Action action, System.Windows.Threading.DispatcherPriority priority)
        {
            /*
            [old way]
            dispatcher.Invoke((Action<string>)((x) => { Console.Write(x); }), "annoying");
            [this way]
            dispatcher.InvokeAction(x => Console.Write(X), "yay lol");
            */
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");
            if (action == null)
                throw new ArgumentNullException("action");

            dispatcher.Invoke(action, priority);
        }
        /// <summary>
        /// Invokes the specified <paramref name="action"/> on the given <paramref name="dispatcher"/>.
        /// </summary>
        /// <typeparam name="T">The type of the argument of the <paramref name="action"/>.</typeparam>
        /// <param name="dispatcher">The dispatcher on which the <paramref name="action"/> executes.</param>
        /// <param name="action">The <see cref="Action{T}"/> to execute.</param>
        /// <param name="arg">The first argument of the action.</param>
        /// <param name="priority">The <see cref="DispatcherPriority"/>.  Defaults to <see cref="DispatcherPriority.ApplicationIdle"/></param>
        public static void InvokeAction<T>(this System.Windows.Threading.Dispatcher dispatcher, Action<T> action, T arg, System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.ApplicationIdle)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");
            if (action == null)
                throw new ArgumentNullException("action");

            dispatcher.Invoke(action, priority, arg);
        }
        /// <summary>
        /// Invokes the specified <paramref name="action"/> on the given <paramref name="dispatcher"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument of the <paramref name="action"/>.</typeparam>
        /// <typeparam name="T2">The type of the second argument of the <paramref name="action"/>.</typeparam>
        /// <param name="dispatcher">The dispatcher on which the <paramref name="action"/> executes.</param>
        /// <param name="action">The <see cref="Action{T1,T2}"/> to execute.</param>
        /// <param name="arg1">The first argument of the action.</param>
        /// <param name="arg2">The second argument of the action.</param>
        /// <param name="priority">The <see cref="DispatcherPriority"/>.  Defaults to <see cref="DispatcherPriority.ApplicationIdle"/></param>
        public static void InvokeAction<T1, T2>(this System.Windows.Threading.Dispatcher dispatcher, Action<T1, T2> action, T1 arg1, T2 arg2, System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.ApplicationIdle)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");
            if (action == null)
                throw new ArgumentNullException("action");

            dispatcher.Invoke(action, priority, arg1, arg2);
        }
        /// <summary>
        /// Invokes the specified <paramref name="action"/> on the given <paramref name="dispatcher"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument of the <paramref name="action"/>.</typeparam>
        /// <typeparam name="T2">The type of the second argument of the <paramref name="action"/>.</typeparam>
        /// <typeparam name="T3">The type of the third argument of the <paramref name="action"/>.</typeparam>
        /// <param name="dispatcher">The dispatcher on which the <paramref name="action"/> executes.</param>
        /// <param name="action">The <see cref="Action{T1,T2,T3}"/> to execute.</param>
        /// <param name="arg1">The first argument of the action.</param>
        /// <param name="arg2">The second argument of the action.</param>
        /// <param name="arg3">The third argument of the action.</param>
        /// <param name="priority">The <see cref="DispatcherPriority"/>.  Defaults to <see cref="DispatcherPriority.ApplicationIdle"/></param>
        public static void InvokeAction<T1, T2, T3>(this System.Windows.Threading.Dispatcher dispatcher, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.ApplicationIdle)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");
            if (action == null)
                throw new ArgumentNullException("action");

            dispatcher.Invoke(action, priority, arg1, arg2, arg3);
        }
        #endregion [Dispatcher Extensions]


        #region [FrameworkElement and DependencyObject Extensions]
        /// <summary>
        /// Find & return a WinForm control based on its resource key name.
        /// </summary>
        public static T FindControl<T>(this System.Windows.Controls.Control control, string resourceKey) where T : System.Windows.Controls.Control
        {
            return (T)control.FindResource(resourceKey);
        }

        /// <summary>
        /// Find & return a WPF control based on its resource key name.
        /// </summary>
        public static T FindControl<T>(this System.Windows.FrameworkElement control, string resourceKey) where T : System.Windows.FrameworkElement
        {
            return (T)control.FindResource(resourceKey);
        }

        /// <summary>
        /// Finds all controls on a Window object by their type.
        /// NOTE: If you're trying to get this to work and finding that your 
        /// Window (for instance) has 0 visual children, try running this method 
        /// in the Loaded event handler. If you run it in the constructor 
        /// (even after InitializeComponent()), the visual children aren't 
        /// loaded yet, and it won't work. 
        /// </summary>
        /// <typeparam name="T">type of control to find</typeparam>
        /// <param name="depObj">the <see cref="DependencyObject"/> to search</param>
        /// <returns><see cref="IEnumerable{T}"/> of controls matching the type</returns>
        public static IEnumerable<T> FindVisualChilds<T>(this DependencyObject depObj) where T : DependencyObject
        {
            /* EXAMPLE USE...
            foreach (TextBlock tb in FindVisualChildren<TextBlock>(window))
            {
                // do something with tb here
            }
            */
            if (depObj == null)
                yield return (T)Enumerable.Empty<T>();

            // NOTE: Switching VisualTreeHelper to LogicalTreeHelpers will cause invisible elements to be included too.
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null)
                    continue;
                if (ithChild is T t)
                    yield return t;
                foreach (T childOfChild in FindVisualChilds<T>(ithChild))
                    yield return childOfChild;
            }
        }

        public static void HideAllVisualChilds<T>(this UIElementCollection coll) where T : UIElementCollection
        {
            // Casting the UIElementCollection into List
            List<FrameworkElement> lstElement = coll.Cast<FrameworkElement>().ToList();

            // Geting all Control from list
            var lstControl = lstElement.OfType<Control>();

            // Hide all Controls
            foreach (Control control in lstControl)
            {
                if (control == null)
                    continue;

                control.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public static IEnumerable<System.Windows.Controls.Control> GetAllControls<T>(this UIElementCollection coll) where T : UIElementCollection
        {
            // Casting the UIElementCollection into List
            List<FrameworkElement> lstElement = coll.Cast<FrameworkElement>().ToList();

            // Geting all Control from list
            var lstControl = lstElement.OfType<Control>();

            // Iterate control objects
            foreach (Control control in lstControl)
            {
                if (control == null)
                    continue;

                yield return control;
            }
        }

        /// <summary>
        /// EXAMPLE: IEnumerable<DependencyObject> cntrls = this.FindUIElements();
        /// NOTE: If you're trying to get this to work and finding that your 
        /// Window (for instance) has 0 visual children, try running this method 
        /// in the Loaded event handler. If you run it in the constructor 
        /// (even after InitializeComponent()), the visual children aren't 
        /// loaded yet, and it won't work. 
        /// </summary>
        /// <param name="parent">some parent control like <see cref="System.Windows.Window"/></param>
        /// <returns>list of <see cref="IEnumerable{DependencyObject}"/></returns>
        public static IEnumerable<DependencyObject> FindUIElements(this DependencyObject parent)
        {
            if (parent == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject o = VisualTreeHelper.GetChild(parent, i);

                foreach (DependencyObject obj in FindUIElements(o))
                {
                    if (obj == null)
                        continue;

                    if (obj is UIElement ret)
                        yield return ret;
                }
            }

            yield return parent;
        }

        /// <summary>
        /// btnTest.RemoveClickEvent();
        /// </summary>
        /// <param name="btn"><see cref="Button"/></param>
        public static void RemoveClickEvent(this Button btn)
        {
            FieldInfo f1 = typeof(Control).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic);
            if (f1 != null)
            {
                object obj = f1.GetValue(btn);
                PropertyInfo pi = btn.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
                System.ComponentModel.EventHandlerList list = (System.ComponentModel.EventHandlerList)pi.GetValue(btn, null);
                list.RemoveHandler(obj, list[obj]);
            }
        }
        #endregion [FrameworkElement and DependencyObject Extensions]


        #region [Task Extensions]
        /// <summary>
        /// Chainable task helper.
        /// var result = await SomeLongAsyncFunction().WithTimeout(TimeSpan.FromSeconds(2));
        /// </summary>
        /// <typeparam name="TResult">the type of task result</typeparam>
        /// <returns><see cref="Task"/>TResult</returns>
        public async static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            Task winner = await (Task.WhenAny(task, Task.Delay(timeout)));

            if (winner != task)
                throw new TimeoutException();

            return await task;   // Unwrap result/re-throw
        }

        /// <summary>
        /// Chainable task helper.
        /// var result = await SomeLongAsyncFunction().WithCancellation(cts.Token);
        /// </summary>
        /// <typeparam name="TResult">the type of task result</typeparam>
        /// <returns><see cref="Task"/>TResult</returns>
        public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancelToken)
        {
            var tcs = new TaskCompletionSource<TResult>();
            var reg = cancelToken.Register(() => tcs.TrySetCanceled());
            task.ContinueWith(ant =>
            {
                reg.Dispose();
                if (ant.IsCanceled)
                    tcs.TrySetCanceled();
                else if (ant.IsFaulted)
                    tcs.TrySetException(ant.Exception.InnerException);
                else
                    tcs.TrySetResult(ant.Result);
            });
            return tcs.Task;  // Return the TaskCompletionSource result
        }

        public static Task<T> WithAllExceptions<T>(this Task<T> task)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            task.ContinueWith(ignored =>
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        Console.WriteLine($"[TaskStatus.Canceled]");
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.SetResult(task.Result);
                        //Console.WriteLine($"[TaskStatus.RanToCompletion({task.Result})]");
                        break;
                    case TaskStatus.Faulted:
                        // SetException will automatically wrap the original AggregateException
                        // in another one. The new wrapper will be removed in TaskAwaiter, leaving
                        // the original intact.
                        Console.WriteLine($"[TaskStatus.Faulted: {task.Exception.Message}]");
                        tcs.SetException(task.Exception);
                        break;
                    default:
                        Console.WriteLine($"[TaskStatus: Continuation called illegally.]");
                        tcs.SetException(new InvalidOperationException("Continuation called illegally."));
                        break;
                }
            });

            return tcs.Task;
        }


        /// <summary>
        ///     Linq extension to be able to fluently wait for all of <see cref="IEnumerable{T}" /> of <see cref="Task" /> 
        ///     just like <see cref="Task.WhenAll(System.Threading.Tasks.Task[])" />.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <returns>An awaitable task</returns>
        /// <remarks></remarks>
        /// <example>
        ///     var something = await foos.Select(foo => BarAsync(foo)).WhenAll();
        /// </example>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static Task WhenAll(this IEnumerable<Task> tasks)
        {
            var enumeratedTasks = tasks as Task[] ?? tasks?.ToArray();

            return Task.WhenAll(enumeratedTasks);
        }

        /// <summary>
        ///     Linq extension to be able to fluently wait for any of <see cref="IEnumerable{T}" /> of <see cref="Task" /> 
        ///     just like <see cref="Task.WhenAll(System.Threading.Tasks.Task[])" />.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <returns>An awaitable task</returns>
        /// <remarks></remarks>
        /// <example>
        ///     var something = await foos.Select(foo => BarAsync(foo)).WhenAll();
        /// </example>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static Task WhenAny(this IEnumerable<Task> tasks)
        {
            var enumeratedTasks = tasks as Task[] ?? tasks.ToArray();

            return Task.WhenAny(enumeratedTasks);
        }

        /// <summary>
        ///     Linq extension to be able to fluently wait for all of <see cref="IEnumerable{T}" /> of <see cref="Task" /> 
        ///     just like <see cref="Task.WhenAll(System.Threading.Tasks.Task{TResult}[])" />.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <returns>An awaitable task</returns>
        /// <remarks></remarks>
        /// <example>
        ///     var bars = await foos.Select(foo => BarAsync(foo)).WhenAll();
        /// </example>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static async Task<IEnumerable<TResult>> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks)
        {
            var enumeratedTasks = tasks as Task<TResult>[] ?? tasks.ToArray();

            var result = await Task.WhenAll(enumeratedTasks);
            return result;
        }

        /// <summary>
        ///     Linq extension to be able to fluently wait for all of <see cref="IEnumerable{T}" /> of <see cref="Task" /> just
        ///     like <see cref="Task.WhenAny(System.Threading.Tasks.Task{TResult}[])" />.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <returns>An awaitable task</returns>
        /// <remarks></remarks>
        /// <example>
        ///     var bar = await foos.Select(foo => BarAsync(foo)).WhenAll();
        /// </example>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static async Task<TResult> WhenAny<TResult>(this IEnumerable<Task<TResult>> tasks)
        {
            var enumeratedTasks = tasks as Task<TResult>[] ?? tasks.ToArray();

            var result = await await Task.WhenAny(enumeratedTasks);
            return result;
        }

        public static async Task<int> EnterAsync(this SemaphoreSlim ss)
        {
            await ss.WaitAsync().ConfigureAwait(false);
            return ss.Release();
        }

        #endregion [Task Extensions]


        #region [Exception Extensions]
        /// <summary>
        /// <para>Creates a log-string from the Exception.</para>
        /// <para>The result includes the stacktrace, innerexception et cetera, separated by <see cref="Environment.NewLine"/>.</para>
        /// </summary>
        /// <param name="ex">The exception to create the string from.</param>
        /// <param name="additionalMessage">Additional message to place at the top of the string, maybe be empty or null.</param>
        /// <returns>formatted string</returns>
        public static string ToLogString(this Exception ex, string additionalMessage = "")
        {
            StringBuilder msg = new StringBuilder();

            if (!string.IsNullOrEmpty(additionalMessage))
            {
                msg.Append($"---[{additionalMessage}]---");
                msg.Append(Environment.NewLine);
            }
            else
            {
                msg.Append($"---[{DateTime.Now.ToString("hh:mm:ss.fff tt")}]---");
                msg.Append(Environment.NewLine);
            }

            if (ex != null)
            {
                try
                {
                    Exception orgEx = ex;

                    msg.Append("[Exception]: ");
                    //msg.Append(Environment.NewLine);
                    while (orgEx != null)
                    {
                        msg.Append(orgEx.Message);
                        msg.Append(Environment.NewLine);
                        orgEx = orgEx.InnerException;
                    }

                    if (ex.Source != null)
                    {
                        msg.Append("[Source]: ");
                        msg.Append(ex.Source);
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.Data != null)
                    {
                        foreach (object i in ex.Data)
                        {
                            msg.Append("[Data]: ");
                            msg.Append(i.ToString());
                            msg.Append(Environment.NewLine);
                        }
                    }

                    if (ex.StackTrace != null)
                    {
                        msg.Append("[StackTrace]: ");
                        msg.Append(ex.StackTrace.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.TargetSite != null)
                    {
                        msg.Append("[TargetSite]: ");
                        msg.Append(ex.TargetSite.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    Exception baseException = ex.GetBaseException();
                    if (baseException != null)
                    {
                        msg.Append("[BaseException]: ");
                        msg.Append(ex.GetBaseException());
                    }
                }
                catch (Exception iex)
                {
                    Debug.WriteLine($"ToLogString: {iex.Message}");
                }
            }
            return msg.ToString();
        }
        #endregion [Exception Extensions]
    }

    /// <summary>
    /// Helper class for changing types.
    /// </summary>
    public static class TConverter
    {
        public static T ChangeType<T>(object value)
        {
            return (T)ChangeType(typeof(T), value);
        }
        public static object ChangeType(Type t, object value)
        {
            System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(t);
            return tc.ConvertFrom(value);
        }
        public static void RegisterTypeConverter<T, TC>() where TC : System.ComponentModel.TypeConverter
        {
            System.ComponentModel.TypeDescriptor.AddAttributes(typeof(T), new System.ComponentModel.TypeConverterAttribute(typeof(TC)));
        }
    }
}
