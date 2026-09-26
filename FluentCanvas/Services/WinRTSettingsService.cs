using FluentCanvas.Helpers;
using System;
using System.IO;

namespace FluentCanvas.Services
{
    /// <summary>
    /// WinRT settings storage used by the packaged (MSIX) build.
    /// The payload is stored in <c>ApplicationData.LocalSettings</c> and falls
    /// back to a file in <c>ApplicationData.LocalFolder</c> when it is too large.
    /// </summary>
    public sealed class WinRTSettingsService : ISettingsService
    {
        private const string LocalSettingsKey = "SettingsJson";
        private const string FileName = "Settings.json";

        public Settings Model { get; set; } = new Settings();

        public bool Load()
        {
            var loaded = SettingsStorage.Deserialize(ReadJson());
            if (loaded == null)
            {
                Model = new Settings();
                return false;
            }

            Model = loaded;
            Model.Automation.AutoSavedStrokesLocation =
                StorageHelper.NormalizeStorageLocation(Model.Automation.AutoSavedStrokesLocation);
            return true;
        }

        public void Save()
        {
            WriteJson(SettingsStorage.Serialize(Model));
        }

        public string NormalizeStorageLocation(string location)
        {
            return StorageHelper.NormalizeStorageLocation(location);
        }

        private static string ReadJson()
        {
            try
            {
                var values = global::Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                if (values.TryGetValue(LocalSettingsKey, out var value) &&
                    value is string json &&
                    !string.IsNullOrEmpty(json))
                {
                    return json;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"WinRTSettingsService | LocalSettings read failed: {ex.Message}", LogHelper.LogType.Error);
            }

            try
            {
                var path = Path.Combine(global::Windows.Storage.ApplicationData.Current.LocalFolder.Path, FileName);
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"WinRTSettingsService | LocalFolder read failed: {ex.Message}", LogHelper.LogType.Error);
                return null;
            }
        }

        private static void WriteJson(string json)
        {
            try
            {
                global::Windows.Storage.ApplicationData.Current.LocalSettings.Values[LocalSettingsKey] = json;
                return;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"WinRTSettingsService | LocalSettings write failed: {ex.Message}", LogHelper.LogType.Error);
            }

            try
            {
                var path = Path.Combine(global::Windows.Storage.ApplicationData.Current.LocalFolder.Path, FileName);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"WinRTSettingsService | LocalFolder write failed: {ex.Message}", LogHelper.LogType.Error);
            }
        }
    }
}
