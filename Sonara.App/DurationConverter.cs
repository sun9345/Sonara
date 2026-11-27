using System.Globalization; 

namespace Sonara.App 
{
    public class DurationConverter : IValueConverter 
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not TimeSpan duration)
                return "0:00";

            int minutes = (int)duration.TotalMinutes;

            return $"{minutes}:{duration.Seconds:D2}";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}