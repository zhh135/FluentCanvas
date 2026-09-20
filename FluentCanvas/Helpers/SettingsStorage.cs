using System;
using System.IO;
using Newtonsoft.Json;

namespace FluentCanvas.Helpers
{
    /// <summary>
    /// Persists the settings payload.
    /// Packaged builds store the file inside the WinRT <c>ApplicationData.LocalFolder</c>;
    /// unpackaged builds store it in the local per-user folder.
    /// </summary>
    internal static class SettingsStorage
    {
        private const string SettingsFileName = "Settings.json";

        public static string FilePath => Path.Combine(StorageHelper.UserDataFolder, SettingsFileName);

        public static string Read()
        {
            try
            {
                var path = FilePath;
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"SettingsStorage | Read failed: {ex.Message}", LogHelper.LogType.Error);
                return null;
            }
        }

        public static void Write(string json)
        {
            try
            {
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"SettingsStorage | Write failed: {ex.Message}", LogHelper.LogType.Error);
            }
        }

        public static Settings Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"SettingsStorage | Deserialize failed: {ex.Message}", LogHelper.LogType.Error);
                return null;
            }
        }

        public static string Serialize(Settings settings)
        {
            return JsonConvert.SerializeObject(settings, Formatting.Indented);
        }
    }
}
