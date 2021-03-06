﻿using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Input;
using MyControlsLibrary;

namespace WPFImageViewer
{
    public partial class CropWindow : Window
    {
        MainWindow globalMainWindow;
        TransformedBitmap tbOriginal = new TransformedBitmap();
        CustomTitleBar titleBar;

        public CropWindow(MainWindow mainWindow, BitmapImage bitmapImage)
        {
            if (bitmapImage != null)
            {
                InitializeComponent();
                initTitleBar();
                Width = SystemParameters.PrimaryScreenWidth / 2;
                Height = SystemParameters.PrimaryScreenHeight / 2;
                RenderOptions.SetBitmapScalingMode(croppedImage, BitmapScalingMode.NearestNeighbor);//scaling mode. BitmapScalingMode.NearestNeighbor disables the smoothing when zooming the image
                croppedImage.Source = bitmapImage;
                globalMainWindow = mainWindow;

                tbOriginal = new TransformedBitmap();
                tbOriginal.BeginInit();
                tbOriginal.Source = bitmapImage;
                tbOriginal.EndInit();

                widthTxt.Text = Convert.ToUInt32(bitmapImage.Width).ToString();
                heightTxt.Text = Convert.ToUInt32(bitmapImage.Height).ToString();
                getOriginalClick();
                updateCropBkp();
            }
            else
            {
                Close();
            }
        }

        #region Flip
        private void flipImage(short flipH, short flipV)
        {
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = tbOriginal;
            var transform = new ScaleTransform(flipH, flipV, 0, 0);
            tb.Transform = transform;
            tb.EndInit();

            croppedImage.Source = tb;
            tbOriginal = new TransformedBitmap();
            tbOriginal.BeginInit();
            tbOriginal.Source = tb;
            tbOriginal.EndInit();
        }


        private void flipHor_Click(object sender, RoutedEventArgs e)
        {
            flipImage(-1,1);
        }


        private void flipVer_Click(object sender, RoutedEventArgs e)
        {
            flipImage(1,-1);
        }
        #endregion

        #region Invert colors
        private void invert_Click(object sender, RoutedEventArgs e)
        {
            BitmapInvertColors(convertToBmp());
        }


        private Bitmap convertToBmp()
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(tbOriginal));
                enc.Save(outStream);
                Bitmap bmp = new Bitmap(outStream);

