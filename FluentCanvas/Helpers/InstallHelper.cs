using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace FluentCanvas.Helpers
{
    /// <summary>
    /// Handles first-run migration and registration.
    /// The packaged (MSIX) build relies on the package manifest, while the
    /// unpackaged build self-registers its file associations on first launch.
    /// </summary>
    internal static class InstallHelper
    {
        private const string ApplicationName = "FluentCanvas";
        private const string StartupValueName = "FluentCanvas";
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ClassesKeyPath = @"Software\Classes";
        private const string RegisteredApplicationsKeyPath = @"Software\RegisteredApplications";
        private const string CapabilitiesKeyPath = @"Software\FluentCanvas\Capabilities";
        private const string AppPathsKeyPath = @"Software\Microsoft\Windows\CurrentVersion\App Paths\FluentCanvas.exe";

        private static readonly string[] LegacyStartupValueNames =
        {
            "InkCanvas",
            "Ink Canvas Annotation",
            "Ink Canvas Artistry",
            "FluentCanvas Annotation"
        };

        private static readonly string[] LegacyShortcutNames =
        {
            "InkCanvas.lnk",
            "Ink Canvas Annotation.lnk",
            "Ink Canvas Artistry.lnk"
        };

        private static readonly string[] LegacyProgIds =
        {
            "InkCanvasArtistryFile.icart",
            "InkCanvasArtistryFile.icstk"
        };

        private static readonly string[] MigratableFiles =
        {
            "Settings.json",
            "Names.txt",
            "Replace.txt"
        };

        private static readonly (string Extension, string ProgId, string Description)[] FileAssociations =
        {
            (".icart", "FluentCanvas.icart", "FluentCanvas Annotation File"),
            (".icstk", "FluentCanvas.icstk", "FluentCanvas Stroke File")
        };

        public static void RunStartupTasks()
        {
            try
            {
                MigrateLegacyUserData();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"InstallHelper | User data migration failed: {ex}", LogHelper.LogType.Error);
            }

            try
            {
                CleanupLegacyStartupEntries();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"InstallHelper | Legacy cleanup failed: {ex}", LogHelper.LogType.Error);
            }

            if (!App.IsPackaged)
            {
                try
                {
                    RegisterUnpackagedApp();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLogToFile($"InstallHelper | Unpackaged registration failed: {ex}", LogHelper.LogType.Error);
                }
            }
        }

        private static void MigrateLegacyUserData()
        {
            var target = App.RootPath;
            if (string.IsNullOrEmpty(target))
            {
                return;
            }

            Directory.CreateDirectory(target);

            foreach (var source in GetLegacyDataDirectories())
            {
                if (!Directory.Exists(source) ||
                    string.Equals(
                        Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar),
                        Path.GetFullPath(target).TrimEnd(Path.DirectorySeparatorChar),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var fileName in MigratableFiles)
                {
                    var sourceFile = Path.Combine(source, fileName);
                    var targetFile = Path.Combine(target, fileName);
                    if (!File.Exists(sourceFile) || File.Exists(targetFile))
                    {
                        continue;
                    }

                    try
                    {
                        File.Copy(sourceFile, targetFile, false);
                        LogHelper.WriteLogToFile($"InstallHelper | Migrated '{sourceFile}' to '{targetFile}'.", LogHelper.LogType.Event);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLogToFile($"InstallHelper | Could not migrate '{sourceFile}': {ex.Message}", LogHelper.LogType.Error);
                    }
                }
            }
        }

        private static IEnumerable<string> GetLegacyDataDirectories()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(localAppData))
            {
                yield return Path.Combine(localAppData, "Ink Canvas Artistry");
                yield return Path.Combine(localAppData, "Ink Canvas");
            }

            var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrEmpty(roamingAppData))
            {
                yield return Path.Combine(roamingAppData, "Ink Canvas Artistry");
                yield return Path.Combine(roamingAppData, "Ink Canvas");
            }

            var baseDirectory = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                yield return baseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            }
        }

        private static void CleanupLegacyStartupEntries()
        {
            using (var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (runKey != null)
                {
                    foreach (var name in LegacyStartupValueNames)
                    {
                        if (runKey.GetValue(name) == null)
                        {
                            continue;
                        }

                        runKey.DeleteValue(name, false);
                        LogHelper.WriteLogToFile($"InstallHelper | Removed legacy startup entry '{name}'.", LogHelper.LogType.Event);
                    }
                }
            }

            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (!string.IsNullOrEmpty(startupFolder))
            {
                foreach (var name in LegacyShortcutNames)
                {
                    var path = Path.Combine(startupFolder, name);
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    try
                    {
                        File.Delete(path);
                        LogHelper.WriteLogToFile($"InstallHelper | Removed legacy startup shortcut '{path}'.", LogHelper.LogType.Event);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLogToFile($"InstallHelper | Could not remove '{path}': {ex.Message}", LogHelper.LogType.Error);
                    }
                }
            }

            using var classes = Registry.CurrentUser.OpenSubKey(ClassesKeyPath, true);
            if (classes == null)
            {
                return;
            }

            foreach (var progId in LegacyProgIds)
            {
                if (classes.OpenSubKey(progId) != null)
                {
                    classes.DeleteSubKeyTree(progId, false);
                    LogHelper.WriteLogToFile($"InstallHelper | Removed legacy file association '{progId}'.", LogHelper.LogType.Event);
                }
            }

            foreach (var (extension, _, _) in FileAssociations)
            {
                using var openWith = classes.OpenSubKey(Path.Combine(extension, "OpenWithProgids"), true);
                if (openWith == null)
                {
                    continue;
                }

                foreach (var progId in LegacyProgIds)
                {
                    if (openWith.GetValue(progId) != null)
                    {
                        openWith.DeleteValue(progId, false);
                    }
                }
            }
        }

        private static void RegisterUnpackagedApp()
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath))
            {
                return;
            }

            using var classes = Registry.CurrentUser.CreateSubKey(ClassesKeyPath);
            using var registeredApplications = Registry.CurrentUser.CreateSubKey(RegisteredApplicationsKeyPath);
            using var capabilities = Registry.CurrentUser.CreateSubKey(CapabilitiesKeyPath);

            registeredApplications.SetValue(ApplicationName, CapabilitiesKeyPath);
            capabilities.SetValue("ApplicationName", ApplicationName);
            capabilities.SetValue("ApplicationDescription", "FluentCanvas ink annotation board");

            using (var fileAssociations = capabilities.CreateSubKey("FileAssociations"))
            {
                foreach (var (extension, progId, description) in FileAssociations)
                {
                    fileAssociations.SetValue(extension, progId);

                    using var progIdKey = classes.CreateSubKey(progId);
                    progIdKey.SetValue(null, description);

                    using (var iconKey = progIdKey.CreateSubKey("DefaultIcon"))
                    {
                        iconKey.SetValue(null, $"\"{exePath}\",0");
                    }

                    using (var commandKey = progIdKey.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue(null, $"\"{exePath}\" \"%1\"");
                    }

                    using var extensionKey = classes.CreateSubKey(extension);
                    using var openWith = extensionKey.CreateSubKey("OpenWithProgids");
                    openWith.SetValue(progId, Array.Empty<byte>(), RegistryValueKind.None);
                }
            }

            using (var appPath = Registry.CurrentUser.CreateSubKey(AppPathsKeyPath))
            {
                appPath.SetValue(null, exePath);
                appPath.SetValue("Path", Path.GetDirectoryName(exePath));
            }

            unsafe { PInvoke.SHChangeNotify(SHCNE_ID.SHCNE_ASSOCCHANGED, SHCNF_FLAGS.SHCNF_IDLIST, null, null); }
            LogHelper.WriteLogToFile("InstallHelper | Registered unpackaged file associations.", LogHelper.LogType.Event);
        }

        public static void UnregisterUnpackagedApp()
        {
            if (App.IsPackaged)
            {
                return;
            }

            using (var classes = Registry.CurrentUser.OpenSubKey(ClassesKeyPath, true))
            {
                if (classes != null)
                {
                    foreach (var (_, progId, _) in FileAssociations)
                    {
                        classes.DeleteSubKeyTree(progId, false);
                    }
                }
            }

            Registry.CurrentUser.DeleteSubKeyTree(@"Software\FluentCanvas", false);
            Registry.CurrentUser.DeleteSubKeyTree(AppPathsKeyPath, false);

            using (var registeredApplications = Registry.CurrentUser.OpenSubKey(RegisteredApplicationsKeyPath, true))
            {
                registeredApplications?.DeleteValue(ApplicationName, false);
            }

            unsafe { PInvoke.SHChangeNotify(SHCNE_ID.SHCNE_ASSOCCHANGED, SHCNF_FLAGS.SHCNF_IDLIST, null, null); }
            LogHelper.WriteLogToFile("InstallHelper | Unregistered unpackaged file associations.", LogHelper.LogType.Event);
        }
    }
}
