using FluentCanvas.Services;
using System.Windows;
using System.Windows.Controls;

namespace FluentCanvas.Views
{
    /// <summary>
    /// Base class for shell parts. It registers the view with
    /// <see cref="IAppViewService"/> when loaded and removes it when unloaded,
    /// and provides default show/hide behavior that derived views can override
    /// (for example to run animations).
    /// </summary>
    public abstract class FCViewBase : UserControl, IFCView
    {
        protected FCViewBase()
        {
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        public abstract string ViewName { get; }

        public bool IsViewVisible => Visibility == Visibility.Visible;

        public virtual void ShowView() => Visibility = Visibility.Visible;

        public virtual void HideView() => Visibility = Visibility.Collapsed;

        public virtual void ToggleView()
        {
            if (IsViewVisible) HideView();
            else ShowView();
        }

        protected virtual void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            Locator.Get<IAppViewService>().Register(this);
        }

        protected virtual void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            Locator.Get<IAppViewService>().Unregister(this);
        }
    }
}
