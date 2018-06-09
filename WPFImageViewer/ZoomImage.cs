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
        //halfZoomModifier is used to zoom the upper/bottom half of the image. A number 2 will zoom him exactly half of the screen, a lower number than 2 will zoom in less and a higher number will zoom in more than half
        //for example: 1.5 will zoom in 75% of either the upper or bottom part of the image
        private const double halfZoomModifier = 2;

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
        public static void PerformZoomTop(Image mainImage, Border ImageBackGround, Point mainWindowDimensions, short zoomIndex, short halfZoomIncrementPercent)
        {
            //halfZoomModifier is used to zoom in on either the upper or bottom portion of the image. zoomIndex allows for the zoom percentage (halfZoomIncrementPercent) to stack a certain amount of times
            //doubling the size of mainWindowDimensions.Y means that 50% of either the upper or bottom part of the image will be zoomed on screen
            //four times mainWindowDimensions.Y means that only 25% of the upper or bottom part of the image will be zoomed on screen
            //halfZoomModifier is used to determined how many times mainWindowDimensions.Y will be multiplied, the higher its value the stronger the zoom will be, until the image fills the entire window area
            double halfZoomModifier = Convert.ToDouble(100 - (halfZoomIncrementPercent * zoomIndex));

            //a division by zero can occur and the value of halfZoomModifier can end up being infinity or negative
            //we need to check if this happens, otherwise an error will be thrown
            if (!double.IsPositiveInfinity(halfZoomModifier) && halfZoomModifier > 0)
                halfZoomModifier = 100 / halfZoomModifier;
            else
                halfZoomModifier = 100;

            mainImage.Width = mainWindowDimensions.X;
            mainImage.Height = mainWindowDimensions.Y * halfZoomModifier;//mainImage.Height = mainWindowDimensions.Y * halfZoomModifier;

            double maxHeightPossible = (mainImage.Width * mainImage.ActualHeight) / mainImage.ActualWidth;//rule of three to convert the height of the image to an equivalent of the width based of the width of the current window
            if (mainImage.Height > maxHeightPossible || halfZoomModifier >= 100)//this if is needed because when the value of halfZoomModifier is too high (like 100) the half zoom will not work properly
                mainImage.Height = (mainImage.Width * mainImage.ActualHeight) / mainImage.ActualWidth;//rule of three to convert the height of the image to an equivalent of the width based of the width of the current window

            Thickness margin = mainImage.Margin;
            margin.Left = 0;
            margin.Top = 0;
            mainImage.Margin = margin;
            ImageBackGround.Margin = margin;
        }
        #endregion


        #region PerformeZoomBottom
        public static void PerformZoomBottom(Image mainImage, Border ImageBackGround, Point mainWindowDimensions, short zoomIndex, short halfZoomIncrementPercent)
        {
            //halfZoomModifier is used to zoom in on either the upper or bottom portion of the image. zoomIndex allows for the zoom percentage (halfZoomIncrementPercent) to stack a certain amount of times
            //doubling the size of mainWindowDimensions.Y means that 50% of either the upper or bottom part of the image will be zoomed on screen
            //four times mainWindowDimensions.Y means that only 25% of the upper or bottom part of the image will be zoomed on screen
            //halfZoomModifier is used to determined how many times mainWindowDimensions.Y will be multiplied, the higher its value the stronger the zoom will be, until the image fills the entire window area
            zoomIndex *= -1;
            double halfZoomModifier = Convert.ToDouble(100 - (halfZoomIncrementPercent * zoomIndex));

            //a division by zero can occur and the value of halfZoomModifier can end up being infinity or negative
            //we need to check if this happens, otherwise an error will be thrown
            if (!double.IsPositiveInfinity(halfZoomModifier) && halfZoomModifier > 0)
                halfZoomModifier = 100 / halfZoomModifier;
            else
                halfZoomModifier = 100;

            mainImage.Width = mainWindowDimensions.X;
            mainImage.Height = mainWindowDimensions.Y * halfZoomModifier;

            double maxHeightPossible = (mainImage.Width * mainImage.ActualHeight) / mainImage.ActualWidth;//rule of three to convert the height of the image to an equivalent of the width based of the width of the current window
            if (mainImage.Height > maxHeightPossible || halfZoomModifier >= 100)//this if is needed because when the value of halfZoomModifier is too high (like 100) the half zoom will not work properly
                mainImage.Height = (mainImage.Width * mainImage.ActualHeight) / mainImage.ActualWidth;//rule of three to convert the height of the image to an equivalent of the width based of the width of the current window

            Thickness margin = mainImage.Margin;
            margin.Left = 0;

            margin.Top = (mainImage.Height - mainWindowDimensions.Y) * -1;
            mainImage.Margin = margin;
            ImageBackGround.Margin = margin;
        }
        #endregion


        #region GetSourceDimensions
        public static int[] GetSourceDimensions(string defaultMedia)
        {
            int[] dimensions = new int[2];

            try
            {
                using (var imageStream = File.OpenRead(defaultMedia))
                {
                    var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                    dimensions[0] = decoder.Frames[0].PixelWidth;
                    dimensions[1] = decoder.Frames[0].PixelHeight;

                    return dimensions;
                }
            }
            catch (FileNotFoundException)
            {
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
