using System;
using System.IO;
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

        private static void HandleCorruptedDatabase(Exception ex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UserSettings db error: {ex.Message}");

                // Keep a backup of the broken file, then recreate.
                if (File.Exists(DbPath))
                {
                    var backup = DbPath + ".corrupted." + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".bak";
                    File.Move(DbPath, backup);
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
                using var db = new LiteDatabase(DbPath);
                var col = db.GetCollection<UserSettings>("settings");
                var settings = col.FindById(SettingsId);
                return settings ?? new UserSettings();
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ex);

                // Try again with a fresh DB.
                try
                {
                    using var db = new LiteDatabase(DbPath);
                    var col = db.GetCollection<UserSettings>("settings");
                    var settings = col.FindById(SettingsId);
                    return settings ?? new UserSettings();
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
                using var db = new LiteDatabase(DbPath);
                var col = db.GetCollection<UserSettings>("settings");

                var settings = col.FindById(SettingsId) ?? new UserSettings();
                settings.UserAgreementAccepted = true;
                settings.UserAgreementAcceptedAtUtc = DateTime.UtcNow;
                settings.UserAgreementVersion = version;

                // Upsert with a stable id.
                col.Upsert(SettingsId, settings);
            }
            catch (Exception ex)
            {
                HandleCorruptedDatabase(ex);

                // Best-effort retry.
                try
                {
                    using var db = new LiteDatabase(DbPath);
                    var col = db.GetCollection<UserSettings>("settings");

                    var settings = col.FindById(SettingsId) ?? new UserSettings();
                    settings.UserAgreementAccepted = true;
                    settings.UserAgreementAcceptedAtUtc = DateTime.UtcNow;
                    settings.UserAgreementVersion = version;
                    col.Upsert(SettingsId, settings);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
