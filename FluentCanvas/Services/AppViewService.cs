using FluentCanvas.Views;
using System;
using System.Collections.Generic;

namespace FluentCanvas.Services
{
    public sealed class AppViewService : IAppViewService
    {
        private readonly Dictionary<Type, object> views = new Dictionary<Type, object>();
        private readonly Dictionary<(Type, string), object> keyedViews = new Dictionary<(Type, string), object>();
        private readonly object gate = new object();

        public void Register(IFCView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            lock (gate)
            {
                var viewType = view.GetType();
                views[viewType] = view;

                foreach (var contract in viewType.GetInterfaces())
                {
                    if (contract == typeof(IFCView))
                    {
                        continue;
                    }

                    if (typeof(IFCView).IsAssignableFrom(contract))
                    {
                        views[contract] = view;
                    }
                }
            }
        }

        public void Register(string key, IFCView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("A key is required.", nameof(key));

            lock (gate)
            {
                keyedViews[(view.GetType(), key)] = view;
            }
        }

        public TView Get<TView>() where TView : class
        {
            lock (gate)
            {
                if (views.TryGetValue(typeof(TView), out var view))
                {
                    return (TView)view;
                }
            }

            throw new KeyNotFoundException($"View '{typeof(TView).FullName}' is not registered.");
        }

        public TView Get<TView>(string key) where TView : class
        {
            lock (gate)
            {
                if (keyedViews.TryGetValue((typeof(TView), key), out var view))
                {
                    return (TView)view;
                }
            }

            throw new KeyNotFoundException($"View '{typeof(TView).FullName}' with key '{key}' is not registered.");
        }

        public bool TryGet<TView>(out TView view) where TView : class
        {
            lock (gate)
            {
                if (views.TryGetValue(typeof(TView), out var value))
                {
                    view = (TView)value;
                    return true;
                }
            }

            view = null;
            return false;
        }

        public void Unregister(IFCView view)
        {
            if (view == null) return;

            lock (gate)
            {
                var staleKeys = new List<Type>();
                foreach (var pair in views)
                {
                    if (ReferenceEquals(pair.Value, view))
                    {
                        staleKeys.Add(pair.Key);
                    }
                }

                foreach (var key in staleKeys)
                {
                    views.Remove(key);
                }

                var staleStringKeys = new List<(Type, string)>();
                foreach (var pair in keyedViews)
                {
                    if (ReferenceEquals(pair.Value, view))
                    {
                        staleStringKeys.Add(pair.Key);
                    }
                }

                foreach (var key in staleStringKeys)
                {
                    keyedViews.Remove(key);
                }
            }
        }

        public void Show<TView>() where TView : class => SetVisible<TView>(true);

        public void Hide<TView>() where TView : class => SetVisible<TView>(false);

        public void Toggle<TView>() where TView : class => Get<TView>().AsView().ToggleView();

        public void SetVisible<TView>(bool visible) where TView : class
        {
            var view = Get<TView>().AsView();
            if (visible) view.ShowView();
            else view.HideView();
        }
    }

    internal static class AppViewExtensions
    {
        public static IFCView AsView(this object view)
        {
            if (view is IFCView fcView)
            {
                return fcView;
            }

            throw new InvalidOperationException($"'{view?.GetType().FullName}' is not an {nameof(IFCView)}.");
        }
    }
}
