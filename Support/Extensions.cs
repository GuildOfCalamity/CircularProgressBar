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
using System.Windows.Media.Imaging;
using Con = System.Diagnostics.Debug;

namespace CircularProgress
{
    public static class Extensions
    {
        #region [Misc Extensions]
        /// <summary>
        /// Debugging helper.
        /// </summary>
        public static string NameOf(this object o)
        {
            return $"{o.GetType().Name} --> {o.GetType().BaseType.Name}";
            // Similar: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name
        }

        /// <summary>
        /// Converts <see cref="TimeSpan"/> objects to a simple human-readable string.  Examples: 3.1 seconds, 2 minutes, 4.23 hours, etc.
        /// </summary>
        /// <param name="span">The timespan.</param>
        /// <param name="significantDigits">Significant digits to use for output.</param>
        /// <returns></returns>
        public static string ToTimeString(this TimeSpan span, int significantDigits = 3)
        {
            var format = "G" + significantDigits;
            return span.TotalMilliseconds < 1000 ? span.TotalMilliseconds.ToString(format) + " milliseconds"
                : (span.TotalSeconds < 60 ? span.TotalSeconds.ToString(format) + " seconds"
                    : (span.TotalMinutes < 60 ? span.TotalMinutes.ToString(format) + " minutes"
                        : (span.TotalHours < 24 ? span.TotalHours.ToString(format) + " hours"
                                                : span.TotalDays.ToString(format) + " days")));
        }

        //===============================================================================================================
        //Based on the time, it will display a readable sentence as to when that time
        //happened(i.e. 'One second ago' or '2 months ago')
        public static string ToReadableTime(this DateTime value, bool useUTC = false)
        {
            TimeSpan ts;

            if (useUTC)
                ts = new TimeSpan(DateTime.UtcNow.Ticks - value.Ticks);
            else
                ts = new TimeSpan(DateTime.Now.Ticks - value.Ticks);

            double delta = ts.TotalSeconds;
            if (delta < 60)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 120)
            {
                return "a minute ago";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return ts.Days + " days ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }

        /// <summary>
        /// Convert number to bytes size notation
        /// </summary>
        /// <param name="size">bytes</param>
        /// <returns>formatted string</returns>
        public static string ToFileSize(this long size)
        {
            if (size < 1024) { return (size).ToString("F0") + " Bytes"; }
            if (size < Math.Pow(1024, 2)) { return (size / 1024).ToString("F0") + " KB"; }
            if (size < Math.Pow(1024, 3)) { return (size / Math.Pow(1024, 2)).ToString("F2") + " MB"; }
            if (size < Math.Pow(1024, 4)) { return (size / Math.Pow(1024, 3)).ToString("F3") + " GB"; }
            if (size < Math.Pow(1024, 5)) { return (size / Math.Pow(1024, 4)).ToString("F3") + " TB"; }
            if (size < Math.Pow(1024, 6)) { return (size / Math.Pow(1024, 5)).ToString("F3") + " PB"; }
            return (size / Math.Pow(1024, 6)).ToString("F3") + " EB";
        }

        #endregion


        #region [Media and Image Extensions]
        public static void RotateImage(this Image imgControl, double angle = 90.0)
        {
            if (imgControl.Source == null)
                return;

            var img = (BitmapSource)(imgControl.Source);
            var cache = new CachedBitmap(img, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            imgControl.Source = new TransformedBitmap(cache, new RotateTransform(angle));
        }

        public static void TurnBlackAndWhite(this Image imgControl, double alphaThresh = 1.0)
        {
            if (imgControl.Source == null)
                return;

            var img = (BitmapSource)(imgControl.Source);
            imgControl.Source = new FormatConvertedBitmap(img, PixelFormats.Gray8, BitmapPalettes.Gray256, alphaThresh);
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
        /// Simple dialog that may be called from any thread.
        /// </summary>
        /// <remarks>
        /// This dialog uses a dark theme style.
        /// </remarks>
        public static void ShowDialogThreadSafe(string msg, string caption = "Notice", bool nonModal = false, bool isWarning = false, bool shadows = true, string imagePath = "", Action? onClose = null)
        {
            try
            {
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    // Upon entry set the synchronization context to the current dispatcher.
                    System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Threading.Dispatcher.CurrentDispatcher));

                    #region [Common colors]
                    Brush tbForeground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                    Brush btnForeground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                    var clrShadow = new Color { A = 255, R = 1, G = 1, B = 5 };
                    var btnEffect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                         Color = clrShadow,
                         Direction = 310,
                         ShadowDepth = 2.5,
                         Opacity = 0.9,
                         BlurRadius = 2
                    };
                    #endregion

                    // NOTE: Borders can only have one child element. If you need to place borders around
                    //       multiple elements, you must place a border around each element individually.
                    Border border = new Border();
                    border.Width = 600;
                    border.Height = 200;
                    if (isWarning)
                        border.Background = ChangeBackgroundColor(Color.FromRgb(160, 30, 10), Color.FromRgb(90, 20, 5), Color.FromRgb(50, 10, 2));
                    else
                        border.Background = ChangeBackgroundColor(Color.FromRgb(39, 39, 46), Color.FromRgb(22, 22, 27), Color.FromRgb(12, 12, 17));
                    border.BorderThickness = new Thickness(1.5);
                    border.BorderBrush = new SolidColorBrush(Colors.Gray);
                    border.CornerRadius = new CornerRadius(5);
                    border.HorizontalAlignment = HorizontalAlignment.Stretch;
                    border.VerticalAlignment = VerticalAlignment.Stretch;

                    // The canvas will hold the controls and the border will hold the canvas.
                    Canvas cnvs = new Canvas();
                    cnvs.VerticalAlignment = VerticalAlignment.Stretch;
                    cnvs.HorizontalAlignment = HorizontalAlignment.Stretch;

                    #region [Configure FrameworkElements]
                    // StackPanel setup
                    var sp = new StackPanel
                    {
                        Background = Brushes.Transparent,
                        Orientation = Orientation.Vertical,
                        Height = border.Height,
                        Width = border.Width
                    };

                    Image? img = null;
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        if (!imagePath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            imagePath = $"/{imagePath}";

                        // "pack://application:,,,/Assets/MB_Info.png".ReturnImageSource();
                        var imgSource = $"pack://application:,,,{imagePath}".ReturnImageSource();
                        if (imgSource != null)
                        {
                            var ib = new System.Windows.Media.ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(@$"pack://application:,,,{imagePath}")));
                            img = new Image()
                            {
                                Width = 42,
                                Opacity = 0.9,
                                Margin = new Thickness(6),
                                VerticalAlignment = VerticalAlignment.Top,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Source = imgSource,
                            };
                        }
                        else
                        {
                            Con.WriteLine($"[WARNING] Image was not found for the dialog.");
                        }
                    }

