using System;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;

namespace WPFImageViewer
{
    static class SaveImageAs
    {
        #region SaveImage
        static public void SaveImage(string filePath)
        {
            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                saveFile.FileName = Path.GetFileNameWithoutExtension(filePath);

                switch (Path.GetExtension(filePath))
                {
                    case ".jpg":
                        saveFile.Filter = "JPG (*.jpg)|*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif|JPEG (*.jpeg)|*.jpeg";
                        break;
                    case ".png":
                        saveFile.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|JPEG (*.jpeg)|*.jpeg";
                        break;
                    case ".gif":
                        saveFile.Filter = "GIF (*.gif)|*.gif|JPG (*.jpg)|*.jpg|PNG (*.png)|*.png|JPEG (*.jpeg)|*.jpeg";
                        break;
                    case ".jpeg":
                        saveFile.Filter = "JPG (*.jpg)|*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif|JPEG (*.jpeg)|*.jpeg";
                        break;
                }

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = new Uri(filePath);
                    bitmapImage.EndInit();

                    string ext = Path.GetExtension(saveFile.FileName);
                    switch (ext)
                    {
                        case ".jpg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case ".png":
                            encoder = new PngBitmapEncoder();
                            break;
                        case ".gif":
                            encoder = new GifBitmapEncoder();
                            break;
                        case ".jpeg":
                            encoder = new JpegBitmapEncoder();
                            break;
                    }

                    if (Path.GetExtension(saveFile.FileName) == Path.GetExtension(filePath) & Path.GetFileName(saveFile.FileName) != Path.GetFileName(filePath) || Path.GetDirectoryName(saveFile.FileName) != Path.GetDirectoryName(filePath))
                    {
                        try
                        {
                            File.Copy(filePath, saveFile.FileName);
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult == -2147024816)//File already exits exception
                            {
                                File.Delete(saveFile.FileName);
                                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                                using (var fileStream = new FileStream(saveFile.FileName, FileMode.Create))
                                {
                                    encoder.Save(fileStream);
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    else if (saveFile.FileName != filePath)
                    {
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                        using (var fileStream = new FileStream(saveFile.FileName, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        } 
        #endregion
    }
}
