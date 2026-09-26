using FluentCanvas.Helpers;
using FluentCanvas.Services;
using FluentCanvas.Views;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {
        private void SymbolIconSaveStrokes_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.Visibility != Visibility.Visible) return;
            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);
            if (Locator.Get<IAppViewService>().TryGet<INotificationsView>(out var notificationsView))
            {
                notificationsView.HideView();
            }
            SaveInkCanvasFile(true, true);
        }

        private void SaveInkCanvasFile(bool newNotice = true, bool saveByUser = false)
        {
            try
            {
                var basePath = StorageHelper.NormalizeStorageLocation(Settings.Automation.AutoSavedStrokesLocation);
                Settings.Automation.AutoSavedStrokesLocation = basePath;

                string savePath = basePath
                    + (saveByUser ? @"\User Saved - " : @"\Auto Saved - ")
                    + (currentMode == 0 ? "Annotation Strokes" : "BlackBoard Strokes");

                StorageHelper.EnsureDirectory(savePath);

                string savePathWithName = savePath + @"\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff")
                    + (currentMode != 0 ? " Page-" + CurrentWhiteboardIndex + " StrokesCount-" + inkCanvas.Strokes.Count + ".icart" : ".icart");

                using (FileStream fs = new FileStream(savePathWithName, FileMode.Create))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    // Save strokes
                    var strokesEntry = archive.CreateEntry("strokes.icstk");
                    using (var strokesStream = strokesEntry.Open())
                    {
                        inkCanvas.Strokes.Save(strokesStream);
                    }

                    // Save UI elements
                    var elementsEntry = archive.CreateEntry("elements.xaml");
                    using (var elementsStream = elementsEntry.Open())
                    {
                        XamlWriter.Save(inkCanvas, elementsStream);
                    }

                    // Save related URL files
                    SaveRelatedUrlFiles(archive);

                    if (newNotice)
                    {
                        ShowNotificationAsync("墨迹及元素成功保存至 " + savePathWithName);
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowNotificationAsync("墨迹及元素保存失败！");
                LogHelper.WriteLogToFile("墨迹及元素保存失败 | " + Ex.ToString(), LogHelper.LogType.Error);
            }
        }

        private void SaveRelatedUrlFiles(ZipArchive archive)
        {
            string dependencyFolder = "File Dependency";
            var folderEntry = archive.CreateEntry(dependencyFolder + "/");
            foreach (UIElement element in inkCanvas.Children)
            {
                if (element is Image image && image.Source is BitmapImage bitmapImage && bitmapImage.UriSource != null)
                {
                    AddFileToArchive(archive, bitmapImage.UriSource.LocalPath, dependencyFolder);
                }
                else if (element is MediaElement mediaElement && mediaElement.Source != null)
                {
                    AddFileToArchive(archive, mediaElement.Source.LocalPath, dependencyFolder);
                }
                else
                {
                    LogHelper.WriteLogToFile("该元素类型暂不支持保存", LogHelper.LogType.Error);
                }
            }
        }

        private void AddFileToArchive(ZipArchive archive, string filePath, string folderName)
        {
            if (File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                var fileEntry = archive.CreateEntry(folderName + "/" + fileName);
                using (var entryStream = fileEntry.Open())
                using (var fileStream = File.OpenRead(filePath))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }



        private void SymbolIconOpenInkCanvasFile_Click(object sender, RoutedEventArgs e)
        {
            AnimationsHelper.HideWithSlideAndFade(BorderTools);
            AnimationsHelper.HideWithSlideAndFade(BoardBorderTools);

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Settings.Automation.AutoSavedStrokesLocation,
                Title = "打开墨迹文件",
                Filter = "FluentCanvas Files (*.icart;*.icstk)|*.icart;*.icstk|FluentCanvas Files (*.icart)|*.icart|FluentCanvas Stroke Files (*.icstk)|*.icstk"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadInkCanvasFile(openFileDialog.FileName);
            }
        }

        public void LoadInkCanvasFile(string filePath)
        {
                LogHelper.WriteLogToFile($"Strokes Insert: Name: {filePath}", LogHelper.LogType.Event);

                try
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    using (var fs = new FileStream(filePath, FileMode.Open))
                    {
                        if (extension == ".icart")
                        {
                            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
                            {
                                // load strokes
                                var strokesEntry = archive.GetEntry("strokes.icstk");
                                if (strokesEntry != null)
                                {
                                    using (var strokesStream = strokesEntry.Open())
                                    {
                                        var strokes = new StrokeCollection(strokesStream);
                                        ClearStrokes(true);
                                        timeMachine.ClearStrokeHistory();
                                        inkCanvas.Strokes.Add(strokes);
                                        LogHelper.NewLog($"Strokes Insert: Strokes Count: {inkCanvas.Strokes.Count}");
                                    }
                                }

                                // load URL files
                                string saveDirectory = Settings.Automation.AutoSavedStrokesLocation;
                                ExtractUrlFiles(archive, saveDirectory);

                                // load UI Elements
                                var elementsEntry = archive.GetEntry("elements.xaml");
                                using (var elementsStream = elementsEntry.Open())
                                {
                                    try
                                    {
                                        var loadedCanvas = XamlReader.Load(elementsStream) as InkCanvas;
                                        if (loadedCanvas != null)
                                        {
                                            inkCanvas.Children.Clear();
                                            foreach (UIElement child in loadedCanvas.Children)
                                            {
                                                var xaml = XamlWriter.Save(child);
                                                UIElement clonedChild = (UIElement)XamlReader.Parse(xaml);
                                                if (clonedChild is MediaElement mediaElement)
                                                {
                                                    mediaElement.LoadedBehavior = MediaState.Manual;
                                                    mediaElement.UnloadedBehavior = MediaState.Manual;
                                                    mediaElement.Loaded += async (_, args) =>
                                                    {
                                                        mediaElement.Play();
                                                        await Task.Delay(100);
                                                        mediaElement.Pause();
                                                    };
                                                }
                                                inkCanvas.Children.Add(clonedChild);
                                            }
                                            LogHelper.NewLog($"Elements Insert: Elements Count: {inkCanvas.Children.Count}");
                                        }
                                    }
                                    catch (XamlParseException xamlEx)
                                    {
                                        LogHelper.WriteLogToFile($"XAML 解析错误: {xamlEx.Message}", LogHelper.LogType.Error);
                                        ShowNotificationAsync("加载 UI 元素时出现 XAML 解析错误");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.WriteLogToFile($"加载 UI 元素失败: {ex.Message}", LogHelper.LogType.Error);
                                        ShowNotificationAsync("加载 UI 元素失败");
                                    }
                                }
                            }
                        }
                        else if (extension == ".icstk")
                        {
                            // 直接加载 .icstk 文件中的墨迹
                            using (var strokesStream = new MemoryStream())
                            {
                                fs.CopyTo(strokesStream);
                                strokesStream.Seek(0, SeekOrigin.Begin);
                                var strokes = new StrokeCollection(strokesStream);
                                ClearStrokes(true);
                                timeMachine.ClearStrokeHistory();
                                inkCanvas.Strokes.Add(strokes);
                                LogHelper.NewLog($"Strokes Insert: Strokes Count: {inkCanvas.Strokes.Count}");
                            }
                        }
                        else
                        {
                            ShowNotificationAsync("不支持的文件格式。");
                        }

                        if (inkCanvas.Visibility != Visibility.Visible)
                        {
                            SymbolIconCursor_Click(null, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowNotificationAsync("墨迹或元素打开失败");
                    LogHelper.WriteLogToFile($"打开墨迹或元素失败: {ex.Message}\n{ex.StackTrace}", LogHelper.LogType.Error);
                }
        }

        private void ExtractUrlFiles(ZipArchive archive, string outputDirectory)
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith("File Dependency/", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        string fileName = Path.Combine(outputDirectory, entry.FullName);

                        string directoryPath = Path.GetDirectoryName(fileName);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        if (!File.Exists(fileName))
                        {
                            entry.ExtractToFile(fileName, overwrite: false);
                        }
                    }
                }
            }
        }
    }
}
