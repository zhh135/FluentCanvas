namespace FluentCanvas.Services
{
    /// <summary>
    /// Owns the application settings model and its persistence.
    /// Replaces the former static <c>MainWindow.Settings</c> field.
    /// </summary>
    public interface ISettingsService
    {
        Settings Model { get; set; }

        /// <summary>Loads persisted settings. Returns false when no settings file exists.</summary>
        bool Load();

        void Save();

        string NormalizeStorageLocation(string location);
    }
}
