using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CircularProgress
{
    /// <summary>
    /// Global enum for logging support.
    /// </summary>
    public enum LogLevel
    {
        None = 0,
        Debug = 1,
        Verbose = 2,
        Info = 3,
        Warning = 4,
        Error = 5,
        Success = 6,
        Important = 7,
    }

    // Our delegate for the event methods.
    public delegate void LoggingEventHandler(string message);

    /// <summary>
    /// This is an event-based logger which can be used for file I/O.
    /// Console output can be used via the event-based methods.
    /// </summary>
    /// <remarks>
    /// The default output encoding is UTF-8.
    /// </remarks>
    public class Logger
    {
        private static Logger _instance = null;
        private static DateTime? _date = null;
        private static string _logPath = string.Empty;
        private static string _fileName = string.Empty;

        // Events for hooking in external work module.
        public event LoggingEventHandler OnVerbose;
        public event LoggingEventHandler OnInfo;
        public event LoggingEventHandler OnDebug;
        public event LoggingEventHandler OnWarning;
        public event LoggingEventHandler OnError;
        public event LoggingEventHandler OnSuccess;
        public event LoggingEventHandler OnImportant;

        /// <summary>
        /// Introduces a way to call the class once without worrying about creating a working object.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                    _date = DateTime.Now;
                }
                return _instance;
            }
        }

        public string LogPath
        {
            get
            {
                var title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Application";

                // Determine if our file name needs to be updated.
                if (string.IsNullOrEmpty(_fileName) || _date?.Day != DateTime.Now.Day)
                    _fileName = $"{DateTime.Now.ToString("yyyy-MM-dd")}_{title}.log";

                // The logging path should only need to be updated once.
                if (string.IsNullOrEmpty(_logPath))
                {
                    try
                    {
                        IOrderedEnumerable<DirectoryInfo> logPaths = DriveInfo.GetDrives().Where(di => (di.DriveType == DriveType.Fixed) && di.IsReady).Select(di => di.RootDirectory).OrderByDescending(di => di.FullName);
                        string root = Path.Combine(logPaths.FirstOrDefault().FullName, "Logs", $"{title}");
                        Directory.CreateDirectory(root);
                        _logPath = Path.Combine(root, _fileName);
                    }
                    catch (Exception) // If error then log to the runtime folder.
                    {
                        _logPath = Path.Combine(Environment.CurrentDirectory, _fileName);
                    }
                }

                return _logPath;
            }
        }

        #region [Event-based Methods]
        /// <summary>
        /// Signal the OnDebug <see cref="LoggingEventHandler"/>.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        public void Debug(string message)
        {
            if (!string.IsNullOrEmpty(message))
                OnDebug?.Invoke(message);
        }

        /// <summary>
        /// Signal the OnInfo <see cref="LoggingEventHandler"/>.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        public void Info(string message)
        {
            if (!string.IsNullOrEmpty(message))
                OnInfo?.Invoke(message);
        }

        /// <summary>
        /// Signal the OnError <see cref="LoggingEventHandler"/>.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        public void Error(string message)
        {
            if (!string.IsNullOrEmpty(message))
                OnError?.Invoke(message);
        }

        /// <summary>
        /// Signal the OnWarning <see cref="LoggingEventHandler"/>.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        public void Warning(string message)
        {
            if (!string.IsNullOrEmpty(message))
                OnWarning?.Invoke(message);
        }

        /// <summary>
        /// Signal the OnSuccess <see cref="LoggingEventHandler"/>.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        public void Success(string message)
        {
            if (!string.IsNullOrEmpty(message))
                OnSuccess?.Invoke(message);
        }

        /// <summary>
        /// Signal the OnImportant <see cref="LoggingEventHandler"/>.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        public void Important(string message)
        {
            if (!string.IsNullOrEmpty(message))
                OnImportant?.Invoke(message);
        }
        #endregion

        /// <summary>
        /// Core logging method with <see cref="System.IO.StreamWriter"/>. 
        /// This is usually called from the event delegate handler, but you may call it directly.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        /// <param name="level"><see cref="LogLevel"/></param>
        public void Write(string message, LogLevel level = LogLevel.Info, [System.Runtime.CompilerServices.CallerMemberName] string origin = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            // Format the message.
            message = $"[{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt")} -> {System.IO.Path.GetFileName(filePath)} -> {origin}(line {lineNumber})] {message}";

            // Write to disk.
            WriteToDisk(message, level);
        }

        /// <summary>
        /// Core logging method with <see cref="System.IO.StreamWriter"/>. 
        /// This is usually called from the event delegate handler, but you may call it directly.
        /// </summary>
        /// <param name="message">The text to write to the file.</param>
        /// <param name="level"><see cref="LogLevel"/></param>
        /// <param name="formatParams">additional objects to log</param>
        public void Write(string message, LogLevel level = LogLevel.Info, [System.Runtime.CompilerServices.CallerMemberName] string origin = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, params object[] formatParams)
        {
            if (formatParams != null)
            {
                try { message = String.Format(message, formatParams); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Logger.Write()]: {ex.Message}"); }
            }

            // Format the message
            message = $"[{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt")} -> {System.IO.Path.GetFileName(filePath)} -> {origin}(line {lineNumber})] {message}";

            // Write to disk.
            WriteToDisk(message, level);
        }

        /// <summary>
        /// Consolidated write method for disk I/O.
        /// </summary>
        internal void WriteToDisk(string message, LogLevel level)
        {
            // Fire any listening events.
            switch (level)
            {
                case LogLevel.None: return;
                case LogLevel.Debug: OnDebug?.Invoke(message); break;
                case LogLevel.Verbose: OnVerbose?.Invoke(message); break;
                case LogLevel.Info: OnInfo?.Invoke(message); break;
                case LogLevel.Warning: OnWarning?.Invoke(message); break;
                case LogLevel.Error: OnError?.Invoke(message); break;
                case LogLevel.Success: OnSuccess?.Invoke(message); break;
                case LogLevel.Important: OnImportant?.Invoke(message); break;
                default: break;
            }

            // Write to disk if file is free.
            if (!IsFileLocked(new FileInfo(LogPath)))
            {
                try
                {
                    using (var fileStream = new StreamWriter(File.OpenWrite(LogPath), System.Text.Encoding.UTF8))
                    {
                        // Jump to the end of the file before writing (same as append).
                        fileStream.BaseStream.Seek(0, SeekOrigin.End);
                        // Write the text to the file (adds CRLF automatically).
                        fileStream.WriteLine(message);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Logger] {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Logging test method extension.
        /// </summary>
        /// <remarks>asynchronous</remarks>
        public async Task<bool> LogLocalAsync(string message, string logName = null)
        {
            try
            {
                string name = logName ?? $"{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                //string path1 = System.IO.Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.ToString() ?? Environment.CurrentDirectory;
                string path2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{name}");
                await Task.Run(() => { File.AppendAllText(path2, $"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] {message}{Environment.NewLine}", System.Text.Encoding.UTF8); });
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"> LogLocalAsync: {ex.Message}");
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// Uses the <see cref="FileStream"/> to determine if a file is currently in use.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/></param>
        /// <returns>true if locked, false otherwise</returns>
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine if a file/folder path exceed the maximum allowed.
        /// </summary>
        /// <param name="path">full path</param>
        /// <returns>true if too long, false otherwise</returns>
        public static bool IsPathTooLong(string path)
        {
            try
            {
                var tmp = Path.GetFullPath(path);
                return false;
            }
            catch (UnauthorizedAccessException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
            catch (PathTooLongException) { return true; }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Determines if a path is not a <see cref="System.IO.FileAttributes.ReparsePoint"/>
        /// and <see cref="IsReadable(string)"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if valid, false otherwise</returns>
        public static bool IsValidPath(string path)
        {
            if ((File.GetAttributes(path) & System.IO.FileAttributes.ReparsePoint) == System.IO.FileAttributes.ReparsePoint)
            {
                System.Diagnostics.Debug.WriteLine("'" + path + "' is a reparse point (skipped)");
                return false;
            }
            if (!IsReadable(path))
            {
                System.Diagnostics.Debug.WriteLine("'" + path + "' *ACCESS DENIED* (skipped)");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to get the directory name and the directories inside the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if readable, false otherwise</returns>
        public static bool IsReadable(string path)
        {
            try
            {
                var dn = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(dn)) { return false; }
                string[] test = Directory.GetDirectories(dn, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch (UnauthorizedAccessException) { return false; }
            catch (PathTooLongException) { return false; }
            catch (IOException) { return false; }
            return true;
        }

        public static bool FilePathHasInvalidChars(string path) => (!string.IsNullOrEmpty(path) && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0);
        public static bool IsInvalidFileNameChar(Char c) => c < 64U ? (1UL << c & 0xD4008404FFFFFFFFUL) != 0 : c == '\\' || c == '|';
        public static string RemoveInvalidCharacters(string path) => Path.GetInvalidFileNameChars().Aggregate(path, (current, c) => current.Replace(c.ToString(), string.Empty));
        public static string SanitizeFilePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                while (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                {
                    int idx = path.IndexOfAny(System.IO.Path.GetInvalidPathChars());
                    path = path.Remove(idx, 1).Insert(idx, string.Empty);
                }
                return path;
            }
            else
                return path;
        }


        /// <summary>
        /// Debug method (can be removed)
        /// </summary>
        public void TestDateChanged() => _date = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)); // Set to yesterday.
    }
}
