#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Windows.Threading;
#endregion

namespace WPFImageViewer
{
    public partial class MainWindow : Window
    {
        #region Global variables
        DispatcherTimer timer;
        List<string> listOfFiles = new List<string>();//holds all the supported media located in the same folder as the current media
        int defaultImageIndex = 0;//used to navigate throught the listOfFiles variable
        int previousImageIndex = 0;//used to navigate throught the listOfFiles variable
        public string defaultMedia = null;//holds the path of the current media
        bool isMediaPlaying = true;
        short zoomIndex = 0;
        System.Drawing.Point lastDrag = new System.Drawing.Point();
        int[] originalImageDimensions = new int[2];
        bool isDraggingImage = false;
        XMLSettings settings = new XMLSettings();
        bool isCropEnabled = false;
        Rectangle rectangle = new Rectangle();
        Point clickDown;//Left mouse click down
        Point clickRelease;//Left mouse click release
        Point currentPos;//current mouse position
        CancellationTokenSource tokenSourceTitle = new CancellationTokenSource();
        CancellationTokenSource tokenSourceNavigation = new CancellationTokenSource();
        Thickness margin;
        short zoomPercentage = 100;
        bool zoomPreventLoop = false;
        bool firstZoomCheck = true;
        string fileName;
        bool isSettingsChanged = false;
        bool isInitResize = false;
        bool hasTimeSpan = false;
        Line line;
        Line line2;
        Line line3;
        Line line4;
        double lineWidth = 0;
        double lineHeight = 0;
        bool afterPPreventLoop = false;
        bool firstAfterPCheck = true;
        TaskbarItemInfo taskBarMedia = new TaskbarItemInfo();
        double mediaTotsec = 0;
        MediaPlayer _mediaPlayer;
        private uint[] framePixels;
        private uint[] previousFramePixels;
        bool orderBPreventLoop = false;
        bool firstOrderBCheck = true;
        bool isResizing = false;
        Point startPt;
        double reWidth = 0, reheight = 0;
        Point newPt;
        Point oriMediaRatio = new Point();
        bool firstRatioCheck = true;
        LoadingControl loading = new LoadingControl();
        enum MediaType { Video, Audio, Image }
        enum MediaExtension { JPEG, PNG, GIF, MP4, WEBM, MKV, AVI, MP3, WAV, WMV }
        MediaType mediaType;
        MediaExtension mediaExtension;
        #endregion


        public MainWindow(string[] args)
        {
            InitializeComponent();
            loadConfigs(args);
            //#if DEBUG
            //initializeDebug(); 
            //#endif
        }


        #region Generic events
        #region mainMedia_MediaOpened
        private void mainMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            oriMediaRatio = new Point(mainMedia.NaturalVideoWidth, mainMedia.NaturalVideoHeight);

            if (mainMedia.NaturalDuration.HasTimeSpan)
            {
                if (TaskbarItemInfo == null) { TaskbarItemInfo = taskBarMedia; }
                taskBarMedia.ProgressValue = 0;
                taskBarMedia.ProgressState = TaskbarItemProgressState.Normal;
                mediaTotsec = mainMedia.NaturalDuration.TimeSpan.TotalSeconds;
                TimeSpan ts = mainMedia.NaturalDuration.TimeSpan;
                seekBar.Maximum = ts.TotalSeconds;
                seekBar.SmallChange = 1;
                seekBar.LargeChange = Math.Min(10, ts.Seconds / 10);
                timer.Start();
                hasTimeSpan = true;

                lblmediaTimeSpan.Content = mainMedia.NaturalDuration.ToString().Substring(0, 8);
                showVideoControls();
                navigationGrid.IsHitTestVisible = true;
            }
            else
            {
                hasTimeSpan = false;
                if (TaskbarItemInfo != null) TaskbarItemInfo = null;
            }

            loading.StopAnimation();
        }
        #endregion


        #region timer_Tick
        void timer_Tick(object sender, EventArgs e)
        {
            seekBar.Value = mainMedia.Position.TotalSeconds;
            taskBarMedia.ProgressValue = seekBar.Value / mediaTotsec;
            lblseekerValue.Content = mainMedia.Position.Hours.ToString("D2") + ":" + mainMedia.Position.Minutes.ToString("D2") + ":" + mainMedia.Position.Seconds.ToString("D2");
        }
        #endregion


