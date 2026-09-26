using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentCanvas.Services;

namespace FluentCanvas.ViewModels
{
    /// <summary>
    /// Single entry point for settings persistence. The model itself is owned
    /// by <see cref="ISettingsService"/> so the rest of the app shares one instance.
    /// </summary>
    public sealed partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService settingsService;

        public SettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public Settings Model
        {
            get => settingsService.Model;
            private set
            {
                settingsService.Model = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Loads persisted settings. Returns false when no settings file exists.</summary>
        public bool Load() => settingsService.Load();

        [RelayCommand]
        public void Save()
        {
            settingsService.Save();
        }
    }
}
