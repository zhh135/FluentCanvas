using Microsoft.Extensions.DependencyInjection;
using System;

namespace FluentCanvas.Services
{
    /// <summary>
    /// Static wrapper around the Microsoft dependency injection container.
    /// Services are registered through <see cref="Services"/> and resolved
    /// through <see cref="Get{T}"/> after <see cref="Initialize"/> is called.
    /// </summary>
    public static class Locator
    {
        public static IServiceCollection Services { get; } = new ServiceCollection();

        private static IServiceProvider provider;

        public static bool IsInitialized => provider != null;

        public static void Initialize()
        {
            provider ??= Services.BuildServiceProvider();
        }

        public static T Get<T>() where T : notnull
        {
            if (provider == null)
            {
                throw new InvalidOperationException("Locator has not been initialized.");
            }

            return provider.GetRequiredService<T>();
        }

        public static T GetOrDefault<T>()
        {
            return provider == null ? default : provider.GetService<T>();
        }
    }
}
