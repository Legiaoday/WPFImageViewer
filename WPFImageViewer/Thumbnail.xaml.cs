#region References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Threading;
#endregion

namespace WPFImageViewer
{
    public partial class Thumbnail : Window
    {
        #region Global variables/objects
        // Member variables
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private Queue<TimeSpan> _positionsToThumbnail = new Queue<TimeSpan>();
        private DispatcherTimer _watchdogTimer = new DispatcherTimer();
        private uint[] framePixels;
        private uint[] previousFramePixels;
        TimeSpan currentPos;
        List<string> listOfIntervals = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        int listIndex = 4;
        List<string> listOfIntervals2 = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        int listIndex2 = 4;
        #endregion

        #region Dependencies
        // Inheritable attached dependency property indicates whether processing is going on
        public static bool GetProcessing(DependencyObject obj)
        {
            return (bool)obj.GetValue(ProcessingProperty);
        }
        public static void SetProcessing(DependencyObject obj, bool value)
        {
            obj.SetValue(ProcessingProperty, value);
        }
        public static readonly DependencyProperty ProcessingProperty = DependencyProperty.RegisterAttached(
            "Processing", typeof(bool), typeof(Thumbnail), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        // Dependency property indicates the file name of the current media
        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(Thumbnail)); 
        #endregion


        public Thumbnail(string defaultMedia, TimeSpan span)
        {
            InitializeComponent();
            _mediaPlayer.MediaOpened += new EventHandler(HandleMediaPlayerMediaOpened);
            _mediaPlayer.Changed += new EventHandler(HandleMediaPlayerChanged);
            _watchdogTimer.Interval = TimeSpan.FromMilliseconds(250);
            _watchdogTimer.Tick += new EventHandler(HandleWatchdogTimerTick);
            currentPos = span;

            FileName = defaultMedia;
            SetProcessing(this, true);
            _mediaPlayer.ScrubbingEnabled = true;
            _mediaPlayer.Open(new Uri(defaultMedia));
        }

        #region HandleMediaPlayerMediaOpened
        private void HandleMediaPlayerMediaOpened(object sender, EventArgs e)
        {
            // Get details about opened file
            var numberFramesToThumbnail = (ThumbnailPanel.Columns * ThumbnailPanel.Rows);
            framePixels = new uint[_mediaPlayer.NaturalVideoWidth * _mediaPlayer.NaturalVideoHeight];
            previousFramePixels = new uint[framePixels.Length];
            int iSub = (numberFramesToThumbnail / 2) * -1;

            // Enqueue a position for each frame (at the center of each of the N segments)
            for (int i = 0; i < numberFramesToThumbnail; i++)
            {
                _positionsToThumbnail.Enqueue(TimeSpan.FromMilliseconds(currentPos.TotalMilliseconds - (iSub * 10)));
                iSub++;
            }

            // Capture the first frame as a baseline
            RenderBitmapAndCapturePixels(previousFramePixels);

            // Seek to the first thumbnail position
            SeekToNextThumbnailPosition();
        } 
        #endregion


        #region SeekToNextThumbnailPosition
        private void SeekToNextThumbnailPosition()
        {
            // If more frames remain to capture...
            if (0 < _positionsToThumbnail.Count)
            {
                // Seek to next position and start watchdog timer
                _mediaPlayer.Position = _positionsToThumbnail.Dequeue();
                _watchdogTimer.Start();
            }
            else
            {
                // Done; close media file and stop processing
                _mediaPlayer.Close();
                framePixels = null;
                previousFramePixels = null;
                SetProcessing(this, false);
            }
        } 
        #endregion


        #region HandleMediaPlayerChanged
        private void HandleMediaPlayerChanged(object sender, EventArgs e)
        {
            // If still processing the file (i.e., not done)...
            if (GetProcessing(this))
            {
                // Capture the current frame
                CaptureCurrentFrame(false);
            }
        } 
        #endregion