        private void mainWindow_Deactivated(object sender, EventArgs e)
        {
            if (isCropEnabled)
            {
                deactivateCrop();
            }
        }


        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loadConfigsDelay();
        }


        private void mainMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            switch (settings.AfterPlayback)
            {
                case XMLSettings.AfterP.Loop:
                    if (hasTimeSpan) mainMedia.Position = TimeSpan.Zero;
                    else mainMedia.Position = new TimeSpan(0, 0, 0, 0, 1);
                    break;
                case XMLSettings.AfterP.Next:
                    if (listOfFiles.Count > 1)
                    {
                        nextButton_Click(sender, e);
                    }
                    else
                    {
                        mainMedia.Stop();
                        isMediaPlaying = false;
                        taskBarMedia.ProgressState = TaskbarItemProgressState.Error;
                        if (hasTimeSpan) mainMedia.Position = TimeSpan.Zero;
                        else mainMedia.Position = new TimeSpan(0, 0, 0, 0, 1);
                    }
                    break;
                case XMLSettings.AfterP.Nothing:
                    mainMedia.Stop();
                    isMediaPlaying = false;
                    taskBarMedia.ProgressState = TaskbarItemProgressState.Error;
                    if (hasTimeSpan) mainMedia.Position = TimeSpan.Zero;
                    else mainMedia.Position = new TimeSpan(0, 0, 0, 0, 1);
                    break;
            }
        }


        #region mainImage_MouseMove
        private void mainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingImage)
            {
                margin = mainImage.Margin;

                if (margin.Left + System.Windows.Forms.Cursor.Position.X - lastDrag.X > mainImage.ActualWidth * (-1) && margin.Left + System.Windows.Forms.Cursor.Position.X - lastDrag.X < mainMediaGrid.ActualWidth / 2)
                {
                    margin.Left += System.Windows.Forms.Cursor.Position.X - lastDrag.X;
                }

                if (margin.Top + System.Windows.Forms.Cursor.Position.Y - lastDrag.Y > mainImage.ActualHeight * (-1) && margin.Top + System.Windows.Forms.Cursor.Position.Y - lastDrag.Y < mainMediaGrid.ActualHeight / 2)
                {
                    margin.Top += System.Windows.Forms.Cursor.Position.Y - lastDrag.Y;
                }

                mainImage.Margin = margin;
                ImageBackGround.Margin = margin;

                lastDrag = System.Windows.Forms.Cursor.Position;
            }
            else if (isCropEnabled)
            {
                currentPos = e.GetPosition(mainImage);

                if (e.LeftButton == MouseButtonState.Released)
                {
                    drawLine();
                }
                else
                {
                    drawRec();
                }
            }
        }
        #endregion


        #region Main window size changed
        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!isSettingsChanged && isInitResize)
            {
                isSettingsChanged = true;
            }

            if (WindowState == WindowState.Maximized && isInitResize)
            {
                settings.Left = Left;
                settings.Top = Top;
                settings.Width = e.PreviousSize.Width;
                settings.Height = e.PreviousSize.Height;
            }
        }
        #endregion


        private void seekBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mainMedia.Position = TimeSpan.FromSeconds(cPos);
                seekBar.Value = cPos;
            }
        }


        private void mainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (!isSettingsChanged && isInitResize) isSettingsChanged = true;
        }


        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isSettingsChanged)
            {
                settings.Volume = mainMedia.Volume;
                settings.WindowState = WindowState;
                settings.Zoom = zoomPercentage;

                if (WindowState == WindowState.Normal)
                {
                    settings.Left = Left;
                    settings.Top = Top;
                    settings.Width = Width;
                    settings.Height = Height;
                }

                XMLHandler.writeConfigXML(settings);
            }
        }


        private void openExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(defaultMedia))
            {
                return;
            }

            string argument = "/select, \"" + defaultMedia + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }


        #region mainImage_PreviewMouseDown
        private void mainImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isCropEnabled)
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    if (zoomIndex > 0)
                    {
                        revertZoom();
                    }
                }
                else if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (zoomIndex == 0)
                    {
                        Point mainWindowPos = new Point(mainWindow.Left, mainWindow.Top);
                        Point mainMediaGridDimensions = new Point(mainMediaGrid.ActualWidth, mainMediaGrid.ActualHeight);
                        originalImageDimensions = ZoomImage.GetSourceDimensions(defaultMedia);

                        //if (mainMediaGridDimensions.X < originalImageDimensions[0] || mainMediaGridDimensions.Y < originalImageDimensions[1])
                        //{
                            ZoomImage.PerformeZoom(mainImage, ImageBackGround, defaultMedia, WindowState, mainWindowPos, mainMediaGridDimensions, zoomPercentage);
                            zoomIndex++;
                            hideControlsZoom();
                            mainImage.Cursor = Cursors.Hand;
                        //}
                    }
                    else
                    {
                        lastDrag = System.Windows.Forms.Cursor.Position;
                        isDraggingImage = true;
                    }
                }
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    clickDown = e.GetPosition(mainImage);
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    deactivateCrop();
                }
            }
        }
        #endregion


        #region mainImage_PreviewMouseUp
        private void mainImage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (isDraggingImage)
                {
                    isDraggingImage = false;
                }
                else if (isCropEnabled)
                {
                    if (rectangle.Width > 0 && rectangle.Height > 0 && !Double.IsNaN(rectangle.Width) && !Double.IsNaN(rectangle.Height))
                    {
                        clickRelease = e.GetPosition(mainImage);
                        CropWindow cropWindow = new CropWindow(this, CropImage.GetCrop(mainImage, clickDown, clickRelease, defaultMedia));
                        try
                        {
                            cropWindow.ShowDialog();
                        }
                        catch (InvalidOperationException)
                        {
                        }

                        deactivateCrop();
                        forceGC();
                    }

                    deactivateCrop();
                }
            }
        }
        #endregion


        private void mainImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isDraggingImage)
            {
                isDraggingImage = false;
            }
        }


        private void openMedia_Click(object sender, RoutedEventArgs e)
        {
            openPicture();
            forceGC();
        }


        private void closeApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        #region nextButton_Click
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (listOfFiles.Count > 1)
            {
                previousImageIndex = defaultImageIndex;

                if (defaultImageIndex < (listOfFiles.Count - 1))
                {
                    defaultImageIndex++;
                }
                else if (defaultImageIndex == (listOfFiles.Count - 1))
                {
                    defaultImageIndex = 0;
                }

                defaultMedia = System.IO.Path.GetDirectoryName(defaultMedia) + "\\" + listOfFiles[defaultImageIndex];
                setMainMedia();
            }
            forceGC();
        }
        #endregion


        #region previousButton_Click
        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (listOfFiles.Count > 1)
            {
                previousImageIndex = defaultImageIndex;

                if (defaultImageIndex > 0)
                {
                    defaultImageIndex--;
                }
                else if (defaultImageIndex == 0)
                {
                    defaultImageIndex = (listOfFiles.Count - 1);
                }

                defaultMedia = System.IO.Path.GetDirectoryName(defaultMedia) + "\\" + listOfFiles[defaultImageIndex];
                setMainMedia();
            }
            forceGC();
        }
        #endregion



        #region mainWindow_PreviewKeyDown
        private void mainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (mediaType == MediaType.Video || mediaType == MediaType.Audio)
            {
                if (e.Key == Key.Space)
                {
                    if (isMediaPlaying)
                    {
                        mainMedia.Pause();
                        isMediaPlaying = false;
                        taskBarMedia.ProgressState = TaskbarItemProgressState.Paused;
                    }
                    else
                    {
                        mainMedia.Play();
                        isMediaPlaying = true;
                        taskBarMedia.ProgressState = TaskbarItemProgressState.Normal;
                    }
                }
                else if (e.Key == Key.Left)
                {
                    mainMedia.Position = TimeSpan.FromSeconds(mainMedia.Position.TotalSeconds - 5);
                }
                else if (e.Key == Key.Right)
                {
                    mainMedia.Position = TimeSpan.FromSeconds(mainMedia.Position.TotalSeconds + 5);
                }
            }
            else
            {
                if (e.Key == Key.Left)
                {
                    RoutedEventArgs eM = new RoutedEventArgs();

                    previousButton_Click(sender, eM);
                }
                else if (e.Key == Key.Right)
                {
                    RoutedEventArgs eM = new RoutedEventArgs();

                    nextButton_Click(sender, eM);
                }
                else if (e.Key == Key.Space && mediaExtension == MediaExtension.GIF)
                {
                    if (isMediaPlaying)
                    {
                        mainMedia.Pause();
                        isMediaPlaying = false;
                    }
                    else
                    {
                        mainMedia.Play();
                        isMediaPlaying = true;
                    }
                }
                else if (e.Key == Key.Up)
                {
                    if (zoomIndex == 0)
                    {
                        Point mainWindowDimensions = new Point(mainWindow.ActualWidth, mainWindow.ActualHeight);
                        originalImageDimensions = ZoomImage.GetSourceDimensions(defaultMedia);

                        //if (mainWindowDimensions.X < originalImageDimensions[0] || mainWindowDimensions.Y < originalImageDimensions[1])
                        //{
                            if (originalImageDimensions[0] < originalImageDimensions[1] && mainWindowDimensions.X > mainWindowDimensions.Y)
                            {
                                ZoomImage.PerformeZoomTop(mainImage, ImageBackGround, mainWindowDimensions);
                                zoomIndex++;
                                hideControlsZoom();
                                mainImage.Cursor = Cursors.Hand;

                                if (WindowState == WindowState.Normal)
                                {
                                    ResizeMode = ResizeMode.CanMinimize;
                                }
                            }
                        //}
                    }
                    else if (zoomIndex > 0)
                    {
                        revertZoom();
                    }
                }
                else if (e.Key == Key.Down)
                {
                    if (zoomIndex == 0)
                    {
                        Point mainWindowDimensions = new Point(mainWindow.ActualWidth, mainWindow.ActualHeight);
                        originalImageDimensions = ZoomImage.GetSourceDimensions(defaultMedia);

                        //if (mainWindowDimensions.X < originalImageDimensions[0] || mainWindowDimensions.Y < originalImageDimensions[1])
                        //{
                            if (originalImageDimensions[0] < originalImageDimensions[1] && mainWindowDimensions.X > mainWindowDimensions.Y)
                            {
                                ZoomImage.PerformeZoomBottom(mainImage, ImageBackGround, mainWindowDimensions);
                                zoomIndex++;
                                hideControlsZoom();
                                mainImage.Cursor = Cursors.Hand;

                                if (WindowState == WindowState.Normal)
                                {
                                    ResizeMode = ResizeMode.CanMinimize;
                                }
                            }
                        //}
                    }
                    else if (zoomIndex > 0)
                    {
                        revertZoom();
                    }
                }
                else if (e.Key == Key.C && mediaExtension != MediaExtension.GIF)
                {
                    if (!isCropEnabled)
                    {
                        drawFirstLine();
                        RoutedEventArgs eM = new RoutedEventArgs();
                        cropMode_Click(sender, eM);
                    }
                    else
                    {
                        deactivateCrop();
                    }
                }
            }

            if (e.Key == Key.Delete)
            {
                deleteFile();
            }
        }
        #endregion


        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
        }


        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private void titleBarGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            hideTitleControls();
        }


        private void titleBarGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            showTitleControls();
        }


        private void navigationGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            showNavigationControls();
        }


        private void navigationGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            hideNavigationControls();
        }


        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mainMedia.Volume = volumeSlider.Value / 100;
            if (!isSettingsChanged && isInitResize) isSettingsChanged = true;

            try
            {
                lblVolume.Content = Convert.ToUInt32(volumeSlider.Value);
            }
            catch (Exception)
            {

            }
        }


        private void volumeTriangle_MouseEnter(object sender, MouseEventArgs e)
        {
            showVolumeControls();
        }


        private void volumeTriangle_MouseLeave(object sender, MouseEventArgs e)
        {
            hideVolumeControls();
        }


        private void volumeTriangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (mainMedia.IsMuted)
                {
                    mainMedia.IsMuted = false;
                    volumeTriangle.Fill = Brushes.DarkSlateBlue;
                    settings.IsMuted = false;
                }
                else
                {
                    mainMedia.IsMuted = true;
                    volumeTriangle.Fill = Brushes.Red;
                    settings.IsMuted = true;
                }

                isSettingsChanged = true;
            }
        }


        private void mainMedia_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isMediaPlaying)
                {
                    mainMedia.Pause();
                    isMediaPlaying = false;
                    taskBarMedia.ProgressState = TaskbarItemProgressState.Paused;
                }
                else
                {
                    mainMedia.Play();
                    isMediaPlaying = true;
                    taskBarMedia.ProgressState = TaskbarItemProgressState.Normal;
                }
            }
        }


        private void mainMediaGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (zoomIndex == 0)
            {
                mainImage.Width = mainMediaGrid.ActualWidth;
                mainImage.Height = mainMediaGrid.ActualHeight;
            }
        }


        private void dragGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }


        private void mainImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ImageBackGround.Width = mainImage.ActualWidth;
            ImageBackGround.Height = mainImage.ActualHeight;
            imageCanvas.Width = mainImage.ActualWidth;
            imageCanvas.Height = mainImage.ActualHeight;
            lineWidth = mainImage.ActualWidth;
            lineHeight = mainImage.ActualHeight;
        }


        private void cropMode_Click(object sender, RoutedEventArgs e)
        {
            if (zoomIndex == 0)
            {
                drawFirstLine();
                Mouse.Capture(mainImage);
                isCropEnabled = true;
                mainImage.Cursor = Cursors.Cross;
                hideControlsZoom();
            }
        }


        private void saveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImageAs.SaveImage(defaultMedia);
        }


        private void saveMediaCopy_Click(object sender, RoutedEventArgs e)
        {
            SaveMediaCopy.SaveMedia(defaultMedia);
        }


        private void nextButtonGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            showNextButton();
        }


        private void nextButtonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            hideNextButton();
        }


        private void previousButtonGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            showPreviousButton();
        }

        private void previousButtonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            hidePreviousButton();
        }


        private void mainWindow_StateChanged(object sender, EventArgs e)
        {
            dragGrid.Visibility = Visibility.Collapsed;
        }


        #region backgroundMenuItem_Click
        private void backgroundMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;

            switch (obj.Name)
            {
                case "stretch":
                    Wallpaper.Set(defaultMedia, Wallpaper.Style.Stretch);
                    break;
                case "fit":
                    Wallpaper.Set(defaultMedia, Wallpaper.Style.Fit);
                    break;
                case "fill":
                    Wallpaper.Set(defaultMedia, Wallpaper.Style.Fill);
                    break;
                case "center":
                    Wallpaper.Set(defaultMedia, Wallpaper.Style.Center);
                    break;
                case "tile":
                    Wallpaper.Set(defaultMedia, Wallpaper.Style.Tile);
                    break;
            }
        }
        #endregion
        #endregion


        #region Generic methods
        #region setMainMedia
        public void setMainMedia()
        {
            #region Variables/objects reset
            //Do not change the order of any of the objects/variables in this region
            navigationGrid.IsHitTestVisible = false;
            navigationGridWhite.Visibility = Visibility.Collapsed;
            timer.Stop();
            mainMedia.Stop();
            getMediaType();
            if (isMediaPlaying && mediaType == MediaType.Image) { loading.StopAnimation(); }
            isMediaPlaying = false;
            lblFileName.Content = null;
            #endregion

            try
            {
                if (mediaType == MediaType.Video || mediaExtension == MediaExtension.GIF || mediaType == MediaType.Audio)
                {
                    #region Variables/objects reset
                    if (mainImage.Source != null) mainImage.Source = null;
                    seekBar.Value = 0;
                    lblseekerValue.Content = null;
                    lblmediaTimeSpan.Content = null;

                    #region Check same media
                    //this block is used to check if the same media is being loaded twice in a roll, when that happens the loading animation needs to be stopped bellow because the media opened event won't fire
                    string previousMedia = null;

                    if (mainMedia.Source != null)
                    {
                        previousMedia = mainMedia.Source.LocalPath;
                    }  
                    #endregion
                    #endregion

                    if (File.Exists(defaultMedia))
                    {              
                        mainMedia.Source = new Uri(defaultMedia, UriKind.Absolute);                      
                        fileName = System.IO.Path.GetFileName(defaultMedia);
                        mainMedia.Visibility = Visibility.Visible;
                        mainMedia.Play();
                        isMediaPlaying = true;

                        if (previousMedia != mainMedia.Source.LocalPath) loading.PlayAnimation(); else loading.StopAnimation();//this line is used to check if the same media is being loaded twice in a roll, when that happens the loading animation needs to be stopped here because the media opened event won't fire
                    }
                    else
                    {
                        fileName = "File not found!";
                        mainMedia.Source = null;
                    }
                }
                else
                {
                    #region Variables/objects resets
                    if (mainMedia.Source != null) mainMedia.Source = null;                  
                    if (TaskbarItemInfo != null) TaskbarItemInfo = null;
                    mainMedia.Visibility = Visibility.Collapsed;
                    #endregion

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(defaultMedia);
                    image.EndInit();
                    oriMediaRatio = new Point(image.PixelWidth, image.PixelHeight);
                    mainImage.Source = image;
                    fileName = System.IO.Path.GetFileName(defaultMedia);

                    ImageBackGround.Visibility = (mediaExtension == MediaExtension.PNG) ? ImageBackGround.Visibility = Visibility.Visible : ImageBackGround.Visibility = Visibility.Collapsed;
                }
            }
            catch (FileNotFoundException)
            {
                mainImage.Source = null;
                fileName = "File not found!";
            }

            Title = fileName;
            lblFileName.Content = fileName + " (" + (defaultImageIndex + 1) + "/" + listOfFiles.Count + ")";
            hideVideoControls();
            showTitleControls();
            hideTitleControls();

            if (zoomIndex > 0)
            {
                revertZoom();
            }
        }
        #endregion


        #region loadConfigs
        private void loadConfigs(string[] args)
        {
            #region settings
            settings = XMLHandler.loadConfigXML();
            WindowState = settings.WindowState;
            mainMedia.Volume = settings.Volume;
            volumeSlider.Value = settings.Volume * 100;
            zoomPercentage = settings.Zoom;
            if (settings.IsMuted)
            {
                mainMedia.IsMuted = true;
                volumeTriangle.Fill = Brushes.Red;
            }
            #endregion

            #region Lines
            line = new Line();
            line.StrokeThickness = 1;
            line.Stroke = Brushes.Red;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            line2 = new Line();
            line2.StrokeThickness = 1;
            line2.Stroke = Brushes.SkyBlue;
            line2.SnapsToDevicePixels = true;
            line2.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            line3 = new Line();
            line3.StrokeThickness = 1;
            line3.Stroke = Brushes.Red;
            line3.SnapsToDevicePixels = true;
            line3.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            line4 = new Line();
            line4.StrokeThickness = 1;
            line4.Stroke = Brushes.SkyBlue;
            line4.SnapsToDevicePixels = true;
            line4.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            #endregion

            rectangle.Stroke = Brushes.Red;
            rectangle.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = 0.25f };

            hideTitleControls();
            zoomMenuItem.Header = "Zoom (" + zoomPercentage + "%)";
            navigationGridWhite.Visibility = Visibility.Collapsed;
            dragGrid.Visibility = Visibility.Collapsed;
            hideNextButton();
            hidePreviousButton();
            hideVolumeControls();
            hideVideoControls();
            lblVolume.Content = Convert.ToUInt32(volumeSlider.Value);
            taskBarMedia.ProgressState = TaskbarItemProgressState.Normal;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler(timer_Tick);

            timerSeek = new DispatcherTimer();
            timerSeek.Interval = TimeSpan.FromMilliseconds(25);
            timerSeek.Tick += new EventHandler(updatePopup);

            if (args.Length == 1)
            {
                defaultMedia = args[0];
                setMainMedia();
                getFolderMedia();
            }
        }
        #endregion


        #region getFolderMedia
        private void getFolderMedia()
        {
            if (defaultMedia != null)
            {
                try
                {
                    string extensions = "*.jpg,*.png,*.gif,*.jpeg,*.mp4,*.webm,*.mkv,*.avi, *.mp3";

                    switch (settings.OrderBy)
                    {
                        case XMLSettings.OrderB.Name:
                            listOfFiles = Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(defaultMedia), "*.*").Select(path => System.IO.Path.GetFileName(path)).Where(s => extensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToList();
                            listOfFiles = listOfFiles.OrderBy(x => PadNumbers(x)).ToList();//sorts the filenames properly. PadNumbers method is used to pad the numbers
                            break;
                        case XMLSettings.OrderB.Date:
                            var listFileToRename = new List<FileToRename>();
                            listOfFiles = Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(defaultMedia), "*.*").Select(path => System.IO.Path.GetFileName(path)).Where(s => extensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToList();

                            for (int i = 0; i < listOfFiles.Count; i++)
                            {
                                FileInfo fileInfo = new FileInfo(System.IO.Path.GetDirectoryName(defaultMedia) + "\\" + listOfFiles[i]);

                                listFileToRename.Add(new FileToRename
                                {
                                    fileName = listOfFiles[i],
                                    createDate = fileInfo.LastWriteTime.ToString("yyyyMMddHHmmss")
                                });
                            }

                            listFileToRename = listFileToRename.OrderByDescending(e => e.createDate).ToList();
                            listOfFiles = new List<string>();

                            for (int i = 0; i < listFileToRename.Count; i++)
                            {
                                listOfFiles.Add(listFileToRename[i].fileName);
                            }
                            break;
                    }

                    setCurrentImageIndex();
                    lblFileName.Content = System.IO.Path.GetFileName(defaultMedia) + " (" + (defaultImageIndex + 1) + "/" + listOfFiles.Count + ")";
                }
                catch (Exception ex)
                {

                }

            }
        }
        #endregion


        private void revertZoom()
        {
            zoomIndex--;
            ZoomImage.RevertZoom(mainImage, ImageBackGround, mainMediaGrid, defaultMedia);
            showControlsZoom();
            mainImage.Cursor = Cursors.Arrow;
        }


        private void openPicture()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "All supported images|*.jpg;*.png;*.gif;*.jpeg;*.mp4;*.webm;*.mkv;*.avi;*.mp3;*.wav;*.wmv";
            openFileDialog1.FileName = null;
            System.Windows.Forms.DialogResult result = openFileDialog1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                defaultMedia = openFileDialog1.FileName;
                setMainMedia();
                getFolderMedia();
            }
        }


        #region getMediaType
        private void getMediaType()
        {
            switch (System.IO.Path.GetExtension(defaultMedia))
            {
                case ".jpg":
                    mediaType = MediaType.Image;
                    mediaExtension = MediaExtension.JPEG;
                    break;
                case ".png":
                    mediaType = MediaType.Image;
                    mediaExtension = MediaExtension.PNG;
                    break;
                case ".gif":
                    mediaType = MediaType.Video;
                    mediaExtension = MediaExtension.GIF;
                    break;
                case ".mp4":
                    mediaType = MediaType.Video;
                    mediaExtension = MediaExtension.MP4;
                    break;
                case ".webm":
                    mediaType = MediaType.Video;
                    mediaExtension = MediaExtension.WEBM;
                    break;
                case ".jpeg":
                    mediaType = MediaType.Image;
                    mediaExtension = MediaExtension.JPEG;
                    break;
                case ".mkv":
                    mediaType = MediaType.Video;
                    mediaExtension = MediaExtension.MKV;
                    break;
                case ".avi":
                    mediaType = MediaType.Video;
                    mediaExtension = MediaExtension.AVI;
                    break;
                case ".wmv":
                    mediaType = MediaType.Video;
                    mediaExtension = MediaExtension.WMV;
                    break;
                case ".mp3":
                    mediaType = MediaType.Audio;
                    mediaExtension = MediaExtension.MP3;
                    break;
                case ".wav":
                    mediaType = MediaType.Audio;
                    mediaExtension = MediaExtension.WAV;
                    break;
            }
        }
        #endregion


        //Adds zeros to the left of a char that constains 0~9 so when it can be sorted properly. Returns a string with the zeros. In the in the actual filename remains untouched
        private static string PadNumbers(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(222, '0'));
        }


        //Updates the current image index. It's not part of the getFolderImages() method for 'some' reason, so let it be for now
        private void setCurrentImageIndex()
        {
            for (int i = 0; i < listOfFiles.Count; i++)
            {
                if (listOfFiles[i] == fileName)
                {
                    defaultImageIndex = previousImageIndex = i;
                    return;
                }
            }
        }


        //Forces the garbage collector to 'collect' unused resource
        private void forceGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        #region Hide/show controls
        private async void hideTitleControls()
        {
            try
            {
                await Task.Delay(2000, tokenSourceTitle.Token);

                titleBarGridWhite.Visibility = Visibility.Collapsed;
                dragGrid.Visibility = Visibility.Collapsed;
            }
            catch (TaskCanceledException ex)
            {

            }
        }


        private async void hideNavigationControls()
        {
            try
            {
                await Task.Delay(1000, tokenSourceNavigation.Token);
                navigationGridWhite.Visibility = Visibility.Collapsed;
            }
            catch (TaskCanceledException ex)
            {

            }
        }


        private void showTitleControls()
        {
            canceLTokenTitle();
            titleBarGridWhite.Visibility = Visibility.Visible;
            lblFileName.Visibility = Visibility.Visible;
            if (WindowState == WindowState.Normal)
            {
                dragGrid.Visibility = Visibility.Visible;
            }
        }



        private void showNavigationControls()
        {
            if (mediaType == MediaType.Video || mediaType == MediaType.Audio)
            {
                canceLTokenNavigation();
                navigationGridWhite.Visibility = Visibility.Visible;
            }
        }


        private void hideVideoControls()
        {
            volumeTriangle.Visibility = Visibility.Collapsed;
            seekBar.Visibility = Visibility.Collapsed;
            lblVolume.Visibility = Visibility.Collapsed;
        }


        private void showVideoControls()
        {
            if (mainMedia.HasAudio)
            {
                volumeTriangle.Visibility = Visibility.Visible;
                lblVolume.Visibility = Visibility.Visible;
            }
            seekBar.Visibility = Visibility.Visible;
        }


        private void hideControlsZoom()
        {
            titleBarGrid.Visibility = Visibility.Collapsed;
            navigationGrid.Visibility = Visibility.Collapsed;
            nextButtonGrid.Visibility = Visibility.Collapsed;
            previousButtonGrid.Visibility = Visibility.Collapsed;
            gripGrid.Visibility = Visibility.Collapsed;
        }


        private void showControlsZoom()
        {
            titleBarGrid.Visibility = Visibility.Visible;
            navigationGrid.Visibility = Visibility.Visible;
            nextButtonGrid.Visibility = Visibility.Visible;
            previousButtonGrid.Visibility = Visibility.Visible;
            gripGrid.Visibility = Visibility.Visible;
        }


        private void hideVolumeControls()
        {
            volumeGrid.Visibility = Visibility.Collapsed;
        }


        private void showVolumeControls()
        {
            volumeGrid.Visibility = Visibility.Visible;
        }


        private void hideNextButton()
        {
            nextButton.Visibility = Visibility.Collapsed;
        }


        private void showNextButton()
        {
            nextButton.Visibility = Visibility.Visible;
        }


        private void hidePreviousButton()
        {
            previousButton.Visibility = Visibility.Collapsed;
        }


        private void showPreviousButton()
        {
            previousButton.Visibility = Visibility.Visible;
        }
        #endregion


        private void loadConfigsDelay()
        {
            lineWidth = mainImage.ActualWidth;
            lineHeight = mainImage.ActualHeight;
            Left = settings.Left;
            Top = settings.Top;
            Width = settings.Width;
            Height = settings.Height;
            isInitResize = true;
            mainMediaGrid.Children.Add(loading.ClientBounds);
        }


        public void canceLTokenTitle()
        {
            tokenSourceTitle.Cancel();
            tokenSourceTitle = new CancellationTokenSource();
        }


        public void canceLTokenNavigation()
        {
            tokenSourceNavigation.Cancel();
            tokenSourceNavigation = new CancellationTokenSource();
        }


        private void deactivateCrop()
        {
            cleanCanvas();
            mainImage.ReleaseMouseCapture();
            isCropEnabled = false;
            mainImage.Cursor = Cursors.Arrow;
            rectangle.Width = 0;
            rectangle.Height = 0;
            showControlsZoom();
        }
        #endregion


        #region Checkable menus
        #region Zoom checkables
        private void zoomManuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            setZoomChecked();
        }

        #region setZoomChecked
        private void setZoomChecked()
        {
            if (firstZoomCheck)
            {
                switch (zoomPercentage)
                {
                    case 25:
                        zoom25.IsChecked = true;
                        break;
                    case 50:
                        zoom50.IsChecked = true;
                        break;
                    case 65:
                        zoom65.IsChecked = true;
                        break;
                    case 75:
                        zoom75.IsChecked = true;
                        break;
                    case 85:
                        zoom85.IsChecked = true;
                        break;
                    case 100:
                        zoom100.IsChecked = true;
                        break;
                    case 150:
                        zoom150.IsChecked = true;
                        break;
                    case 200:
                        zoom200.IsChecked = true;
                        break;
                }

                firstZoomCheck = false;
            }
        }
        #endregion


        #region zoomPercentage_Checked
        private void zoomPercentage_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;

            if (!zoomPreventLoop)
            {
                cleanZoomChecked();
                zoomPreventLoop = true;

                switch (obj.Name)
                {
                    case "zoom25":
                        zoomPercentage = 25;
                        zoom25.IsChecked = true;
                        break;
                    case "zoom50":
                        zoomPercentage = 50;
                        zoom50.IsChecked = true;
                        break;
                    case "zoom65":
                        zoomPercentage = 65;
                        zoom65.IsChecked = true;
                        break;
                    case "zoom75":
                        zoomPercentage = 75;
                        zoom75.IsChecked = true;
                        break;
                    case "zoom85":
                        zoomPercentage = 85;
                        zoom85.IsChecked = true;
                        break;
                    case "zoom100":
                        zoomPercentage = 100;
                        zoom100.IsChecked = true;
                        break;
                    case "zoom150":
                        zoomPercentage = 150;
                        zoom150.IsChecked = true;
                        break;
                    case "zoom200":
                        zoomPercentage = 200;
                        zoom200.IsChecked = true;
                        break;
                }

                zoomMenuItem.Header = "Zoom (" + zoomPercentage + "%)";
                zoomPreventLoop = false;
                if (!isSettingsChanged && isInitResize) isSettingsChanged = true;
            }
        }
        #endregion


        private void cleanZoomChecked()
        {
            zoom25.IsChecked = false;
            zoom50.IsChecked = false;
            zoom65.IsChecked = false;
            zoom75.IsChecked = false;
            zoom85.IsChecked = false;
            zoom100.IsChecked = false;
            zoom150.IsChecked = false;
            zoom200.IsChecked = false;
        }
        #endregion


        #region After playback checkables
        private void afterP_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            setAfterPChecked();
        }


        #region afterPlayback_Checked
        private void afterPlayback_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;

            if (!afterPPreventLoop)
            {
                cleanAfterPChecked();
                afterPPreventLoop = true;

                switch (obj.Name)
                {
                    case "afterPLoop":
                        afterPLoop.IsChecked = true;
                        settings.AfterPlayback = XMLSettings.AfterP.Loop;
                        break;
                    case "afterPNext":
                        afterPNext.IsChecked = true;
                        settings.AfterPlayback = XMLSettings.AfterP.Next;
                        break;
                    case "afterPNothing":
                        afterPNothing.IsChecked = true;
                        settings.AfterPlayback = XMLSettings.AfterP.Nothing;
                        break;
                }

                afterPPreventLoop = false;
                if (!isSettingsChanged && isInitResize) isSettingsChanged = true;
            }
        }
        #endregion


        #region setAfterPChecked
        private void setAfterPChecked()
        {
            if (firstAfterPCheck)
            {
                switch (settings.AfterPlayback)
                {
                    case XMLSettings.AfterP.Loop:
                        afterPLoop.IsChecked = true;
                        break;
                    case XMLSettings.AfterP.Next:
                        afterPNext.IsChecked = true;
                        break;
                    case XMLSettings.AfterP.Nothing:
                        afterPNothing.IsChecked = true;
                        break;
                }

                firstAfterPCheck = false;
            }
        }
        #endregion


        private void cleanAfterPChecked()
        {
            afterPLoop.IsChecked = false;
            afterPNothing.IsChecked = false;
            afterPNext.IsChecked = false;
        }
        #endregion



        #region OrderBy checkable menu
        private void orderBy_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;

            if (!orderBPreventLoop && !firstOrderBCheck)
            {
                cleanOrderBChecked();
                orderBPreventLoop = true;

                switch (obj.Name)
                {
                    case "orderBName":
                        orderBName.IsChecked = true;
                        orderBName2.IsChecked = true;
                        settings.OrderBy = XMLSettings.OrderB.Name;
                        break;
                    case "orderBDate":
                        orderBDate.IsChecked = true;
                        orderBDate2.IsChecked = true;
                        settings.OrderBy = XMLSettings.OrderB.Date;
                        break;
                    case "orderBName2":
                        orderBName.IsChecked = true;
                        orderBName2.IsChecked = true;
                        settings.OrderBy = XMLSettings.OrderB.Name;
                        break;
                    case "orderBDate2":
                        orderBDate.IsChecked = true;
                        orderBDate2.IsChecked = true;
                        settings.OrderBy = XMLSettings.OrderB.Date;
                        break;
                }

                orderBPreventLoop = false;
                if (!isSettingsChanged && isInitResize) isSettingsChanged = true;
                getFolderMedia();
                showTitleControls();
                hideTitleControls();
            }
        }


        private void cleanOrderBChecked()
        {
            orderBName.IsChecked = false;
            orderBDate.IsChecked = false;
            orderBName2.IsChecked = false;
            orderBDate2.IsChecked = false;
        }


        private void orderB_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            setOrderBChecked();
        }


        #region setOrderBChecked
        private void setOrderBChecked()
        {
            if (firstOrderBCheck)
            {
                switch (settings.OrderBy)
                {
                    case XMLSettings.OrderB.Name:
                        orderBName.IsChecked = true;
                        orderBName2.IsChecked = true;
                        break;
                    case XMLSettings.OrderB.Date:
                        orderBDate.IsChecked = true;
                        orderBDate2.IsChecked = true;
                        break;
                }

                firstOrderBCheck = false;
            }
        }
        #endregion
        #endregion


        #region Aspect ratio checkables
        private void keepRatio_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;

            if (!firstRatioCheck)
            {
                if (obj.IsChecked)
                {
                    settings.KeepRatio = false;
                }
                else
                {
                    settings.KeepRatio = true;
                }

                if (!isSettingsChanged && isInitResize) isSettingsChanged = true;
            }
        }


        private void more_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            setRatioChecked();
        }


        #region setRatioChecked
        private void setRatioChecked()
        {
            if (firstRatioCheck)
            {
                if (settings.KeepRatio)
                {
                    keepRatio.IsChecked = true;
                    keepRatio2.IsChecked = true;
                }

                firstRatioCheck = false;
            }
        }
        #endregion 
        #endregion
        #endregion


        #region Grip resize window
        private void gripGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            showGrip();
        }


        private void gripGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            hideGrip();
        }


        void grip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture(this.grip);
                startPt = e.GetPosition(this.grip);
                isResizing = true;
            }
        }


        void grip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isResizing)
            {
                isResizing = false;
                this.grip.ReleaseMouseCapture();
            }
        }

        private void showGrip()
        {
            grip.Visibility = Visibility.Visible;
        }


        private void hideGrip()
        {
            grip.Visibility = Visibility.Collapsed;
        }


        private void mainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                newPt = e.GetPosition(this.grip);
                reWidth = this.Width + newPt.X - startPt.X;

                if (settings.KeepRatio)
                {
                    reheight = (oriMediaRatio.Y / oriMediaRatio.X) * reWidth;

                    if (reWidth <= SystemParameters.PrimaryScreenWidth && reheight <= SystemParameters.PrimaryScreenHeight && reWidth > 0)
                    {
                        if (reWidth >= MinWidth && reheight >= MinHeight)
                        {
                            this.Width = reWidth;
                            this.Height = (oriMediaRatio.Y / oriMediaRatio.X) * reWidth;//rule of three to convert the mouse left click Y to an equivalent on the image    
                        }
                    }
                }
                else
                {
                    reheight = this.Height + newPt.Y - startPt.Y;

                    if (reWidth > 0)
                    {
                        this.Width = reWidth;
                    }

                    if (reheight > 0)
                    {
                        this.Height = reheight;
                    }
                }
            }

            //#if DEBUG
            //popDebug(); 
            //#endif
        }
        #endregion


        #region Old stuff
        #region Media current position popup
        double cPos = 0;//postion of the mouse over the video's actual lenght
        double spanTSec = 0;//total lenght of the video in seconds
        double seekBW = 0;//seekBar actual width
        double tempS = 0;//temporary double
        short tSecond = 0;//total in seconds
        short tMinute = 0;//total in minutes
        short tHour = 0;//total in hours

        Point mCPos = new Point();//current postion of the mouse over the seekBar
        DispatcherTimer timerSeek = new DispatcherTimer();
        bool isGetpos = false;

        private void seekBar_MouseEnter(object sender, MouseEventArgs e)
        {
            spanTSec = mainMedia.NaturalDuration.TimeSpan.TotalSeconds;
            seekBW = seekBar.ActualWidth - 10;
            isGetpos = true;
            mCPos = e.GetPosition(seekBar);
            fTT.HorizontalOffset = (mCPos.X < seekBW / 2) ? fTT.HorizontalOffset = mCPos.X : fTT.HorizontalOffset = mCPos.X - 50;
            timerSeek.Start();
            fTT.IsOpen = true;//display the popup
        }


        private void seekBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isGetpos) mCPos = e.GetPosition(seekBar);
        }


        private void updatePopup(object sender, EventArgs e)
        {
            isGetpos = (isGetpos) ? isGetpos = false : isGetpos = true;
            fTT.HorizontalOffset = (mCPos.X < seekBW / 2) ? fTT.HorizontalOffset = mCPos.X : fTT.HorizontalOffset = mCPos.X - 50;

            cPos = spanTSec * (mCPos.X - 5) / seekBW;//conversion to get the current position in the video based on the current mouse postion over the seek bar
            tempS = cPos / 60;
            tHour = (short)(tempS / 60);
            tSecond = (short)(((tempS - Math.Truncate(tempS)) * 60));
            tempS = tempS / 60;
            tMinute = (short)(((tempS - Math.Truncate(tempS)) * 60));

            fText.Text = tHour.ToString("D2") + ":" + tMinute.ToString("D2") + ":" + tSecond.ToString("D2");
        }


        private void seekBar_MouseLeave(object sender, MouseEventArgs e)
        {
            fTT.IsOpen = false;
            timerSeek.Stop();
        }
        #endregion


        #region Crop methods
        #region Draw rectangle
        private void drawRec()
        {
            rectangle.Width = Math.Abs(clickDown.X - currentPos.X);
            rectangle.Height = Math.Abs(clickDown.Y - currentPos.Y);

            cleanCanvas();
            imageCanvas.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, Math.Min(clickDown.X, currentPos.X));
            Canvas.SetTop(rectangle, Math.Min(clickDown.Y, currentPos.Y));
        }
        #endregion


        #region Clean canvas
        private void cleanCanvas()
        {
            if (imageCanvas.Children.Count == 4)
            {
                imageCanvas.Children.Remove(line);
                imageCanvas.Children.Remove(line2);
                imageCanvas.Children.Remove(line3);
                imageCanvas.Children.Remove(line4);
            }
            else if (imageCanvas.Children.Count == 1)
            {
                imageCanvas.Children.Remove(rectangle);
            }
            else if (imageCanvas.Children.Count == 2)
            {
                imageCanvas.Children.Remove(line);
                imageCanvas.Children.Remove(line3);
            }
        }
        #endregion


        #region Draw lines
        private void drawLine()
        {
            line.X1 = currentPos.X;
            line.Y1 = lineHeight;
            line.X2 = currentPos.X;//start of the line X

            line2.X1 = currentPos.X - 1;
            line2.Y1 = lineHeight;
            line2.X2 = currentPos.X - 1;//start of the line X

            line3.X1 = lineWidth;
            line3.Y1 = currentPos.Y;
            line3.Y2 = currentPos.Y;//start of the line Y

            line4.X1 = lineWidth;
            line4.Y1 = currentPos.Y - 1;
            line4.Y2 = currentPos.Y - 1;//start of the line Y
        }
        #endregion


        #region drawFirstLine
        private void drawFirstLine()
        {
            currentPos = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            Point mainWindowPos = new Point(mainWindow.Left, mainWindow.Top);
            Point mainMediaGridDimensions = new Point(mainMediaGrid.ActualWidth, mainMediaGrid.ActualHeight);
            Point mainImageDimensions = new Point(mainImage.ActualWidth, mainImage.ActualHeight);

            currentPos = CropImage.GetClickPosOverMedia(currentPos, mainMediaGridDimensions, mainImageDimensions, mainWindowPos, WindowState);

            line.X1 = currentPos.X;
            line.Y1 = lineHeight;
            line.X2 = currentPos.X;//start of the line X
            line.Y2 = 0;

            line2.X1 = currentPos.X - 1;
            line2.Y1 = lineHeight;
            line2.X2 = currentPos.X - 1;//start of the line X
            line2.Y2 = 0;

            line3.X1 = lineWidth;
            line3.Y1 = currentPos.Y;
            line3.X2 = 0;//start of the line X
            line3.Y2 = currentPos.Y;//start of the line Y

            line4.X1 = lineWidth;
            line4.Y1 = currentPos.Y - 1;
            line4.X2 = 0;//start of the line X
            line4.Y2 = currentPos.Y - 1;//start of the line Y

            cleanCanvas();

            imageCanvas.Children.Add(line);
            imageCanvas.Children.Add(line2);
            imageCanvas.Children.Add(line3);
            imageCanvas.Children.Add(line4);
        }
        #endregion
        #endregion


        #region Save snapshot
        private void snapshot_Click(object sender, RoutedEventArgs e)
        {
            //_mediaPlayer = new MediaPlayer();
            //_mediaPlayer.MediaOpened += new EventHandler(mediaOpened);
            //_mediaPlayer.ScrubbingEnabled = true;
            //_mediaPlayer.Open(new Uri(defaultMedia));

            Thumbnail tb = new Thumbnail(defaultMedia, mainMedia.Position);
            tb.ShowDialog();
        }

        private void mediaOpened(object sender, EventArgs e)
        {
            _mediaPlayer.Position = mainMedia.Position;
            framePixels = new uint[_mediaPlayer.NaturalVideoWidth * _mediaPlayer.NaturalVideoHeight];
            previousFramePixels = new uint[framePixels.Length];

            try
            {
                System.Windows.Forms.SaveFileDialog saveFile = new System.Windows.Forms.SaveFileDialog();
                saveFile.FileName = System.IO.Path.GetFileNameWithoutExtension(defaultMedia);
                saveFile.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|JPEG (*.jpeg)|*.jpeg";
                BitmapEncoder encoder = new PngBitmapEncoder();

                if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string ext = System.IO.Path.GetExtension(saveFile.FileName);
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

                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)RenderBitmapAndCapturePixels(previousFramePixels)));
                    using (var fileStream = new FileStream(saveFile.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            _mediaPlayer = null;
            _mediaPlayer = new MediaPlayer();
        }


        private ImageSource RenderBitmapAndCapturePixels(uint[] pixels)
        {
            // Render the current frame into a bitmap
            var drawingVisual = new DrawingVisual();
            var renderTargetBitmap = new RenderTargetBitmap(_mediaPlayer.NaturalVideoWidth, _mediaPlayer.NaturalVideoHeight, 96, 96, PixelFormats.Default);
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawVideo(_mediaPlayer, new Rect(0, 0, _mediaPlayer.NaturalVideoWidth, _mediaPlayer.NaturalVideoHeight));
            }
            renderTargetBitmap.Render(drawingVisual);

            // Copy the pixels to the specified location
            renderTargetBitmap.CopyPixels(pixels, _mediaPlayer.NaturalVideoWidth * 4, 0);

            // Return the bitmap
            return renderTargetBitmap;
        }
        #endregion
        #endregion


        #region Delete file
        private void deleteFile_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            deleteFile();
        }


        void deleteFile()
        {
            if (MessageBox.Show("Are you sure you want to delete this file?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                lblFileName.Content = null;
                fileName = null;

                if (mediaType == MediaType.Video || mediaExtension == MediaExtension.GIF || mediaType == MediaType.Audio)
                {
                    this.IsHitTestVisible = false;
                    navigationGrid.IsHitTestVisible = false;
                    navigationGridWhite.Visibility = Visibility.Collapsed;
                    if (isMediaPlaying && mediaType == MediaType.Image) loading.StopAnimation();
                    TaskbarItemInfo = null;
                    isMediaPlaying = false;
                    timer.Stop();
                    mainMedia.Stop();
                    mainMedia.Source = null;
                    mainMedia.Visibility = Visibility.Collapsed;

                    DispatcherTimer deleteTimer = new DispatcherTimer();
                    deleteTimer.Interval = TimeSpan.FromMilliseconds(100);
                    deleteTimer.Tick += new EventHandler(deleteTimer_Tick);
                    deleteTimer.Start();
                }
                else
                {
                    if (mainImage.Source != null) mainImage.Source = null;
                    seekBar.Value = 0;
                    lblseekerValue.Content = null;
                    lblmediaTimeSpan.Content = null;

                    File.Delete(defaultMedia);
                    removeFromList();
                }
            }
        }


        void removeFromList ()
        {
            if (defaultImageIndex < previousImageIndex)
            {
                previousImageIndex--;
                listOfFiles.RemoveAt(defaultImageIndex);
            }
            else if (defaultImageIndex == previousImageIndex)
            {
                listOfFiles.RemoveAt(defaultImageIndex);
                if (previousImageIndex > 0) previousImageIndex--;
            }
            else
            {
                listOfFiles.RemoveAt(defaultImageIndex);
            }

            if (listOfFiles.Count > 0)
            {
                defaultImageIndex = previousImageIndex;

                defaultMedia = System.IO.Path.GetDirectoryName(defaultMedia) + "\\" + listOfFiles[defaultImageIndex];
                setMainMedia();
            }
            else
            {
                defaultMedia = null;
                this.Title = "WPFImageViewer";
            }
        }


        void deleteTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer tempT = sender as DispatcherTimer;
            tempT.Stop();

            try
            {
                File.Delete(defaultMedia);
                this.IsHitTestVisible = true;
                removeFromList();
            }
            catch (IOException)
            {
                tempT.Start();
            }
        } 
        #endregion
    }
}
