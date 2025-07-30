using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Lustige_Huehnchen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Button btnChicken = new Button();
        private Image imgChicken = new Image();
        private int _score = 0; // Variable für die Punkte
        private int _timeInSec = 5; // Zeit in Sekunden
        private Random _random = new Random(); // Random als Klassenfeld
        private List<Image> _huehnchenBilder = new List<Image>();  // Bilder persistieren
        public ObservableCollection<HighscoreEntry> Highscores { get; } = new ObservableCollection<HighscoreEntry>();
        private DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        public Highscore highscoreWindow = default!;
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private bool _gameStarted;
        private int _lastMouseSpeed;

        //Const
        public const UInt32 SPI_SETMOUSESPEED = 0x0071;
        public const uint SPI_GETMOUSESPEED = 0x0070;



        public MainWindow()
        {
            InitializeComponent();
            comboGameMode.SelectedIndex = 1;
            _lastMouseSpeed = GetMouseSpeed();
            SetSettings(); // Einstellungen initialisieren
        }

        private void SQLLiteConnection(string query)
        {
            string connectionString = "Data Source=highscore.db;Version=3;";
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();

        }
        

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(
            uint uiAction, uint uiParam, int pvParam, uint fWinIni);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(
            uint uiAction, uint uiParam, out int pvParam, uint fWinIni);

        void SetMouseSpeed(int speed)
        {
            // Speed zwischen 1 (langsam) bis 20 (schnell), Standard 10
            SystemParametersInfo(SPI_SETMOUSESPEED, 0, speed, 0);
        }
        public static int GetMouseSpeed()
        {
            int speed;
            SystemParametersInfo(SPI_GETMOUSESPEED, 0, out speed, 0);
            return speed;  // Wert zwischen 1 (langsam) und 20 (schnell)
        }

        private void SetSettings()
        {
            // Label Settings
            lblPoints.Content = "Punkte: 0"; // Initialisierung der Punkteanzeige
            lblTime.Content = $"Zeit verbleibend: 0 s";

            // Image Settings
            imgChicken.Source = new BitmapImage(new Uri("pack://application:,,,/Images/little_chicken.png"));

            // Button Settings
            btnChicken.Content = imgChicken;
            btnChicken.Background = Brushes.Transparent; // Hintergrund transparent setzen
            btnChicken.BorderThickness = new Thickness(0); // Rahmen entfernen
            btnChicken.Width = 40;
            btnChicken.Height = 40;
            

            // Timer Settings
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

        }

        private void ResetGame()
        {
            _gameStarted = false;
            _score = 0;
            _timeInSec = 5;
            lblPoints.Content = "Punkte: 0";
            lblTime.Content = $"Zeit verbleibend: {_timeInSec} s";
            btnStart.IsEnabled = true;
            comboGameMode.IsEnabled = true;
            SetMouseSpeed(_lastMouseSpeed);
            GameCanvas.Children.Clear(); // Canvas leeren
            _huehnchenBilder.Clear(); // Liste der persistierten Bilder leeren
            _huehnchenBilder.Capacity = 0; // Speicher freigeben
            GameCanvas.Children.Capacity = 0; // Speicher freigeben

        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (comboGameMode.SelectedItem != null)
            {
                _gameStarted = true;
                _dispatcherTimer.Tick -= dispatcherTimer_Tick;

                Canvas.SetLeft(btnChicken, 50); // 50 Pixel vom linken Rand
                Canvas.SetTop(btnChicken, 30);  // 30 Pixel vom oberen Rand

                GameCanvas.Children.Add(btnChicken); // myCanvas ist ein Canvas im XAML

                _dispatcherTimer.Tick += dispatcherTimer_Tick;
                _dispatcherTimer.Start();
                btnChicken.Click += btnChicken_Click;

                btnStart.IsEnabled = false;
                txtPlayer.IsEnabled = false;
                comboGameMode.IsEnabled = false;
                if (comboGameMode.SelectedIndex == 0)
                    btnChicken.Focus();
                if (comboGameMode.SelectedIndex == 2)
                    SetMouseSpeed(3);
                else
                    SetMouseSpeed(_lastMouseSpeed);
            }
            else
                MessageBox.Show("Einen GameMode auswählen","GameMode fehlt!",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        // ...

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            _timeInSec--; // Zeit in Sekunden erhöhen
            lblTime.Content = $"Zeit verbleibend: {_timeInSec} s";
            if (comboGameMode.SelectedIndex == 0)
                btnChicken.Focus();

            if (_timeInSec <= 0)
            {
                _dispatcherTimer.Stop();
                btnChicken.Click -= btnChicken_Click;

                lblTime.Content = "Zeit abgelaufen!";

                // Highscore speichern
                var newEntry = new HighscoreEntry 
                { 
                    Score = _score, 
                    Name = txtPlayer.Text != "" ? txtPlayer.Text : "Unbekannt",
                    Date = DateTime.Now.ToString("g")
                };

                Highscores.Add(newEntry);
                var highscoreWindow = new Highscore(Highscores);
                highscoreWindow.Show();

                //reset Game
                ResetGame();

            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void btnChicken_Click(object sender, RoutedEventArgs e)
        {
            // 1. Punkte aktualisieren
            _score += 10;
            lblPoints.Content = $"Punkte: {_score}";

            Uri path = new Uri("Sounds/fart-short.mp3", UriKind.Relative);
            _mediaPlayer.Open(path);
            _mediaPlayer.Play();

            // 2. Alte Position des Buttons erfassen
            double oldLeft = Canvas.GetLeft(btnChicken);
            double oldTop = Canvas.GetTop(btnChicken);

            // 3. Button-Bild an alter Position erstellen
            CreatePersistentButtonImage(oldLeft, oldTop);

            // 4. Neue Position berechnen
            var (newLeft, newTop) = CalculateNewPosition();

            // 5. Button verschieben
            Canvas.SetLeft(btnChicken, newLeft);
            Canvas.SetTop(btnChicken, newTop);
        }

        private void CreatePersistentButtonImage(double left, double top)
        {
            // 1. Button rendern
            btnChicken.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            btnChicken.Arrange(new Rect(btnChicken.DesiredSize));

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)btnChicken.ActualWidth,
                (int)btnChicken.ActualHeight,
                96,
                96,
                PixelFormats.Pbgra32);

            rtb.Render(btnChicken);

            // 2. Image erstellen
            Image persistedImage = new Image()
            {
                //Source = rtb,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/egg.png")),
                Width = btnChicken.ActualWidth,
                Height = btnChicken.ActualHeight,
                Visibility = Visibility.Visible
            };

            // 3. Positionierung
            Canvas.SetLeft(persistedImage, left);
            Canvas.SetTop(persistedImage, top);

            // 4. Unter den Button legen
            Canvas.SetZIndex(persistedImage, 0);
            Canvas.SetZIndex(btnChicken, 1);

            // 5. Bild persistieren und hinzufügen
            _huehnchenBilder.Add(persistedImage);
            GameCanvas.Children.Add(persistedImage);
        }

        private (double left, double top) CalculateNewPosition()
        {
            PresentationSource source = PresentationSource.FromVisual(GameCanvas);
            Matrix m = source.CompositionTarget.TransformToDevice;

            double pixelMaxWidth = GameCanvas.ActualWidth * m.M11;
            double pixelMaxHeight = GameCanvas.ActualHeight * m.M22;

            int left = _random.Next(0, Math.Max(1, (int)pixelMaxWidth - (int)btnChicken.ActualWidth));
            int top = _random.Next(0, Math.Max(1, (int)pixelMaxHeight - (int)btnChicken.ActualHeight));

            return (left / m.M11, top / m.M22);
        }

        private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_gameStarted)
            {
                var highscoreWindow = new Highscore(Highscores);
                highscoreWindow.Show();
            }
            
 
        }
    }
}