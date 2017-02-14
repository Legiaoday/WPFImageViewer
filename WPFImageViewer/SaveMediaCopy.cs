using System;
using System.Windows.Forms;
using System.IO;

namespace WPFImageViewer
{
    static class SaveMediaCopy
    {
        #region SaveMedia
        static public void SaveMedia(string filePath)
        {
            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.FileName = Path.GetFileNameWithoutExtension(filePath);

                switch (Path.GetExtension(filePath))
                {
                    case ".gif":
                        saveFile.Filter = "GIF (*.gif)|*.gif";
                        break;
                    case ".mp4":
                        saveFile.Filter = "MP4 (*.mp4)|*.mp4";
                        break;
                    case ".webm":
                        saveFile.Filter = "WEBM (*.webm)|*.webm";
                        break;
                    case ".mkv":
                        saveFile.Filter = "MKV (*.mkv)|*.mkv";
                        break;
                    case ".avi":
                        saveFile.Filter = "AVI (*.avi)|*.avi";
                        break;
                    case ".mp3":
                        saveFile.Filter = "MP3 (*.mp3)|*.mp3";
                        break;
                }

                if (saveFile.ShowDialog() == DialogResult.OK)
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
                            File.Copy(filePath, saveFile.FileName);
                        }
                        else
                        {
                            throw;
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
