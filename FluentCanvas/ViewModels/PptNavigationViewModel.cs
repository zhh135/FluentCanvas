using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace FluentCanvas.ViewModels
{
    /// <summary>
    /// State for the PowerPoint navigation panels. The shell subscribes to the
    /// request events so it can run its own orchestration (screenshots, board
    /// mode switching) while the view stays free of shell references.
    /// </summary>
    public sealed partial class PptNavigationViewModel : ObservableObject
    {
        public PptNavigationViewModel()
        {
            PositionText = "0/0";
            IsBottomButtonVisible = true;
            IsSideButtonVisible = true;
        }

        [ObservableProperty]
        public partial int CurrentPosition { get; set; }

        [ObservableProperty]
        public partial int TotalSlides { get; set; }

        [ObservableProperty]
        public partial bool IsActive { get; set; }

        [ObservableProperty]
        public partial bool IsBottomPanelVisible { get; set; }

        [ObservableProperty]
        public partial bool IsSidePanelVisible { get; set; }

        [ObservableProperty]
        public partial bool IsBottomButtonVisible { get; set; }

        [ObservableProperty]
        public partial bool IsSideButtonVisible { get; set; }

        public string PositionText { get; private set; }

        public event EventHandler NextRequested;

        public event EventHandler PreviousRequested;

        public event EventHandler NavigationToggleRequested;

        public void SetPosition(int current, int total)
        {
            CurrentPosition = current;
            TotalSlides = total;
            PositionText = total > 0 ? $"{current}/{total}" : "0/0";
            OnPropertyChanged(nameof(PositionText));
        }

        public void ShowPanels(bool bottom, bool side)
        {
            IsBottomPanelVisible = bottom;
            IsSidePanelVisible = side;
        }

        public void HidePanels()
        {
            IsBottomPanelVisible = false;
            IsSidePanelVisible = false;
        }

        [RelayCommand]
        private void Next() => NextRequested?.Invoke(this, EventArgs.Empty);

        [RelayCommand]
        private void Previous() => PreviousRequested?.Invoke(this, EventArgs.Empty);

        [RelayCommand]
        private void ToggleNavigation() => NavigationToggleRequested?.Invoke(this, EventArgs.Empty);
    }
}
