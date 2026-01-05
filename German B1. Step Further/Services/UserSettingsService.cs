using System;
using System.IO;
using System.Linq;
using LiteDB;
using German_B1._Step_Further.Models;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Stores minimal user settings locally.
    /// Contract:
    /// - Local only: stores whether User Agreement was accepted.
    /// - Uses the same AppData folder as session storage.
    /// </summary>
    public static class UserSettingsService
    {
        private static readonly string DbPath = Path.Combine(SessionDatabaseService.GetAppDataPath(), "UserSettings.db");

        // Single logical record; we keep a stable id.
        private const int SettingsId = 1;

        private static readonly object SettingsDbLock = new();
        private const int MaxCorruptedBackups = 5;

        private static void CleanupOldCorruptedBackups()
        {
            try
            {
                var dir = Path.GetDirectoryName(DbPath);
                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                    return;

                var fileName = Path.GetFileName(DbPath);
                var backups = Directory.GetFiles(dir, fileName + ".corrupted.*.bak")
                    .Select(p => new FileInfo(p))
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .ToList();

                foreach (var extra in backups.Skip(MaxCorruptedBackups))
                {
                    try { extra.Delete(); } catch { /* ignore */ }
                }
            }
            catch
            {
                // ignore
            }
        }

        private static void HandleCorruptedDatabase(Exception ex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UserSettings db error: {ex.Message}");

                if (File.Exists(DbPath))
                {
                    var backup = DbPath + ".corrupted." + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".bak";

                    try
                    {
                        File.Move(DbPath, backup);
                    }
                    catch
                    {
                        try
                        {
                            File.Copy(DbPath, backup, overwrite: true);
                            try { File.Delete(DbPath); } catch { /* ignore */ }
                        }
                        catch
                        {
                            try { File.Delete(DbPath); } catch { /* ignore */ }
                        }
                    }

                    CleanupOldCorruptedBackups();
                }
            }
            catch (Exception inner)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to backup corrupted UserSettings db: {inner.Message}");
            }
        }

        public static UserSettings Get()
        {
            try
            {
                lock (SettingsDbLock)
                {
                    using var db = new LiteDatabase(DbPath);
                    var col = db.GetCollection<UserSettings>("settings");
                    var settings = col.FindById(SettingsId);
                    return settings ?? new UserSettings();
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ex);

                try
                {
                    lock (SettingsDbLock)
                    {
                        using var db = new LiteDatabase(DbPath);
                        var col = db.GetCollection<UserSettings>("settings");
                        var settings = col.FindById(SettingsId);
                        return settings ?? new UserSettings();
                    }
                }
                catch
                {
                    return new UserSettings();
                }
            }
        }

        public static bool IsUserAgreementAccepted()
        {
            var s = Get();
            return s.UserAgreementAccepted;
        }

        public static void SetUserAgreementAccepted(string version = "1.0")
        {
            try
            {
                lock (SettingsDbLock)
                {
                    using var db = new LiteDatabase(DbPath);
                    var col = db.GetCollection<UserSettings>("settings");

                    var settings = col.FindById(SettingsId) ?? new UserSettings();
                    settings.UserAgreementAccepted = true;
                    settings.UserAgreementAcceptedAtUtc = DateTime.UtcNow;
                    settings.UserAgreementVersion = version;

                    col.Upsert(SettingsId, settings);
                }
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ex);

                try
                {
                    lock (SettingsDbLock)
                    {
                        using var db = new LiteDatabase(DbPath);
                        var col = db.GetCollection<UserSettings>("settings");

                        var settings = col.FindById(SettingsId) ?? new UserSettings();
                        settings.UserAgreementAccepted = true;
                        settings.UserAgreementAcceptedAtUtc = DateTime.UtcNow;
                        settings.UserAgreementVersion = version;
                        col.Upsert(SettingsId, settings);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
