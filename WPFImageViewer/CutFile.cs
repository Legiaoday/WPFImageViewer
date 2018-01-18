﻿using System;
using System.Windows.Forms;
using System.IO;

namespace WPFImageViewer
{
    class CutFile
    {
        #region Move file
        static public bool MoveFile(string filePath)
        {
            //returns true if the file is moved or false it the user cancelled the operation

            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();
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
                        File.Move(filePath, saveFile.FileName);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == -2147024713)//File already exits exception
                        {
                            try
                            {
                                File.Delete(saveFile.FileName);
                                File.Move(filePath, saveFile.FileName);
                                return true;
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

                return false;          
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
    }
}
