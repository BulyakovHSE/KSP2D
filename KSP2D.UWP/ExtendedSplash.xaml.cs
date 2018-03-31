using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KSP2D.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtendedSplash : Page
    {
        public Rect SplashImageRect;
        private SplashScreen _splashScreen;
        private Rect _sceneSizeRect;
        private double[] _images;
        private double[] _rotateSpeeds; // degrees per 0.01 second
        private double[] _angles;
        private double[] _ellipses;
        public double ScaleFactor;
        public ExtendedSplash(SplashScreen splashScreen)
        {
            this.InitializeComponent();
            InitSizes();
            SizeChanged += ExtendedSplash_SizeChanged;
            _splashScreen = splashScreen;
            if (_splashScreen != null)
            {
                _splashScreen.Dismissed += SplashScreenOnDismissed;
                ScaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                SplashImageRect = splashScreen.ImageLocation;
            }
        }

        private void InitSizes()
        {
            _sceneSizeRect = new Rect(0, 0, 1800, 1400);
            _images = new double[8];
            _images[0] = SunImage.Width / _sceneSizeRect.Width;
            _images[1] = MohoImage.Width / _sceneSizeRect.Width;
            _images[2] = EveImage.Width / _sceneSizeRect.Width;
            _images[3] = KerbinImage.Width / _sceneSizeRect.Width;
            _images[4] = DunaImage.Width / _sceneSizeRect.Width;
            _images[5] = DressImage.Width / _sceneSizeRect.Width;
            _images[6] = DjulImage.Width / _sceneSizeRect.Width;
            _images[7] = ElooImage.Width / _sceneSizeRect.Width;

            _ellipses = new double[7];
            _ellipses[0] = MohoEllipse.Width / _sceneSizeRect.Width;
            _ellipses[1] = EveEllipse.Width / _sceneSizeRect.Width;
            _ellipses[2] = KerbinEllipse.Width / _sceneSizeRect.Width;
            _ellipses[3] = DunaEllipse.Width / _sceneSizeRect.Width;
            _ellipses[4] = DressEllipse.Width / _sceneSizeRect.Width;
            _ellipses[5] = DjulEllipse.Width / _sceneSizeRect.Width;
            _ellipses[6] = ElooEllipse.Width / _sceneSizeRect.Width;
        }

        private void SetSizeImage(Size size)
        {
            SunImage.Width = size.Width * _images[0];
            MohoImage.Width = size.Width * _images[1];
            EveImage.Width = size.Width * _images[2];
            KerbinImage.Width = size.Width * _images[3];
            DunaImage.Width = size.Width * _images[4];
            DressImage.Width = size.Width * _images[5];
            DjulImage.Width = size.Width * _images[6];
            ElooImage.Width = size.Width * _images[7];
        }

        private void SetSizeEllipse(Size size)
        {
            MohoEllipse.Width = _ellipses[0] * size.Width;
            EveEllipse.Width = _ellipses[1] * size.Width;
            KerbinEllipse.Width = _ellipses[2] * size.Width;
            DunaEllipse.Width = _ellipses[3] * size.Width;
            DressEllipse.Width = _ellipses[4] * size.Width;
            DjulEllipse.Width = _ellipses[5] * size.Width;
            ElooEllipse.Width = _ellipses[6] * size.Width;
        }

        private void InitRotaging()
        {
            _rotateSpeeds = new double[7];
            _angles = new double[7];
            for (int i = 0; i < 7; i++)
                _angles[i] = 0;

            _rotateSpeeds[0] = 0.05;
            _rotateSpeeds[1] = 0.03;
            _rotateSpeeds[2] = 0.02;
            _rotateSpeeds[3] = 0.01;
            _rotateSpeeds[4] = 0.008;
            _rotateSpeeds[5] = 0.005;
            _rotateSpeeds[6] = 0.003;
        }

        private async void SplashScreenOnDismissed(SplashScreen sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RotatePlanets);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DismissExtendedSplash);
        }

        private async void RotatePlanets()
        {
            InitRotaging();
            while (true)
            {
                for (int i = 0; i < 7; i++)
                {
                    var sin = Math.Sin(_angles[i]);
                    var cos = Math.Cos(_angles[i]);
                    double ellipseWidth;
                    switch (i)
                    {
                        case 0: ellipseWidth = MohoEllipse.Width;break;
                        case 1: ellipseWidth = EveEllipse.Width; break;
                        case 2: ellipseWidth = KerbinEllipse.Width; break;
                        case 3: ellipseWidth = DunaEllipse.Width; break;
                        case 4: ellipseWidth = DressEllipse.Width; break;
                        case 5: ellipseWidth = DjulEllipse.Width; break;
                        case 6: ellipseWidth = ElooEllipse.Width; break;
                        default: ellipseWidth = 1;
                                break;
                    }
                    var left = ellipseWidth * cos;
                    var top = ellipseWidth * sin;
                    var margin = new Thickness(left, top, 0, 0);
                    switch (i)
                    {
                            case 0: MohoImage.Margin = margin;break;
                        case 1: EveImage.Margin = margin; break;
                        case 2: KerbinImage.Margin = margin; break;
                        case 3: DunaImage.Margin = margin; break;
                        case 4: DressImage.Margin = margin; break;
                        case 5: DjulImage.Margin = margin; break;
                        case 6: ElooImage.Margin = margin; break;
                    }

                    _angles[i] += _rotateSpeeds[i];
                    if (Math.Abs(_angles[i] - 360) < 0.00001) _angles[i] = 0;
                }

                await Task.Delay(new TimeSpan(0, 0, 0, 0, 10));
            }
        }

        private async void DismissExtendedSplash()
        {
            LoadingStatusTxt.Text = "Загрузка...";
            await Task.Delay(new TimeSpan(0, 0, 60));

            Frame rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage));
            Window.Current.Content = rootFrame;
        }

        private void ExtendedSplash_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_splashScreen != null)
            {
                SplashImageRect = _splashScreen.ImageLocation;
                SetSizeImage(e.NewSize);
                SetSizeEllipse(e.NewSize);
            }
        }
    }
}
