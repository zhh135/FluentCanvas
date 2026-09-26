namespace FluentCanvas.Views
{
    /// <summary>
    /// Base contract for every independently loaded view in the shell.
    /// Each concrete view exposes a matching <c>I&lt;Name&gt;View</c> interface
    /// derived from this type, which is what the rest of the app resolves
    /// through <see cref="Services.IAppViewService"/>.
    /// </summary>
    public interface IFCView
    {
        /// <summary>Stable identifier used for diagnostics and logging.</summary>
        string ViewName { get; }

        bool IsViewVisible { get; }

        void ShowView();

        void HideView();

        void ToggleView();
    }
}
