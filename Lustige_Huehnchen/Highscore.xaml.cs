using System.Collections.ObjectModel;
using System.Windows;


namespace Lustige_Huehnchen
{
    /// <summary>
    /// Interaktionslogik für Highscore.xaml
    /// </summary>
    public partial class Highscore : Window
    {
        public Highscore(ObservableCollection<HighscoreEntry> scores)
        {
            InitializeComponent();
            var sorted = scores
                    .OrderByDescending(s => s.Score)
                    .Take(15)
                    .Select((s, index) => new HighscoreEntry
                    {
                        Name = s.Name,
                        Score = s.Score,
                        Rank = index + 1,
                        Date = s.Date
                    })
                    .ToList();

            HighscoreListBox.ItemsSource = sorted;

        }
        

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide(); // Schließt das Highscore-Fenster
        }
    }
}
