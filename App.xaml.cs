using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CircularProgress
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string LoggingPath = AppContext.BaseDirectory;
        public static string SystemTitle = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            CircularProgress.Properties.Settings.Default.SettingChanging += Default_SettingChanging;
            CircularProgress.Properties.Settings.Default.SettingsSaving += Default_SettingsSaving;
            CircularProgress.Properties.Settings.Default.SettingsLoaded += Default_SettingsLoaded;
            //if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            //    App.Current.Shutdown();

            bool aIsNewInstance = false;
            _mutex = new Mutex(true, "Circular_Progress_Bar_v1", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                //MessageBox.Show("An instance is already running...");
                App.Current.Shutdown();
            }

            base.OnStartup(e);
        }

        void Default_SettingChanging(object sender, SettingChangingEventArgs e)
        {
            Debug.WriteLine($"[INFO] SettingChanging: {e.SettingName} --> {e.NewValue}");
            // NOTE: We could also call Default.Save() & Default.Reload() here.
        }
        void Default_SettingsLoaded(object sender, SettingsLoadedEventArgs e) => Debug.WriteLine($"[INFO] Settings Loaded: {e.Provider.ApplicationName}");
        void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e) => Debug.WriteLine($"[INFO] Settings Saving: {e.Cancel}");

        void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Debug.WriteLine($"Unhandled exception thrown from Dispatcher {e.Dispatcher.Thread.ManagedThreadId}: {e.Exception}");
                Extensions.ShowDialogThreadSafe(e.Exception.ToLogString(), "DispatcherUnhandledException");
                //MessageBox.Show(e.Exception.ToLogString(), "DispatcherUnhandledException");
                //System.Diagnostics.EventLog.WriteEntry(SystemTitle, $"Unhandled exception thrown from Dispatcher {e.Dispatcher.Thread.ManagedThreadId}: {e.Exception.ToString()}");
                e.Handled = true;
            }
            catch (Exception) { }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Debug.WriteLine($"Unhandled exception thrown: {((Exception)e.ExceptionObject).Message}");
                Extensions.ShowDialogThreadSafe(((Exception)e.ExceptionObject).ToLogString(), "DomainUnhandledException");
                //MessageBox.Show(((Exception)e.ExceptionObject).ToLogString(), "DomainUnhandledException");
                //System.Diagnostics.EventLog.WriteEntry(SystemTitle, $"Unhandled exception thrown:\r\n{((Exception)e.ExceptionObject).ToString()}");
            }
            catch (Exception) { }
        }

        /// <summary>
        /// This is referenced in the App.xaml header.
        /// </summary>
        void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                // NOTE: In the Settings.settings (under Properties) make sure the Scope is set to "User" for each parameter.
                CircularProgress.Properties.Settings.Default.Save();
                CircularProgress.Properties.Settings.Default.Upgrade();
                CircularProgress.Properties.Settings.Default.Reload();
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"Application_Exit: {ex.Message}");
            }
        }
    }
}
