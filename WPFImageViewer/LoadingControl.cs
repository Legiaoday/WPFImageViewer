using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFImageViewer
{
    class LoadingControl
    {
        public double Left
        {
            get
            {
                return ClientBounds.Margin.Left;
            }
            set
            {
                Thickness thick =ClientBounds.Margin;
                thick.Left = value;
                ClientBounds.Margin = thick;
            }
        }
        public double Top
        {
            get
            {
                return ClientBounds.Margin.Top;
            }
            set
            {
                Thickness thick =ClientBounds.Margin;
                thick.Top = value;
                ClientBounds.Margin = thick;
            }
        }
        public double Right
        {
            get
            {
                return ClientBounds.Margin.Right;
            }
            set
            {
                Thickness thick = ClientBounds.Margin;
                thick.Right = value;
                ClientBounds.Margin = thick;
            }
        }
        public double Bottom
        {
            get
            {
                return ClientBounds.Margin.Bottom;
            }
            set
            {
                Thickness thick = ClientBounds.Margin;
                thick.Bottom = value;
                ClientBounds.Margin = thick;
            }
        }
        public double Width
        {
            get
            {
                return ClientBounds.Width;
            }
        }
        public double Height
        {
            get
            {
                return ClientBounds.Height;
            }
        }
        public Grid ClientBounds;
        private Rectangle square1;
        private Rectangle square2;
        private Rectangle square3;
        private Rectangle square4;
        private Rectangle square5;
        private Thread loadingThread;
        private DispatcherTimer timer;
        private double opacitySub1 = 20;
        private double opacitySub2 = 40;
        private double opacitySub3 = 60;
        private double opacitySub4 = 80;
        private double opacitySub5 = 100;

        public LoadingControl()
        {
            initObjects();
        }


        private void initObjects ()
        {
            ClientBounds = new Grid();
            ClientBounds.Background = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString("#FF171717"), Opacity = 1 };
            ClientBounds.Width = 128;
            ClientBounds.Height = 32;
            ClientBounds.HorizontalAlignment = HorizontalAlignment.Center;
            ClientBounds.VerticalAlignment = VerticalAlignment.Center;
            ClientBounds.IsHitTestVisible = false;
            ClientBounds.Visibility = Visibility.Collapsed;

            square1 = new Rectangle();
            square1.Width = 16;
            square1.Height = 16;
            square1.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.2 };
            square1.HorizontalAlignment = HorizontalAlignment.Left;
            square1.VerticalAlignment = VerticalAlignment.Top;
            square1.Margin = new Thickness(8,8,0,0);

            square2 = new Rectangle();
            square2.Width = 16;
            square2.Height = 16;
            square2.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.4 };
            square2.HorizontalAlignment = HorizontalAlignment.Left;
            square2.VerticalAlignment = VerticalAlignment.Top;
            square2.Margin = new Thickness(32, 8, 0, 0);

            square3 = new Rectangle();
            square3.Width = 16;
            square3.Height = 16;
            square3.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.6 };
            square3.HorizontalAlignment = HorizontalAlignment.Left;
            square3.VerticalAlignment = VerticalAlignment.Top;
            square3.Margin = new Thickness(56, 8, 0, 0);

            square4 = new Rectangle();
            square4.Width = 16;
            square4.Height = 16;
            square4.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.8 };
            square4.HorizontalAlignment = HorizontalAlignment.Left;
            square4.VerticalAlignment = VerticalAlignment.Top;
            square4.Margin = new Thickness(80, 8, 0, 0);

            square5 = new Rectangle();
            square5.Width = 16;
            square5.Height = 16;
            square5.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 1 };
            square5.HorizontalAlignment = HorizontalAlignment.Left;
            square5.VerticalAlignment = VerticalAlignment.Top;
            square5.Margin = new Thickness(104, 8, 0, 0);

            ClientBounds.Children.Add(square1);
            ClientBounds.Children.Add(square2);
            ClientBounds.Children.Add(square3);
            ClientBounds.Children.Add(square4);
            ClientBounds.Children.Add(square5);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += new EventHandler(timer_Tick);
        }


        public void PlayAnimation ()
        {
            loadingThread = new Thread(loadingThreadWork);
            loadingThread.IsBackground = true;
            loadingThread.Start();
        }


        public void StopAnimation()
        {
            ClientBounds.Visibility = Visibility.Collapsed;
            timer.Stop();
            square1.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.2 };
            square2.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.4 };
            square3.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.6 };
            square4.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 0.8 };
            square5.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = 1 };
            opacitySub1 = 20;
            opacitySub2 = 40;
            opacitySub3 = 60;
            opacitySub4 = 80;
            opacitySub5 = 100;
        }


        private void loadingThreadWork()
        {
            Application.Current.Dispatcher.Invoke(() => ClientBounds.Visibility = Visibility.Visible);
            timer.Start();
        }

      
        void timer_Tick(object sender, EventArgs e)
        {
            square1.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = opacitySub1 / 100 };
            square2.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = opacitySub2 / 100 };
            square3.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = opacitySub3 / 100 };
            square4.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = opacitySub4 / 100 };
            square5.Fill = new SolidColorBrush() { Color = Colors.AliceBlue, Opacity = opacitySub5 / 100 };

            opacitySub1 = (opacitySub1 > 10) ? opacitySub1 -= 10 : opacitySub1 = 100;
            opacitySub2 = (opacitySub2 > 10) ? opacitySub2 -= 10 : opacitySub2 = 100;
            opacitySub3 = (opacitySub3 > 10) ? opacitySub3 -= 10 : opacitySub3 = 100;
            opacitySub4 = (opacitySub4 > 10) ? opacitySub4 -= 10 : opacitySub4 = 100;
            opacitySub5 = (opacitySub5 > 10) ? opacitySub5 -= 10 : opacitySub5 = 100;
        }
    }
}
