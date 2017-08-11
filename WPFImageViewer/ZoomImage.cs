using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System;

namespace WPFImageViewer
{
    class ZoomImage
    {
        private static Point mainImageDimensions;
        private static Point mainWindowPos;
        private static Point mainMediaGridDimensions;


        #region PerformeZoom
        public static void PerformeZoom(Image mainImage, Border ImageBackGround, string defaultMedia, WindowState arg3, Point windowPos, Point mainMediaGridDimen, short zoomPercentage)
        {
            int[] dimensions = GetSourceDimensions(defaultMedia);

            if (dimensions[0] < mainMediaGridDimen.X && dimensions[1] < mainMediaGridDimen.Y)//if the source image is smaller than the mainImage the zoom will be applied to the mainImage dimensions rather than the actual source image dimensions
            {
                dimensions[0] = Convert.ToInt32(mainImage.ActualWidth);
                dimensions[1] = Convert.ToInt32(mainImage.ActualHeight);
            }

            dimensions[0] = dimensions[0] * zoomPercentage / 100;
            dimensions[1] = dimensions[1] * zoomPercentage / 100;
            mainImageDimensions = new Point(mainImage.ActualWidth, mainImage.ActualHeight);
            mainMediaGridDimensions = mainMediaGridDimen;

            if (arg3 == WindowState.Normal)
            {
                mainWindowPos = windowPos;
            }
            else
            {
                mainWindowPos = new Point(0, 0);
            }

            Point mousePos = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            Point clickOverMedia = GetClickPosOverMedia(mousePos);
            Point clickOverImage = ConvertClick(dimensions[0], dimensions[1], clickOverMedia);
            double[] newMargin = GetCorrectMargin(dimensions, clickOverImage, mainImage);

            mainImage.Width = dimensions[0];
            mainImage.Height = dimensions[1];

            Thickness margin = mainImage.Margin;
            margin.Left = newMargin[0];
            margin.Top = newMargin[1];
            mainImage.Margin = margin;
            ImageBackGround.Margin = margin;
        }
        #endregion


        #region PerformeZoomTop
        public static void PerformeZoomTop(Image mainImage, Border ImageBackGround, Point mainWindowDimensions)
        {
            mainImage.Width = mainWindowDimensions.X;
            mainImage.Height = mainWindowDimensions.Y * 2;

            Thickness margin = mainImage.Margin;
            margin.Left = 0;
            margin.Top = 0;
            mainImage.Margin = margin;
            ImageBackGround.Margin = margin;
        }
        #endregion


        #region PerformeZoomBottom
        public static void PerformeZoomBottom(Image mainImage, Border ImageBackGround, Point mainWindowDimensions)
        {
            mainImage.Width = mainWindowDimensions.X;
            mainImage.Height = mainWindowDimensions.Y * 2;

            Thickness margin = mainImage.Margin;
            margin.Left = 0;
            margin.Top = mainImage.ActualHeight * -1;
            mainImage.Margin = margin;
            ImageBackGround.Margin = margin;
        }
        #endregion


        #region GetSourceDimensions
        public static int[] GetSourceDimensions(string defaultMedia)
        {
            int[] dimensions = new int[2];

            using (var imageStream = File.OpenRead(defaultMedia))
            {
                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile,
                    BitmapCacheOption.Default);
                dimensions[0] = decoder.Frames[0].PixelWidth;
                dimensions[1] = decoder.Frames[0].PixelHeight;

                return dimensions;
            }
        }
        #endregion


        #region GetCorrectMargin
        private static double[] GetCorrectMargin(int[] dimensions, Point clickOverImage, Image mainImage)
        {
            double[] newMargin = new double[2];

            if (clickOverImage.Y <= (dimensions[1] - (mainMediaGridDimensions.Y / 2)))
            {
                newMargin[1] = (mainMediaGridDimensions.Y / 2) - clickOverImage.Y;
            }
            else
            {
                newMargin[1] = -(clickOverImage.Y) + (dimensions[1] - clickOverImage.Y);
            }

            if (clickOverImage.X <= (dimensions[0] - (mainMediaGridDimensions.X / 2)))
            {
                newMargin[0] = (mainMediaGridDimensions.X / 2) - clickOverImage.X;
            }
            else
            {
                newMargin[0] = -(clickOverImage.X) + (dimensions[0] - clickOverImage.X);
            }

            return newMargin;
        }
        #endregion


        #region RevertZoom
        public static void RevertZoom(Image mainImage, Border ImageBackGround, Grid mainMediaGrid, string defaultMedia)
        {
            mainImage.Width = mainMediaGrid.ActualWidth;
            mainImage.Height = mainMediaGrid.ActualHeight;
            ImageBackGround.Width = mainMediaGrid.ActualWidth;
            ImageBackGround.Height = mainMediaGrid.ActualHeight;

            Thickness margin = mainImage.Margin;
            margin.Left = 0;
            margin.Top = 0;
            margin.Right = 0;
            margin.Bottom = 0;
            mainImage.Margin = margin;
            ImageBackGround.Margin = margin;
        }
        #endregion


        #region ConvertClick
        private static Point ConvertClick(int bmpWidth, int bmpHeight, Point clickOverMedia)
        {
            Point newClick = new Point();

            newClick.X = (bmpWidth * clickOverMedia.X) / mainImageDimensions.X;//rule of three to convert the mouse left click X to an equivalent on the image
            newClick.Y = (bmpHeight * clickOverMedia.Y) / mainImageDimensions.Y;//rule of three to convert the mouse left click Y to an equivalent on the image

            return newClick;
        }
        #endregion


        #region GetClickPosOverMedia
        private static Point GetClickPosOverMedia(Point mousePos)
        {
            Point click = new Point();

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