        #region saveSnapshot_Click
        private void saveSnapshot_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mI = sender as MenuItem;
            if (mI != null)
            {
                ContextMenu cM = mI.Parent as ContextMenu;
                if (cM != null)
                {
                    Image img = cM.PlacementTarget as Image;
                    if (img != null)
                    {
                        saveSnap(img.Source);
                    }
                }
            }
        } 
        #endregion


        #region saveSnap
        private void saveSnap(ImageSource source)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog saveFile = new System.Windows.Forms.SaveFileDialog();
                saveFile.FileName = Path.GetFileNameWithoutExtension(FileName);
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

                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)source));
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
        } 
        #endregion


        #region CaptureCurrentFrame
        private void CaptureCurrentFrame(bool forceCapture)
        {
            try
            {
                // Capture the current frame as an ImageSource
                var imageSource = RenderBitmapAndCapturePixels(framePixels);

                // If captured pixels are different than the previous frame...
                if (forceCapture || !framePixels.SequenceEqual(previousFramePixels))
                {
                    // Stop the watchdog timer
                    _watchdogTimer.Stop();

                    ContextMenu cT = new ContextMenu();
                    MenuItem mT = new MenuItem();
                    mT.Header = "Save snapshot...";
                    mT.Click += new RoutedEventHandler(saveSnapshot_Click);
                    cT.Items.Add(mT);

                    // Add an Image for the Thumbnail
                    ThumbnailPanel.Children.Add(
                        new Image
                        {
                            ContextMenu = cT,
                            Source = imageSource,
                            ToolTip = _mediaPlayer.Position,
                            MaxWidth = _mediaPlayer.NaturalVideoWidth,
                            MaxHeight = _mediaPlayer.NaturalVideoHeight,
                            Margin = new Thickness(1)
                        });

                    // Swap the pixel buffers (moves current to previous and avoids allocating a new buffer for current)
                    var tempPixels = framePixels;
                    framePixels = previousFramePixels;
                    previousFramePixels = tempPixels;

                    // Seek to the next thumbnail position
                    SeekToNextThumbnailPosition();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                _watchdogTimer.Stop();
                Thread.Sleep(500);
            }
            catch (Exception)
            {
                throw;
            }
        } 
        #endregion


        #region HandleWatchdogTimerTick
        private void HandleWatchdogTimerTick(object sender, EventArgs e)
        {
            // Stop the watchdog timer
            _watchdogTimer.Stop();

            // Capture the current frame (even if it's not different than the previous)
            CaptureCurrentFrame(true);
        } 
        #endregion


        #region RenderBitmapAndCapturePixels
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


        #region Set rows and columns
        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            if (listIndex < listOfIntervals.Count - 1)
            {
                listIndex++;
                textBox.Text = listOfIntervals[listIndex];
            }
        }


        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            if (listIndex > 0)
            {
                listIndex--;
                textBox.Text = listOfIntervals[listIndex];
            }
        }

        private void upButton_Click2(object sender, RoutedEventArgs e)
        {
            if (listIndex2 < listOfIntervals2.Count - 1)
            {
                listIndex2++;
                textBox2.Text = listOfIntervals2[listIndex2];
            }
        }


        private void downButton_Click2(object sender, RoutedEventArgs e)
        {
            if (listIndex2 > 0)
            {
                listIndex2--;
                textBox2.Text = listOfIntervals2[listIndex2];
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _watchdogTimer.Stop();
            ThumbnailPanel.Columns = Int32.Parse(textBox.Text);
            ThumbnailPanel.Rows = Int32.Parse(textBox2.Text);

            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.MediaOpened += new EventHandler(HandleMediaPlayerMediaOpened);
            _mediaPlayer.Changed += new EventHandler(HandleMediaPlayerChanged);
            _watchdogTimer.Interval = TimeSpan.FromMilliseconds(100);
            _watchdogTimer.Tick += new EventHandler(HandleWatchdogTimerTick);
            _mediaPlayer.ScrubbingEnabled = true;

            SetProcessing(this, true);
            ThumbnailPanel.Children.Clear();
            _mediaPlayer.Open(new Uri(FileName));
        }
        #endregion
    }
}