                return bmp;
            }
        }


        public void BitmapInvertColors(Bitmap bitmapImage)
        {
            var bitmapRead = bitmapImage.LockBits(new Rectangle(0, 0, bitmapImage.Width, bitmapImage.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var bitmapLength = bitmapRead.Stride * bitmapRead.Height;
            var bitmapBGRA = new byte[bitmapLength];
            Marshal.Copy(bitmapRead.Scan0, bitmapBGRA, 0, bitmapLength);
            bitmapImage.UnlockBits(bitmapRead);

            for (int i = 0; i < bitmapLength; i += 4)
            {
                bitmapBGRA[i] = (byte)(255 - bitmapBGRA[i]);
                bitmapBGRA[i + 1] = (byte)(255 - bitmapBGRA[i + 1]);
                bitmapBGRA[i + 2] = (byte)(255 - bitmapBGRA[i + 2]);
                //        [i + 3] = ALPHA.
            }

            var bitmapWrite = bitmapImage.LockBits(new Rectangle(0, 0, bitmapImage.Width, bitmapImage.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Marshal.Copy(bitmapBGRA, 0, bitmapWrite.Scan0, bitmapLength);
            bitmapImage.UnlockBits(bitmapWrite);

            using (MemoryStream memory = new MemoryStream())
            {
                bitmapImage.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = memory;
                bmp.EndInit();

                croppedImage.Source = bmp;
                tbOriginal = new TransformedBitmap();
                tbOriginal.BeginInit();
                tbOriginal.Source = bmp;
                tbOriginal.EndInit();
            }

            bitmapImage.Dispose();
        }
        #endregion

        #region Rotate
        private void rotateImage(short angle)
        {
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = tbOriginal;
            RotateTransform transform = new RotateTransform(angle);
            tb.Transform = transform;
            tb.EndInit();

            croppedImage.Source = tb;
            tbOriginal = new TransformedBitmap();
            tbOriginal.BeginInit();
            tbOriginal.Source = tb;
            tbOriginal.EndInit();
        }


        private void rotate90right_Click(object sender, RoutedEventArgs e)
        {
            rotateImage(90);
        }


        private void rotate90left_Click(object sender, RoutedEventArgs e)
        {
            rotateImage(-90);
        }


        private void rotate180_Click(object sender, RoutedEventArgs e)
        {
            rotateImage(180);
        }
        #endregion

        #region saveCrop_Click
        private void saveCrop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.FileName = Path.GetFileNameWithoutExtension(globalMainWindow.defaultMedia);
                BitmapEncoder encoder = new PngBitmapEncoder();

                switch (Path.GetExtension(globalMainWindow.defaultMedia))
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

                if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
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

                    encoder.Frames.Add(BitmapFrame.Create(tbOriginal));

                    if (saveFile.FileName == globalMainWindow.defaultMedia)//Overwrites the file if necessary
                    {
                       

                        using (var fileStream = new FileStream(saveFile.FileName, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }

                        setImageFromStream();
                    }
                    else
                    {
                        using (var fileStream = new FileStream(saveFile.FileName, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }
                    }

                    Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region setImageFromStream
        private void setImageFromStream()
        {
            BitmapImage bitmapImage = new BitmapImage();  // Create new BitmapImage  
            Stream stream = new MemoryStream();  // Create new MemoryStream  
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(globalMainWindow.defaultMedia);  // Create new Bitmap (System.Drawing.Bitmap) from the existing image file (bmpSource set to its path name)  
            bitmap.Save(stream, ImageFormat.Png);  // Save the loaded Bitmap into the MemoryStream - Png format was the only one I tried that didn't cause an error (tried Jpg, Bmp, MemoryBmp)  
            bitmap.Dispose();  // Dispose bitmap so it releases the source image file  
            bitmapImage.BeginInit();  // Begin the BitmapImage's initialisation  
            bitmapImage.StreamSource = stream;  // Set the BitmapImage's StreamSource to the MemoryStream containing the image  
            bitmapImage.EndInit();  // End the BitmapImage's initialisation  
            globalMainWindow.mainImage.Source = bitmapImage;  // Finally, set the WPF Image component's source to the BitmapImage 
        }
        #endregion

        #region Generic events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SystemParameters.PrimaryScreenWidth / 2 > 640 && SystemParameters.PrimaryScreenHeight / 2 > 360)
            {
                Width = SystemParameters.PrimaryScreenWidth / 2;
                Height = SystemParameters.PrimaryScreenHeight / 2;
            }

            Left = (SystemParameters.PrimaryScreenWidth / 2) - (Width / 2);
            Top = (SystemParameters.PrimaryScreenHeight / 2) - (Height / 2);

            WindowState = WindowState.Maximized;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
        #endregion

        #region Resize crop
        int[] bkpWidthHeight;
        int[] bkpXY;


        private void adjustWH_Click(object sender, RoutedEventArgs e)
        {
            doResizeWH();
        }


        private void doResizeWH()
        {
            if (widthTxt.Text != "" && heightTxt.Text != "" && double.Parse(widthTxt.Text) > 0 && double.Parse(heightTxt.Text) > 0)
            {
                if (XTxt.Text != "" && YTxt.Text != "" && double.Parse(XTxt.Text) >= 0 && double.Parse(YTxt.Text) >= 0 && double.Parse(XTxt.Text) < globalMainWindow.originalXY[0] && double.Parse(YTxt.Text) < globalMainWindow.originalXY[1])
                {
                    if (File.Exists(globalMainWindow.defaultMedia))
                    {
                        double[] newSize = new double[] { double.Parse(widthTxt.Text), double.Parse(heightTxt.Text) };
                        double[] newXY = new double[] { double.Parse(XTxt.Text), double.Parse(YTxt.Text) };
                        BitmapImage tempBmp = CropImage.ResizeCropPlus(globalMainWindow.mainImage, globalMainWindow.clickDown, globalMainWindow.clickRelease, globalMainWindow.defaultMedia, newSize, newXY);
                        croppedImage.Source = tempBmp;

                        tbOriginal = new TransformedBitmap();
                        tbOriginal.BeginInit();
                        tbOriginal.Source = tempBmp;
                        tbOriginal.EndInit();

                        widthTxt.Text = Convert.ToUInt32(tempBmp.Width).ToString();
                        heightTxt.Text = Convert.ToUInt32(tempBmp.Height).ToString();
                        XTxt.Text = newXY[0].ToString();
                        YTxt.Text = newXY[1].ToString();
                        updateCropBkp();
                        return;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("File does not exist!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("X and Y must be equal or greater than zero and lesser than the original image dimensions!\n\nOriginal dimensions:\nWidth = " + globalMainWindow.originalXY[0] + "\nHeight = " + globalMainWindow.originalXY[1], "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Width and Height must be greater than zero!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            restoreCropBkp();
        }


        void restoreCropBkp ()
        {
            widthTxt.Text = bkpWidthHeight[0].ToString();
            heightTxt.Text = bkpWidthHeight[1].ToString();

            XTxt.Text = bkpXY[0].ToString();
            YTxt.Text = bkpXY[1].ToString();
        }


        void updateCropBkp ()
        {
            bkpWidthHeight = new int[] { Int32.Parse(widthTxt.Text), Int32.Parse(heightTxt.Text) };
            bkpXY = new int[] { Int32.Parse(XTxt.Text), Int32.Parse(YTxt.Text) };
        }


        private void widthTxt_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }


        private void widthTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }


        private void widthTxt_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                doResizeWH();
            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }


        private void getOriginalClick()
        {
            System.Windows.Point startRec = new System.Windows.Point(Math.Min(globalMainWindow.clickDown.X, globalMainWindow.clickRelease.X), Math.Min(globalMainWindow.clickDown.Y, globalMainWindow.clickRelease.Y));
            startRec = CropImage.ConvertClick(globalMainWindow.originalXY[0], globalMainWindow.originalXY[1], startRec, globalMainWindow.mainImage.ActualWidth, globalMainWindow.mainImage.ActualHeight);


            #region Correct outbounds rectangle
            if (startRec.X < 0)//checks if the rectangle is outbounds and corrects it
            {
                startRec.X = 0;
            }

            if (startRec.Y < 0)//checks if the rectangle is outbounds and corrects it
            {
                startRec.Y = 0;
            }
            #endregion

            XTxt.Text = Convert.ToUInt32(startRec.X).ToString();
            YTxt.Text = Convert.ToUInt32(startRec.Y).ToString();
        }


        private void adjustXY_Click(object sender, RoutedEventArgs e)
        {
            doResizeWH();
        }


        private void XTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }


        private void XTxt_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                doResizeWH();
            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }


        private void XTxt_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
        #endregion

        private void initTitleBar()
        {
            titleBar = new CustomTitleBar(this, mainGrid);//overloaded function that also accepts an icon as parameter, this icon parameter is optional
            titleBar.MinimizeButtonVisibility = Visibility.Collapsed;
            titleBar.Text = "";
            titleBar.Height = 22;//recommended height = 22
            titleBar.IsAutoHide = true;
            titleBar.AutoHideDelay = 2000;
            titleBar.IsPlayAnimation = true;
            titleBar.AnimationInterval = 15;
            titleBar.BackgroundOpacity = 0.0;        
            titleBar.WindowDragMode = CustomTitleBar.DragMode.None;
            titleBar.DoubleClickResize = false;
            titleBar.FullScreenMode = true;
        }
    }
}