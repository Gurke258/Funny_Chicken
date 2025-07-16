using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lustige_Huehnchen
{
    public class HighscoreEntry
    {
        public int Score { get; set; }
        public int Rank { get; set; }
        public string? Name { get; set; }
    }


    [ValueConversion(typeof(int), typeof(Brush))]
    public class RankToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rank)
            {
                return rank switch
                {
                    1 => Brushes.Gold,
                    2 => Brushes.Silver,
                    3 => Brushes.Peru, // Bronze-Farbe
                    _ => Brushes.White
                };
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
