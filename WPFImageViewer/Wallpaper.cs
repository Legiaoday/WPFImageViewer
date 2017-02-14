using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace WPFImageViewer
{
    public sealed class Wallpaper
    {
        Wallpaper() { }


        #region Interop stuff
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni); 
        #endregion


        public enum Style : int
        {
            Tile,
            Center,
            Stretch,
            Fit,
            Fill
        }


        #region Set wallpaper method
        public static void Set(string filePath, Style style)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(filePath);
            bitmapImage.EndInit();

            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            filePath = Path.GetTempPath() + "wallpaperWPFIMGV.jpg";

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (style)
            {
                case Style.Stretch:
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case Style.Fit:
                    key.SetValue(@"WallpaperStyle", "6");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case Style.Fill:
                    key.SetValue(@"WallpaperStyle", "10");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case Style.Center:
                    key.SetValue(@"WallpaperStyle", "1");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case Style.Tile:
                    key.SetValue(@"WallpaperStyle", "1");
                    key.SetValue(@"TileWallpaper", "1");
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                filePath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        } 
        #endregion
    }
}
