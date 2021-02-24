using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using CluelessCrosswords;

using Newtonsoft.Json;

namespace PlayClueless
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CheckForSaveData();
        }

        private void CheckForSaveData()
        {
            if (File.Exists(SaveData.puzzleFile) && File.Exists(SaveData.movesFile))
            {
                try
                {
                    _ = JsonConvert.DeserializeObject<Puzzle>(File.ReadAllText(SaveData.puzzleFile));
                    _ = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(SaveData.movesFile));
                    btnContinue.IsEnabled = true;
                }
                catch (Exception) { } //Ignore
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Difficulty difficulty = button.Content switch
            {
                "Easy" => Difficulty.Easy,
                "Normal" => Difficulty.Normal,
                "Hard" => Difficulty.Hard,
                _ => throw new IndexOutOfRangeException(),
            };
            CluelessGame game = new CluelessGame(difficulty);
            SetGameWindowPosition(game);
            game.Show();
            menuWindow.Close();
        }

        private void SetGameWindowPosition(CluelessGame game)
        {
            game.WindowStartupLocation = WindowStartupLocation.Manual;
            game.Left = Properties.Settings.Default.WindowLeft;
            game.Top = Properties.Settings.Default.WindowTop;
            game.Width = Properties.Settings.Default.WindowWidth;
            game.Height = Properties.Settings.Default.WindowHeight;
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            Puzzle savedGame = JsonConvert.DeserializeObject<Puzzle>(File.ReadAllText(SaveData.puzzleFile));
            string[] savedLettersPlayed = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(SaveData.movesFile));
            CluelessGame game = new CluelessGame(savedGame, savedLettersPlayed);
            SetGameWindowPosition(game);
            game.Show();
            menuWindow.Close();
        }
    }
}
