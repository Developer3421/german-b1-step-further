using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using German_B1._Step_Further.Models;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Сервіс для роботи з базами даних сесій LiteDB
    /// </summary>
    public static class SessionDatabaseService
    {
        private static readonly string AppDataFolder;
        private static readonly string ActiveSessionsDbPath;
        private static readonly string ClosedSessionsDbPath;

        static SessionDatabaseService()
        {
            // Отримуємо шлях до AppData\Local (для Microsoft Store)
            AppDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "German B1 Step Further"
            );

            EnsureAppDataFolder();

            ActiveSessionsDbPath = Path.Combine(AppDataFolder, "ActiveSessions.db");
            ClosedSessionsDbPath = Path.Combine(AppDataFolder, "ClosedSessions.db");
        }

        /// <summary>
        /// Отримує шлях до папки AppData додатку
        /// </summary>
        public static string GetAppDataPath() => AppDataFolder;

        private static void EnsureAppDataFolder()
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        private static void HandleCorruptedDatabase(string dbPath, Exception ex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Database error for '{dbPath}': {ex.Message}");

                EnsureAppDataFolder();

                // If the file exists, keep a backup for diagnostics.
                if (File.Exists(dbPath))
                {
                    var backup = dbPath + ".corrupted." + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".bak";
                    File.Move(dbPath, backup);
                }
            }
            catch (Exception inner)
            {
                // In the worst case, just log and continue.
                System.Diagnostics.Debug.WriteLine($"Failed to backup corrupted db '{dbPath}': {inner.Message}");
            }
        }

        #region Active Sessions (для відкритих вікон)

        /// <summary>
        /// Зберігає сесію активного вікна
        /// NOTE: The app is single-window on restore. We keep only one active window session.
        /// </summary>
        public static void SaveActiveWindowSession(string windowId, List<int> tabPages, int activeTabIndex,
            double posX, double posY, double width, double height, bool isMaximized)
        {
            try
            {
                EnsureAppDataFolder();
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
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ActiveSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Видаляє сесію активного вікна (коли воно закривається)
        /// </summary>
        public static void RemoveActiveWindowSession(string windowId)
        {
            try
            {
                EnsureAppDataFolder();
                using var db = new LiteDatabase(ActiveSessionsDbPath);
                var collection = db.GetCollection<WindowSession>("windows");
                collection.DeleteAll();
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ActiveSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Отримує всі активні сесії вікон
        /// NOTE: should return 0..1 items.
        /// </summary>
        public static List<WindowSession> GetAllActiveWindowSessions()
        {
            try
            {
                EnsureAppDataFolder();
                using var db = new LiteDatabase(ActiveSessionsDbPath);
                var collection = db.GetCollection<WindowSession>("windows");
                return collection.FindAll().Take(1).ToList();
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ActiveSessionsDbPath, ex);
                return new List<WindowSession>();
            }
        }

        #endregion

        #region Closed Sessions (для відновлення після перезапуску)

        /// <summary>
        /// Зберігає сесію при закритті програми для відновлення
        /// Single-window: stores only the first session.
        /// </summary>
        public static void SaveSessionForRestore(List<WindowSession> sessions)
        {
            try
            {
                EnsureAppDataFolder();
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
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Зберігає одну сесію вікна при закритті для відновлення
        /// Single-window: overwrites the stored session.
        /// </summary>
        public static void SaveWindowSessionForRestore(WindowSession session)
        {
            try
            {
                EnsureAppDataFolder();
                using var db = new LiteDatabase(ClosedSessionsDbPath);
                var collection = db.GetCollection<WindowSession>("sessions");

                collection.DeleteAll();

                session.LastSaved = DateTime.Now;
                collection.Insert(session);
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Завантажує сесії для відновлення при запуску
        /// NOTE: returns 0..1 items.
        /// </summary>
        public static List<WindowSession> LoadSessionsForRestore()
        {
            try
            {
                EnsureAppDataFolder();
                using var db = new LiteDatabase(ClosedSessionsDbPath);
                var collection = db.GetCollection<WindowSession>("sessions");
                return collection.FindAll().Take(1).ToList();
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
                return new List<WindowSession>();
            }
        }

        /// <summary>
        /// Очищає збережені сесії після відновлення
        /// </summary>
        public static void ClearRestoredSessions()
        {
            try
            {
                EnsureAppDataFolder();
                using var db = new LiteDatabase(ClosedSessionsDbPath);
                var collection = db.GetCollection<WindowSession>("sessions");
                collection.DeleteAll();
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        /// <summary>
        /// Видаляє сесії старші за вказану кількість днів
        /// </summary>
        public static void CleanOldSessions(int daysOld = 30)
        {
            try
            {
                EnsureAppDataFolder();
                var cutoffDate = DateTime.Now.AddDays(-daysOld);

                using var db = new LiteDatabase(ClosedSessionsDbPath);
                var collection = db.GetCollection<WindowSession>("sessions");
                collection.DeleteMany(s => s.LastSaved < cutoffDate);
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ClosedSessionsDbPath, ex);
            }
        }

        #endregion
    }
}
