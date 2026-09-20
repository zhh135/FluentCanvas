using FluentCanvas.Helpers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {
        private void SaveScreenshot(bool isHideNotification, string fileName = null)
        {
            try
            {
                var basePath = StorageHelper.NormalizeStorageLocation(Settings.Automation.AutoSavedStrokesLocation);
                Settings.Automation.AutoSavedStrokesLocation = basePath;

                string savePath = basePath + @"\Auto Saved - Screenshots";
                if (fileName == null) fileName = DateTime.Now.ToString("u").Replace(":", "-");
                if (Settings.Automation.IsSaveScreenshotsInDateFolders)
                {
                    savePath += @"\" + DateTime.Now.ToString("yyyy-MM-dd");
                }
                savePath += @"\" + fileName + ".png";

                StorageHelper.EnsureDirectory(Path.GetDirectoryName(savePath));

                var bitmap = GetScreenshotBitmap();
                bitmap.Save(savePath, ImageFormat.Png);

                if (Settings.Automation.IsAutoSaveStrokesAtScreenshot)
                {
                    SaveInkCanvasFile(false, false);
                }
                if (!isHideNotification)
                {
                    ShowNotificationAsync("截图成功保存至 " + savePath);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"SaveScreenshot failed: {ex}", LogHelper.LogType.Error);
                if (!isHideNotification)
                {
                    ShowNotificationAsync("截图保存失败，请检查保存路径设置");
                }
            }
        }

        private void SaveScreenShotToDesktop()
        {
            try
            {
                var bitmap = GetScreenshotBitmap();
                string savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = DateTime.Now.ToString("u").Replace(':', '-') + ".png";
                bitmap.Save(savePath + @"\" + fileName, ImageFormat.Png);
                ShowNotificationAsync("截图成功保存至【桌面" + @"\" + fileName + "】");
                if (Settings.Automation.IsAutoSaveStrokesAtScreenshot) SaveInkCanvasFile(false, false);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"SaveScreenShotToDesktop failed: {ex}", LogHelper.LogType.Error);
                ShowNotificationAsync("截图保存失败");
            }
        }

        private void SavePPTScreenshot(string fileName)
        {
            try
            {
                var basePath = StorageHelper.NormalizeStorageLocation(Settings.Automation.AutoSavedStrokesLocation);
                Settings.Automation.AutoSavedStrokesLocation = basePath;

                string savePath = basePath + @"\Auto Saved - PPT Screenshots";
                if (Settings.Automation.IsSaveScreenshotsInDateFolders)
                {
                    savePath += @"\" + DateTime.Now.ToString("yyyy-MM-dd");
                }
                if (fileName == null) fileName = DateTime.Now.ToString("u").Replace(":", "-");
                savePath += @"\" + fileName + ".png";

                StorageHelper.EnsureDirectory(Path.GetDirectoryName(savePath));

                var bitmap = GetScreenshotBitmap();
                bitmap.Save(savePath, ImageFormat.Png);

                if (Settings.Automation.IsAutoSaveStrokesAtScreenshot)
                {
                    SaveInkCanvasFile(false, false);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"SavePPTScreenshot failed: {ex}", LogHelper.LogType.Error);
            }
        }

        private Bitmap GetScreenshotBitmap()
        {
            Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
            {
                memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }
    }
}
