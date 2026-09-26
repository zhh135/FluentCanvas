using FluentCanvas.Helpers;

namespace FluentCanvas.Services
{
    /// <summary>
    /// File-based settings storage used by the unpackaged build.
    /// The file lives under the local per-user folder.
    /// </summary>
    public sealed class LocalSettingsService : ISettingsService
    {
        public Settings Model { get; set; } = new Settings();

        public bool Load()
        {
            var loaded = SettingsStorage.Deserialize(SettingsStorage.Read());
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
            SettingsStorage.Write(SettingsStorage.Serialize(Model));
        }

        public string NormalizeStorageLocation(string location)
        {
            return StorageHelper.NormalizeStorageLocation(location);
        }
    }
}
