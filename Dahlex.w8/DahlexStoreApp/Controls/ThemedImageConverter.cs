using System;
using System.Windows;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Dahlex.StylingButton
{
    public class ThemedImageConverter : IValueConverter
    {
        private static Dictionary<String, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();
       // private static Visibility currentTheme;
        private static string assetPath = "ms-appx:///Assets/Controls/";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage result = null;
            // Path to the icon image
            string path = assetPath + (string)value;
            // Check if we already cached the image
            if (!imageCache.TryGetValue(path, out result))
            {
                Uri source = new Uri(path, UriKind.Absolute);
                result = new BitmapImage(source);
                imageCache.Add(path, result);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
