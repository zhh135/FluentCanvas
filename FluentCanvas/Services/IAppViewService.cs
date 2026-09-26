using FluentCanvas.Views;

namespace FluentCanvas.Services
{
    /// <summary>
    /// Registry for the independently loaded parts of the shell.
    /// A view registers itself when loaded (through <see cref="FCViewBase"/>)
    /// and can then be resolved or shown/hidden by other parts through its
    /// <c>I&lt;Name&gt;View</c> interface, without holding direct references.
    /// </summary>
    public interface IAppViewService
    {
        /// <summary>
        /// Registers a view under its concrete type and under every
        /// <see cref="IFCView"/>-derived interface it implements.
        /// </summary>
        void Register(IFCView view);

        void Register(string key, IFCView view);

        TView Get<TView>() where TView : class;

        TView Get<TView>(string key) where TView : class;

        bool TryGet<TView>(out TView view) where TView : class;

        void Unregister(IFCView view);

        void Show<TView>() where TView : class;

        void Hide<TView>() where TView : class;

        void Toggle<TView>() where TView : class;

        void SetVisible<TView>(bool visible) where TView : class;
    }
}
