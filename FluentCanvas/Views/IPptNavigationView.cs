namespace FluentCanvas.Views
{
    /// <summary>Contract for the PowerPoint slide navigation panels.</summary>
    public interface IPptNavigationView : IFCView
    {
        void SetActive(bool active);

        void ShowPanels(bool bottom, bool side);

        void HidePanels();

        void SetPosition(int current, int total);
    }
}
