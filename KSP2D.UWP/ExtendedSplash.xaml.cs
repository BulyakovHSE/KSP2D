using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
using UWPMVVMLib;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KSP2D.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtendedSplash : Page
    {
        private object _lockThis = new object();
        public Rect SplashImageRect;
        private SplashScreen _splashScreen;
        private Rect _sceneSizeRect;
        private double[] _images;
        private double[] _rotateSpeeds; // degrees per 0.01 second
        private double[] _angles;
        private double[] _ellipses;
        public double ScaleFactor;
        private SynchronizationContext _synchronizationContext;
        private TaskScheduler _taskScheduler;
        private Task[] _tasks;
        private CancellationTokenSource[] _cancellationTokenSources;
        private Thread[] _threads;
        private ThreadPriority[] _threadPriorities;
        public ExtendedSplash(SplashScreen splashScreen)
        {
            this.InitializeComponent();
            InitSizes();
            InitRotaging();
            InitThreading();
            DataContext = new ExtendedSplashViewModel();
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

        private void InitThreading()
        {
            _tasks = new Task[7];
            _cancellationTokenSources = new CancellationTokenSource[7];
            _threads = new Thread[7];
            _threadPriorities = new ThreadPriority[7];
            _synchronizationContext = SynchronizationContext.Current;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private async void SplashScreenOnDismissed(SplashScreen sender, object args)
        {
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RotatePlanets);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DismissExtendedSplash);
        }

        private async void RotatePlanets(CancellationToken ct, int i)
        {
            while (!ct.IsCancellationRequested)
            {
                lock (_lockThis)
                {
                    var sin = Math.Sin(_angles[i]);
                    var cos = Math.Cos(_angles[i]);
                    double ellipseWidth;
                    var tas = Task<double>.Factory.StartNew(() => GetEllipseWidth(i), ct, TaskCreationOptions.None, _taskScheduler);
                    try
                    {
                        ellipseWidth = tas.Result;
                    }
                    catch
                    {
                        return;
                    }
                    var left = ellipseWidth * cos;
                    var top = ellipseWidth * sin;
                    var margin = new Thickness(left, top, 0, 0);
                    Task.Factory.StartNew(() => { SetMargin(i, margin); }, ct, TaskCreationOptions.None, _taskScheduler);
                    _angles[i] += _rotateSpeeds[i];
                    if (Math.Abs(_angles[i] - 360) < 0.00001) _angles[i] = 0;
                }

                await Task.Delay(new TimeSpan(0, 0, 0, 0, 10));
            }
        }

        private void SetMargin(int index, Thickness margin)
        {
            switch (index)
            {
                case 0:
                    MohoImage.Margin = margin;
                    break;
                case 1:
                    EveImage.Margin = margin;
                    break;
                case 2:
                    KerbinImage.Margin = margin;
                    break;
                case 3:
                    DunaImage.Margin = margin;
                    break;
                case 4:
                    DressImage.Margin = margin;
                    break;
                case 5:
                    DjulImage.Margin = margin;
                    break;
                case 6:
                    ElooImage.Margin = margin;
                    break;
            }
        }

        private double GetEllipseWidth(int index)
        {
            switch (index)
            {
                case 0:
                    return MohoEllipse.Width;
                case 1:
                    return EveEllipse.Width;
                case 2:
                    return KerbinEllipse.Width;
                case 3:
                    return DunaEllipse.Width;
                case 4:
                    return DressEllipse.Width;
                case 5:
                    return DjulEllipse.Width;
                case 6:
                    return ElooEllipse.Width;
                default:
                    return 1;
            }
        }

        private async void RotateMoho(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                lock (_lockThis)
                {
                    var sin = Math.Sin(_angles[0]);
                    var cos = Math.Cos(_angles[0]);
                    double ellipseWidth;
                    ellipseWidth = MohoEllipse.Width;
                    var left = ellipseWidth * cos;
                    var top = ellipseWidth * sin;
                    var margin = new Thickness(left, top, 0, 0);
                    MohoImage.Margin = margin;
                    _angles[0] += _rotateSpeeds[0];
                    if (Math.Abs(_angles[0] - 360) < 0.00001) _angles[0] = 0;
                }

                await Task.Delay(new TimeSpan(0, 0, 0, 0, 10));
            }
        }

        private async void DismissExtendedSplash()
        {
            //LoadingStatusTxt.Text = "Загрузка...";
            while (true)
            {
                LoadingStatusTxt.Text = DateTime.Now.ToString("F");
                await Task.Delay(new TimeSpan(0, 0, 0, 1));
            }
            //Frame rootFrame = new Frame();
            //rootFrame.Navigate(typeof(MainPage));
            //Window.Current.Content = rootFrame;
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

        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch t)
                if (t.Parent is StackPanel s)
                    if (s.Parent is StackPanel ss)
                    {
                        var index = GetPlanetIndexFromName(ss.Name);
                        if (index == -1) return;

                        if (t.IsOn)
                        {
                            _cancellationTokenSources[index] = new CancellationTokenSource();
                            _threads[index] = new Thread(() =>
                            {
                                RotatePlanets(_cancellationTokenSources[index].Token
                                    , index);
                            });
                            _threads[index].Priority = _threadPriorities[index];

                            _threads[index].Start();
                        }
                        else
                            _cancellationTokenSources[index]?.Cancel();
                    }
        }

        private int GetPlanetIndexFromName(string planetName)
        {
            switch (planetName)
            {
                case "Moho": return 0;
                case "Eve": return 1;
                case "Kerbin": return 2;
                case "Duna": return 3;
                case "Dress": return 4;
                case "Djul": return 5;
                case "Eloo": return 6;
            }

            return -1;
        }

        private void RangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_rotateSpeeds == null) return;
            if (sender is Slider t)
                if (t.Parent is StackPanel s)
                {
                    int index = GetPlanetIndexFromName(s.Name);
                    _rotateSpeeds[index] = e.NewValue / 10000;
                }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox c)
                if (c.Parent is StackPanel s)
                    if (s.Parent is StackPanel ss)
                    {
                        var index = GetPlanetIndexFromName(ss.Name);
                        if (index == -1) return;
                        if (c.SelectedItem != null)
                        {
                            var priority = (ThreadPriority) c.SelectedItem;
                            _threadPriorities[index] = priority;
                        }
                    }
        }
    }

    public class ExtendedSplashViewModel : ViewModelBase
    {
        private ObservableCollection<ThreadPriority> _priorities;

        public ExtendedSplashViewModel()
        {
            Priorities = new ObservableCollection<ThreadPriority>(Enum.GetValues(typeof(ThreadPriority)).OfType<ThreadPriority>());
        }

        public ObservableCollection<ThreadPriority> Priorities
        {
            get => _priorities;
            set
            {
                _priorities = value; 
                OnPropertyChanged();
            }
        }
    }
}
