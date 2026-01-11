using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using German_B1._Step_Further.Models;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Service for working with LiteDB session databases
    /// </summary>
    public static class SessionDatabaseService
    {
        private static readonly string AppDataFolder;
        private static readonly string ActiveSessionsDbPath;
        private static readonly string ClosedSessionsDbPath;

        static SessionDatabaseService()
        {
            // Get path to AppData\Local (for Microsoft Store)
            AppDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "German B1 Step Further"
            );

            EnsureAppDataFolder();

            ActiveSessionsDbPath = Path.Combine(AppDataFolder, "ActiveSessions.db");
            ClosedSessionsDbPath = Path.Combine(AppDataFolder, "ClosedSessions.db");
        }

        /// <summary>
        /// Gets the application AppData folder path
        /// </summary>
        public static string GetAppDataPath() => AppDataFolder;

        private static void EnsureAppDataFolder()
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        // Prevent concurrent open/write to the same LiteDB file from different UI events/threads.
        private static readonly object ActiveDbLock = new();
        private static readonly object ClosedDbLock = new();

        // Keep backups small and bounded.
        private const int MaxCorruptedBackupsPerDb = 5;

        private static void HandleCorruptedDatabase(string dbPath, Exception ex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Database error for '{dbPath}': {ex.Message}");

                EnsureAppDataFolder();

                if (File.Exists(dbPath))
                {
                    var backup = dbPath + ".corrupted." + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".bak";

                    // Move can fail if the file is locked; fall back to copy+delete, or delete as last resort.
                    try
                    {
                        File.Move(dbPath, backup);
                    }
                    catch
                    {
                        try
                        {
                            File.Copy(dbPath, backup, overwrite: true);
                            try { File.Delete(dbPath); } catch { /* ignore */ }
                        }
                        catch
                        {
                            // As a last resort, try to delete the broken db to allow a clean recreate.
                            try { File.Delete(dbPath); } catch { /* ignore */ }
                        }
                    }

                    CleanupOldCorruptedBackups(dbPath);
                }
            }
            catch (Exception inner)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to backup corrupted db '{dbPath}': {inner.Message}");
            }
        }

        private static void CleanupOldCorruptedBackups(string dbPath)
        {
            try
            {
                var dir = Path.GetDirectoryName(dbPath);
                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                    return;

                var fileName = Path.GetFileName(dbPath);
                var backups = Directory.GetFiles(dir, fileName + ".corrupted.*.bak")
                    .Select(p => new FileInfo(p))
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .ToList();

                foreach (var extra in backups.Skip(MaxCorruptedBackupsPerDb))
                {
                    try { extra.Delete(); } catch { /* ignore */ }
                }
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        #region Active Sessions (for open windows)

        /// <summary>
        /// Saves active window session
        /// NOTE: The app is single-window on restore. We keep only one active window session.
        /// </summary>
        public static void SaveActiveWindowSession(string windowId, List<int> tabPages, int activeTabIndex,
            double posX, double posY, double width, double height, bool isMaximized)
        {
            try
            {
                EnsureAppDataFolder();
                lock (ActiveDbLock)
                {
                    using var db = new LiteDatabase(ActiveSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("windows");

                    // Single-window: clear any previously saved active windows
                    collection.DeleteAll();

                    var session = new WindowSession
                    {
                        WindowId = windowId,
                        TabPages = new List<int>(tabPages),
                        ActiveTabIndex = activeTabIndex,
                        PositionX = posX,
                        PositionY = posY,
                        Width = width,
                        Height = height,
                        IsMaximized = isMaximized,
                        LastSaved = DateTime.Now
                    };

                    collection.Insert(session);
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ActiveSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Removes active window session (when it closes)
        /// </summary>
        public static void RemoveActiveWindowSession(string windowId)
        {
            try
            {
                EnsureAppDataFolder();
                lock (ActiveDbLock)
                {
                    using var db = new LiteDatabase(ActiveSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("windows");
                    collection.DeleteAll();
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ActiveSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Gets all active window sessions
        /// NOTE: should return 0..1 items.
        /// </summary>
        public static List<WindowSession> GetAllActiveWindowSessions()
        {
            try
            {
                EnsureAppDataFolder();
                lock (ActiveDbLock)
                {
                    using var db = new LiteDatabase(ActiveSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("windows");
                    return collection.FindAll().Take(1).ToList();
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ActiveSessionsDbPath, ex);
                return new List<WindowSession>();
            }
        }

        #endregion

        #region Closed Sessions (for restore after restart)

        /// <summary>
        /// Saves session on program close for restore
        /// Single-window: stores only the first session.
        /// </summary>
        public static void SaveSessionForRestore(List<WindowSession> sessions)
        {
            try
            {
                EnsureAppDataFolder();
                lock (ClosedDbLock)
                {
                    using var db = new LiteDatabase(ClosedSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("sessions");

                    collection.DeleteAll();

                    var first = sessions.FirstOrDefault();
                    if (first != null)
                    {
                        first.LastSaved = DateTime.Now;
                        collection.Insert(first);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Saves one window session on close for restore
        /// Single-window: overwrites the stored session.
        /// </summary>
        public static void SaveWindowSessionForRestore(WindowSession session)
        {
            try
            {
                EnsureAppDataFolder();
                lock (ClosedDbLock)
                {
                    using var db = new LiteDatabase(ClosedSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("sessions");

                    collection.DeleteAll();

                    session.LastSaved = DateTime.Now;
                    collection.Insert(session);
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Loads sessions for restore on startup
        /// NOTE: returns 0..1 items.
        /// </summary>
        public static List<WindowSession> LoadSessionsForRestore()
        {
            try
            {
                EnsureAppDataFolder();
                lock (ClosedDbLock)
                {
                    using var db = new LiteDatabase(ClosedSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("sessions");
                    return collection.FindAll().Take(1).ToList();
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
                return new List<WindowSession>();
            }
        }

        /// <summary>
        /// Clears saved sessions after restore
        /// </summary>
        public static void ClearRestoredSessions()
        {
            try
            {
                EnsureAppDataFolder();
                lock (ClosedDbLock)
                {
                    using var db = new LiteDatabase(ClosedSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("sessions");
                    collection.DeleteAll();
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Deletes sessions older than specified number of days
        /// </summary>
        public static void CleanOldSessions(int daysOld = 30)
        {
            try
            {
                EnsureAppDataFolder();
                var cutoffDate = DateTime.Now.AddDays(-daysOld);

                lock (ClosedDbLock)
                {
                    using var db = new LiteDatabase(ClosedSessionsDbPath);
                    var collection = db.GetCollection<WindowSession>("sessions");
                    collection.DeleteMany(s => s.LastSaved < cutoffDate);
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        #endregion
    }
}
