using FluentCanvas.Helpers;
using FluentCanvas.Services;
using FluentCanvas.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FluentCanvas.Views
{
    public partial class PptNavigationView : FCViewBase, IPptNavigationView
    {
        private readonly PptNavigationViewModel viewModel;
        private object lastBorderMouseDownObject;

        public PptNavigationView()
        {
            InitializeComponent();

            viewModel = Locator.Get<PptNavigationViewModel>();
            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        public override string ViewName => nameof(PptNavigationView);

        public void SetActive(bool active) => viewModel.IsActive = active;

        public void ShowPanels(bool bottom, bool side) => viewModel.ShowPanels(bottom, side);

        public void HidePanels() => viewModel.HidePanels();

        public void SetPosition(int current, int total) => viewModel.SetPosition(current, total);

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PptNavigationViewModel.IsBottomPanelVisible))
            {
                if (viewModel.IsBottomPanelVisible)
                {
                    AnimationsHelper.ShowWithScaleFromBottom(PPTNavigationBottomLeft);
                    AnimationsHelper.ShowWithScaleFromBottom(PPTNavigationBottomRight);
                }
                else
                {
                    PPTNavigationBottomLeft.Visibility = Visibility.Collapsed;
                    PPTNavigationBottomRight.Visibility = Visibility.Collapsed;
                }
            }
            else if (e.PropertyName == nameof(PptNavigationViewModel.IsSidePanelVisible))
            {
                if (viewModel.IsSidePanelVisible)
                {
                    AnimationsHelper.ShowWithScaleFromLeft(PPTNavigationSidesLeft);
                    AnimationsHelper.ShowWithScaleFromRight(PPTNavigationSidesRight);
                }
                else
                {
                    PPTNavigationSidesLeft.Visibility = Visibility.Collapsed;
                    PPTNavigationSidesRight.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastBorderMouseDownObject = sender;
        }

        private void GridPPTControlPrevious_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;
            viewModel.PreviousCommand.Execute(null);
        }

        private void GridPPTControlNext_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;
            viewModel.NextCommand.Execute(null);
        }

        private void PPTNavigationBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;
            viewModel.ToggleNavigationCommand.Execute(null);
        }
    }
}
