using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.IO;

namespace WPFImageViewer
{

    static class CropImage
    {
        #region GetCrop
        public static BitmapImage GetCrop(Image mainImage, Point clickDown, Point clickRelease, string defaultMedia, ref int[] originalXY)
        {
            System.Drawing.Bitmap bmpOriginal = new System.Drawing.Bitmap(defaultMedia);

            originalXY = new int[] { bmpOriginal.Width, bmpOriginal.Height };
            Point startRec = new Point(Math.Min(clickDown.X, clickRelease.X), Math.Min(clickDown.Y, clickRelease.Y));
            Point recDimensions = new Point(Math.Abs(clickDown.X - clickRelease.X), Math.Abs(clickDown.Y - clickRelease.Y));

            startRec = ConvertClick(bmpOriginal.Width, bmpOriginal.Height, startRec, mainImage.ActualWidth, mainImage.ActualHeight);
            recDimensions = ConvertClick(bmpOriginal.Width, bmpOriginal.Height, recDimensions, mainImage.ActualWidth, mainImage.ActualHeight);//yep, I'm using the convertClick method to convert width and height of the rectangle

            #region Correct outbounds rectangle
            if (startRec.X < 0)//checks if the rectangle is outbounds and corrects it
            {
                recDimensions.X += startRec.X;
                startRec.X = 0;
            }

            if (startRec.X + recDimensions.X > bmpOriginal.Width)//checks if the rectangle is outbounds and corrects it
            {
                recDimensions.X = bmpOriginal.Width - startRec.X;
            }

            if (startRec.Y < 0)//checks if the rectangle is outbounds and corrects it
            {
                recDimensions.Y += startRec.Y;
                startRec.Y = 0;
            }

            if (startRec.Y + recDimensions.Y > bmpOriginal.Height)//checks if the rectangle is outbounds and corrects it
            {
                recDimensions.Y = bmpOriginal.Height - startRec.Y;
            }
            #endregion

            if (recDimensions.X >= 1 && recDimensions.Y >= 1)
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(Convert.ToInt32(startRec.X), Convert.ToInt32(startRec.Y), Convert.ToInt32(recDimensions.X), Convert.ToInt32(recDimensions.Y));

                System.Drawing.Bitmap bmpCrop = new System.Drawing.Bitmap(rect.Width, rect.Height);
                System.Drawing.Graphics gCrop = System.Drawing.Graphics.FromImage(bmpCrop);
                System.Drawing.Rectangle dstRect = new System.Drawing.Rectangle(0, 0, rect.Width, rect.Height);
                gCrop.DrawImage(bmpOriginal, dstRect, rect, System.Drawing.GraphicsUnit.Pixel);

                using (MemoryStream memory = new MemoryStream())
                {
                    bmpCrop.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memory;
                    bitmapImage.EndInit();

                    gCrop.Dispose();
                    bmpCrop.Dispose();
                    bmpOriginal.Dispose();

                    return bitmapImage;
                }
            }

            return null;
        }


        public static BitmapImage ResizeCropPlus(Image mainImage, Point clickDown, Point clickRelease, string defaultMedia, double[] newSize, double[] newXY)
        {
            System.Drawing.Bitmap bmpOriginal = new System.Drawing.Bitmap(defaultMedia);
            Point startRec = new Point(newXY[0], newXY[1]);

            #region Correct outbounds rectangle
            if (startRec.X < 0)//checks if the rectangle is outbounds and corrects it
            {
                newSize[0] += startRec.X;
                startRec.X = 0;
            }

            if (startRec.X + newSize[0] > bmpOriginal.Width)//checks if the rectangle is outbounds and corrects it
            {
                newSize[0] = bmpOriginal.Width - startRec.X;
            }

            if (startRec.Y < 0)//checks if the rectangle is outbounds and corrects it
            {
                newSize[1] += startRec.Y;
                startRec.Y = 0;
            }

            if (startRec.Y + newSize[1] > bmpOriginal.Height)//checks if the rectangle is outbounds and corrects it
            {
                newSize[1] = bmpOriginal.Height - startRec.Y;
            }
            #endregion


            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(Convert.ToInt32(startRec.X), Convert.ToInt32(startRec.Y), Convert.ToInt32(newSize[0]), Convert.ToInt32(newSize[1]));

            System.Drawing.Bitmap bmpCrop = new System.Drawing.Bitmap(rect.Width, rect.Height);
            System.Drawing.Graphics gCrop = System.Drawing.Graphics.FromImage(bmpCrop);
            System.Drawing.Rectangle dstRect = new System.Drawing.Rectangle(0, 0, rect.Width, rect.Height);
            gCrop.DrawImage(bmpOriginal, dstRect, rect, System.Drawing.GraphicsUnit.Pixel);

            using (MemoryStream memory = new MemoryStream())
            {
                bmpCrop.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();

                gCrop.Dispose();
                bmpCrop.Dispose();
                bmpOriginal.Dispose();

                return bitmapImage;
            }
        }
        #endregion


        #region ConvertClick
        public static Point ConvertClick(int bmpWidth, int bmpHeight, Point clickOverMedia, double mainImageWidth, double mainImageHeight)
        {
            Point newClick = new Point();

            newClick.X = (bmpWidth * clickOverMedia.X) / mainImageWidth;//rule of three to convert the mouse left click X to an equivalent on the image
            newClick.Y = (bmpHeight * clickOverMedia.Y) / mainImageHeight;//rule of three to convert the mouse left click Y to an equivalent on the image

            return newClick;
        } 
        #endregion


        #region GetClickPosOverMedia
        public static Point GetClickPosOverMedia(Point mousePos, Point mainMediaGridDimensions, Point mainImageDimensions, Point mainWindowPos, WindowState windowState)
        {
            Point click = new Point();

            if (windowState == WindowState.Maximized)
            {
                mainWindowPos = new Point(0, 0);
            }

            if (mainMediaGridDimensions.Y == mainImageDimensions.Y && mainMediaGridDimensions.X != mainImageDimensions.X)// the mainMedia is touching the bottom and top of the mainWindow
            {
                click = new Point(mousePos.X - mainWindowPos.X - ((mainMediaGridDimensions.X - mainImageDimensions.X) / 2), mousePos.Y - mainWindowPos.Y);
            }
            else if (mainMediaGridDimensions.X == mainImageDimensions.X && mainMediaGridDimensions.Y != mainImageDimensions.Y)//the mainMedia is touching the sides of the mainWindow
            {
                click = new Point(mousePos.X - mainWindowPos.X, mousePos.Y - mainWindowPos.Y - ((mainMediaGridDimensions.Y - mainImageDimensions.Y) / 2));
            }
            else if (mainMediaGridDimensions.X == mainImageDimensions.X && mainMediaGridDimensions.Y == mainImageDimensions.Y)//the mainMedia is touching the all sides of the mainWindow
            {
                click = new Point(mousePos.X - mainWindowPos.X, mousePos.Y - mainWindowPos.Y);
            }
            else//the mainMedia is not touching any sides of the mainWindow
            {
                click = new Point(mousePos.X - mainWindowPos.X - ((mainMediaGridDimensions.X - mainImageDimensions.X) / 2), mousePos.Y - mainWindowPos.Y - ((mainMediaGridDimensions.Y - mainImageDimensions.Y) / 2));
            }

            return click;
        } 
        #endregion

    }
}