                    // Determine if additional margin is needed for the image.
                    Thickness tbPad = new Thickness(0, 0, 0, 0);
                    if (img != null)
                    {
                        tbPad = new Thickness(20, 0, 0, 0);
                        RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
                    }

                    // TextBox setup
                    var tbx = new TextBox()
                    {
                        Background = sp.Background,
                        FontSize = 18,
                        AcceptsReturn = true,
                        BorderThickness = new Thickness(0),
                        MaxHeight = border.Height / 2,
                        MinHeight = border.Height / 2,
                        MaxWidth = border.Width / 1.111,
                        MinWidth = border.Width / 1.111,
                        Padding = tbPad,
                        Margin = new Thickness(10, 25, 10, 15),
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = tbForeground,
                        FontWeight = FontWeights.Regular,
                        Text = msg
                    };

                    // Button setup
                    var btn = new Button()
                    {
                        Width = border.Width / 6,
                        Height = 34,
                        Content = "OK",
                        FontSize = 20,
                        FontWeight = FontWeights.Regular,
                        Foreground = btnForeground,
                        Margin = new Thickness(10, 10, 10, 10),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Background = sp.Background
                    };
                    // Add shadow effect to button.
                    if (shadows)
                        btn.Effect = btnEffect;
                    #endregion

                    // Add textbox to StackPanel.
                    sp.Children.Add(tbx);

                    // Add button to StackPanel.
                    sp.Children.Add(btn);
                    
                    // Add image to Canvas, not StackPanel.
                    if (img != null)
                        cnvs.Children.Add(img);

                    // Add StackPanel to Canvas.
                    cnvs.Children.Add(sp);

                    // Borders can only have one child element.
                    border.Child = cnvs;

                    // Add shadow effect to dialog border.
                    if (shadows)
                    {
                        border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = clrShadow,
                            Direction = 310,
                            ShadowDepth = 6,
                            Opacity = 0.5,
                            BlurRadius = 8
                        };
                    }

