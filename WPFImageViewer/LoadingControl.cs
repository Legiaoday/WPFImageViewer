using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Windows.Threading;

namespace WPFImageViewer
{
    class LoadingControl
    {
        public double Left
        {
            get
            {
                return clientBounds.Margin.Left;
            }
            set
            {
                Thickness thick =clientBounds.Margin;
                thick.Left = value;
                clientBounds.Margin = thick;
            }
        }
        public double Top
        {
            get
            {
                return clientBounds.Margin.Top;
            }
            set
            {
                Thickness thick =clientBounds.Margin;
                thick.Top = value;
                clientBounds.Margin = thick;
            }
        }
        public double Width
        {
            get
            {
                return clientBounds.Width;
            }
        }
        public double Height
        {
            get
            {
                return clientBounds.Height;
            }
        }
        private Grid clientBounds;
        private Rectangle square1;
        private Rectangle square2;
        private Rectangle square3;
        private Rectangle square4;
        private Rectangle square5;


        public LoadingControl()
        {
            initObjects();
        }


        private void initObjects ()
        {
            clientBounds = new Grid();
            clientBounds.Background = new SolidColorBrush() { Color = Colors.Pink, Opacity = 0.50 };
            clientBounds.Width = 128;
            clientBounds.Height = 32;
            clientBounds.HorizontalAlignment = HorizontalAlignment.Left;
            clientBounds.VerticalAlignment = VerticalAlignment.Top;

            square1 = new Rectangle();
            square1.Width = 16;
            square1.Height = 16;
            square1.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = 0.2 };
            square1.HorizontalAlignment = HorizontalAlignment.Left;
            square1.VerticalAlignment = VerticalAlignment.Top;
            square1.Margin = new Thickness(8,8,0,0);

            square2 = new Rectangle();
            square2.Width = 16;
            square2.Height = 16;
            square2.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = 0.4 };
            square2.HorizontalAlignment = HorizontalAlignment.Left;
            square2.VerticalAlignment = VerticalAlignment.Top;
            square2.Margin = new Thickness(32, 8, 0, 0);

            square3 = new Rectangle();
            square3.Width = 16;
            square3.Height = 16;
            square3.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = 0.60 };
            square3.HorizontalAlignment = HorizontalAlignment.Left;
            square3.VerticalAlignment = VerticalAlignment.Top;
            square3.Margin = new Thickness(56, 8, 0, 0);

            square4 = new Rectangle();
            square4.Width = 16;
            square4.Height = 16;
            square4.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = 0.80 };
            square4.HorizontalAlignment = HorizontalAlignment.Left;
            square4.VerticalAlignment = VerticalAlignment.Top;
            square4.Margin = new Thickness(80, 8, 0, 0);

            square5 = new Rectangle();
            square5.Width = 16;
            square5.Height = 16;
            square5.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = 1 };
            square5.HorizontalAlignment = HorizontalAlignment.Left;
            square5.VerticalAlignment = VerticalAlignment.Top;
            square5.Margin = new Thickness(104, 8, 0, 0);

            clientBounds.Children.Add(square1);
            clientBounds.Children.Add(square2);
            clientBounds.Children.Add(square3);
            clientBounds.Children.Add(square4);
            clientBounds.Children.Add(square5);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += new EventHandler(timer_Tick);
        }


        DispatcherTimer timer;
        double opacitySub1 = 20;
        double opacitySub2 = 40;
        double opacitySub3 = 60;
        double opacitySub4 = 80;
        double opacitySub5 = 100;


        public void PlayAnimation ()
        {
            timer.Start();
        }


        void timer_Tick(object sender, EventArgs e)
        {
            square1.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = opacitySub1 / 100 };
            square2.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = opacitySub2 / 100 };
            square3.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = opacitySub3 / 100 };
            square4.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = opacitySub4 / 100 };
            square5.Fill = new SolidColorBrush() { Color = Colors.BlueViolet, Opacity = opacitySub5 / 100 };

            if (opacitySub1 > 10)
            {
                opacitySub1 -= 10;
            }
            else
            {
                opacitySub1 = 100;
            }

            if (opacitySub2 > 10)
            {
                opacitySub2 -= 10;
            }
            else
            {
                opacitySub2 = 100;
            }

            if (opacitySub3 > 10)
            {
                opacitySub3 -= 10;
            }
            else
            {
                opacitySub3 = 100;
            }

            if (opacitySub4 > 10)
            {
                opacitySub4 -= 10;
            }
            else
            {
                opacitySub4 = 100;
            }

            if (opacitySub5 > 10)
            {
                opacitySub5 -= 10;
            }
            else
            {
                opacitySub5 = 100;
            }
        }


        public Grid ControlArea ()
        {
            return clientBounds;
        }
    }
}
