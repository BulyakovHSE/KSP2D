using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KSP2D.UWP.Styles
{
    public class DoubleToLeftMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double val)
            {
                return new Thickness(val, 0, 0, 0);
            }
            return new Thickness(0,0,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Thickness margin)
            {
                return margin.Left;
            }

            return 0;
        }
    }
}