                    #region [Create the window and show]
                    // The window will hold our control content.
                    var w = new Window();
                    w.WindowStyle = WindowStyle.None;
                    w.AllowsTransparency = true;
                    w.Background = Brushes.Transparent;
                    w.VerticalAlignment = VerticalAlignment.Center;
                    w.HorizontalAlignment = HorizontalAlignment.Center;
                    // Add padding for shadow effect.
                    w.Height = border.Height + 20;
                    w.Width = border.Width + 20;

                    // Apply content to new window.
                    w.Content = border;

                    if (string.IsNullOrEmpty(caption))
                        caption = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                    // Set the window title.
                    w.Title = caption;

                    // Set the window start position.
                    w.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    #region [Events]
                    // Setup a delegate for the window loaded event.
                    w.Loaded += (s, e) =>
                    {
                        // We could add a timer here to self-close.
                        Con.WriteLine($"[INFO] Thread-safe dialog loaded.");
                    };

                    // Setup a delegate for the window closed event.
                    w.Closed += (s, e) =>
                    {
                        onClose?.Invoke();
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                    };

                    // Setup a delegate to support escape keypress close.
                    w.PreviewKeyUp += (s, e) =>
                    {
                        if (e.Key == System.Windows.Input.Key.Escape)
                            w.Close();
                    };

                    // Setup a delegate for the close button click event.
                    btn.Click += (s, e) =>
                    {
                        w.Close();
                    };

                    // Setup a delegate for the mouse enter event.
                    btn.MouseEnter += (s, e) =>
                    {
                        btn.Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                        if (shadows)
                        {
                            btn.Effect = new System.Windows.Media.Effects.DropShadowEffect
                             {
                                 Color = new Color { A = 255, R = 190, G = 230, B = 253 },
                                 Direction = 90,
                                 ShadowDepth = 0.1,
                                 Opacity = 0.9,
                                 BlurRadius = 24
                             };
                        }
                    };

                    // Setup a delegate for the mouse leave event.
                    btn.MouseLeave += (s, e) =>
                    {
                        btn.Foreground = btnForeground;
                        if (shadows)
                            btn.Effect = btnEffect;
                    };

                    btn.PreviewMouseDown += (s, e) => 
                    {
                        if (shadows)
                            btn.Effect = btnEffect;
                    };

                    // Setup a delegate for the window mouse down event.
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
                    #endregion

                    // We'll ensure topmost since the dialog could become burried.
                    if (nonModal)
                        w.Topmost = true;

                    // Show our constructed window. We might not bo on the
                    // main UI thread, so we shouldn't use "w.ShowDialog()"
                    w.Show();
                    #endregion

                    // Make sure the essential WPF duties are taken care of.
                    System.Windows.Threading.Dispatcher.Run();
                });

                // You can only show a dialog in a STA thread.
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
                // If modal then block until the dialog is closed.
                if (!nonModal)
                    thread.Join();
            }
            catch (Exception ex) { Con.WriteLine($"[WARNING] Could not show dialog: {ex.Message}"); }
        }

        /// <summary>
        /// <see cref="System.Windows.Media.Imaging.BitmapImage"/> helper method.
        /// </summary>
        /// <param name="uriPath">the pack uri path to the image</param>
        /// <returns><see cref="System.Windows.Media.Imaging.BitmapImage"/></returns>
        /// <remarks>
        /// URI Packing can assume the following formats:
        /// 1) Content File...
        ///    "pack://application:,,,/Assets/logo.png"
        ///    https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf?view=netframeworkdesktop-4.8#content-file-pack-uris
        /// 2) Referenced Assembly Resource...
        ///    "pack://application:,,,/AssemblyNameHere;component/Resources/logo.png"
        ///    https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf?view=netframeworkdesktop-4.8#referenced-assembly-resource-file
        /// 3) Site Of Origin...
        ///    "pack://siteoforigin:,,,/Assets/SiteOfOriginFile.xaml"
        ///    https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf?view=netframeworkdesktop-4.8#site-of-origin-pack-uris
        /// </remarks>
        public static System.Windows.Media.Imaging.BitmapImage? ReturnImageSource(this string uriPath)
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
                Con.WriteLine($"[ERROR] ReturnImageSource: {ex.Message}");
                return null;
            }
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
