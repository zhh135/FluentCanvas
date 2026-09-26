namespace FluentCanvas.Views
{
    /// <summary>Contract for the detailed settings panel.</summary>
    public interface ISettingsPanelView : IFCView
    {
        bool IsPanelVisible { get; }

        void LoadFromSettings();

        void ShowPanel();

        void HidePanel();

        void TogglePanel();
    }
}
