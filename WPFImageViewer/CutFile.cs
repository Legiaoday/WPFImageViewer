using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace WPFImageViewer
{
    class CutFile
    {
        static public bool MoveFile(string filePath)
        {
            //returns true if the file is moved or false it the user cancelled the operation

            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.OverwritePrompt = false;
                saveFile.Title = "Move File To";
                saveFile.FileName = Path.GetFileNameWithoutExtension(filePath);

                switch (Path.GetExtension(filePath))
                {
                    case ".jpg":
                        saveFile.Filter = "JPEG (*.jpg)|*.jpg";
                        break;
                    case ".png":
                        saveFile.Filter = "PNG (*.png)|*.png";
                        break;
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
                    case ".jpeg":
                        saveFile.Filter = "JPEG (*.jpeg)|*.jpeg";
                        break;
                }

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileSystem.MoveFile(filePath, saveFile.FileName, UIOption.AllDialogs);
                        if (!File.Exists(filePath)) return true; else return false;
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("File not found, operation cancelled!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }
                else
                {
                    return false;
                }  
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
