using System;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Collections.Generic;
using System.Globalization;

namespace Dahlex.StylingButton
{
    public class ThemedImageConverter : IValueConverter
    {
        private static Dictionary<String, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();
        private static string assetPath = "/Images/Controls/";


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage result = null;
            // Path to the icon image
            string path = assetPath + (string)value;
            // Check if we already cached the image
            if (!imageCache.TryGetValue(path, out result))
            {
                Uri source = new Uri(path, UriKind.Relative);
                result = new BitmapImage(source);
                imageCache.Add(path, result);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
