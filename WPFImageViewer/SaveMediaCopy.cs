using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.FileIO;

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
                saveFile.OverwritePrompt = false;
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
                        //File.Copy(filePath, saveFile.FileName);
                        FileSystem.CopyFile(filePath, saveFile.FileName, UIOption.AllDialogs);
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == -2147024816)//File already exits exception
                        {
                            try
                            {
                                File.Delete(saveFile.FileName);
                                FileSystem.CopyFile(filePath, saveFile.FileName, UIOption.AllDialogs);
                                //File.Copy(filePath, saveFile.FileName);
                            }
                            catch (OperationCanceledException)
                            {

                            }
                            catch (FileNotFoundException)
                            {
                                MessageBox.Show("File not found, operation cancelled!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File not found, operation cancelled!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                throw;
            }
        } 
        #endregion
    }
}
