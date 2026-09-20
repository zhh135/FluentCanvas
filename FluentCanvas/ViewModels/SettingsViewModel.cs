using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentCanvas.Helpers;

namespace FluentCanvas.ViewModels
{
    /// <summary>
    /// Single entry point for settings persistence.
    /// The model instance stays shared with the rest of the application
    /// (<see cref="MainWindow.Settings"/>) so migration can proceed incrementally.
    /// </summary>
    public sealed partial class SettingsViewModel : ObservableObject
    {
        public Settings Model
        {
            get => MainWindow.Settings;
            private set
            {
                MainWindow.Settings = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Loads persisted settings. Returns false when no settings file exists.</summary>
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

        [RelayCommand]
        public void Save()
        {
            SettingsStorage.Write(SettingsStorage.Serialize(Model));
        }
    }
}
