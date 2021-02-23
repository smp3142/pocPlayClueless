using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using CluelessCrosswords;

using Newtonsoft.Json;

// TODO: Add option to click on letters in Clueless
// TODO: Have victory check if all words are good

namespace PlayClueless
{
    public static class SaveData
    {
        public static readonly string puzzleFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath) + "/puzzleFile");
        public static readonly string movesFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath) + "/savedMoves");
    }

    /// <summary>
    /// Interaction logic for CluelessGame.xaml
    /// </summary>
    public partial class CluelessGame : Window
    {
        public Difficulty difficulty;
        public Puzzle puzzle;
        private string[] lettersPlayed;
        private readonly TextBlock[] HintsTextBlocks;
        private readonly TextBlock[,] GameBoardTextBlocks;
        private readonly TextBlock[] LettersTextBlocks;

        private readonly Brush GivenLetterColor = (Brush)new BrushConverter().ConvertFromString("#4575b4");
        private readonly Brush ErrorLetterColor = (Brush)new BrushConverter().ConvertFromString("#d73027");
        private readonly Brush PaleLetterColor = (Brush)new BrushConverter().ConvertFromString("#e0f3f8");
        private readonly Brush NumberColor = (Brush)new BrushConverter().ConvertFromString("#abd9e9");
        private readonly Brush LetterColor = Brushes.Black;
        private readonly Brush SelectedForegroundColor = (Brush)new BrushConverter().ConvertFromString("#f46d43");
        private readonly Brush SelectedBackgroundColor = (Brush)new BrushConverter().ConvertFromString("#ffffbf");

        private readonly string NameMarker = "VALUE";
        private readonly int smallestFontSize = 20;
        private int currentFontSize = 20;

        public CluelessGame()
        {
            InitializeComponent();

            difficulty = Difficulty.Normal;

            HintsTextBlocks = new TextBlock[26];
            GameBoardTextBlocks = new TextBlock[13, 13];
            LettersTextBlocks = new TextBlock[26];

            MakeNewGame();
        }

        public CluelessGame(Difficulty gameDifficulty)
        {
            InitializeComponent();

            difficulty = gameDifficulty;

            HintsTextBlocks = new TextBlock[26];
            GameBoardTextBlocks = new TextBlock[13, 13];
            LettersTextBlocks = new TextBlock[26];

            MakeNewGame();
        }

        public CluelessGame(Puzzle savedPuzzle, string[] savedLettersPlayed)
        {
            InitializeComponent();

            HintsTextBlocks = new TextBlock[26];
            GameBoardTextBlocks = new TextBlock[13, 13];
            LettersTextBlocks = new TextBlock[26];

            puzzle = savedPuzzle;
            lettersPlayed = savedLettersPlayed;

            InitizeTextBlocksArrays();
            AddHints();
            AddGameBoard();
            AddLetters();
            AddGridBorders();

            AddSavedLetters();

            SetColors();
        }

        private void AddSavedLetters()
        {
            for (int i = 0; i < lettersPlayed.Length; i++)
            {
                if (lettersPlayed[i] != null)
                {
                    SetCellValue(HintsTextBlocks[i], lettersPlayed[i]);
                }
            }
        }

        private void InitizeTextBlocksArrays()
        {
            for (int i = 0; i < LettersTextBlocks.Length; i++)
            {
                LettersTextBlocks[i] = new TextBlock()
                {
                    FontSize = currentFontSize,
                    FontFamily = new FontFamily("Consolas"),
                    Foreground = LetterColor,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }

            for (int i = 0; i < HintsTextBlocks.Length; i++)
            {
                HintsTextBlocks[i] = new TextBlock()
                {
                    FontSize = currentFontSize,
                    FontFamily = new FontFamily("Consolas"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }

            // Initialize Gameboard
            for (int row = 0; row < 13; row++)
            {
                for (int col = 0; col < 13; col++)
                {
                    GameBoardTextBlocks[row, col] = new TextBlock()
                    {
                        FontSize = currentFontSize,
                        FontFamily = new FontFamily("Consolas"),
                        TextAlignment = TextAlignment.Center,
                    };
                    GameBoardTextBlocks[row, col].Margin = (new Thickness(0, 5, 0, 0));
                }
            }
        }

        private void MakeNewGame()
        {
            puzzle = new Games(1, Difficulty.Hard).Puzzles[0];
            lettersPlayed = new string[26];

            string json = JsonConvert.SerializeObject(puzzle);
            File.WriteAllText(SaveData.puzzleFile, json);
            json = JsonConvert.SerializeObject(lettersPlayed);
            File.WriteAllText(SaveData.movesFile, json);

            InitizeTextBlocksArrays();
            AddHints();
            AddGameBoard();
            AddLetters();
            AddGridBorders();
        }

        private void AddLetters()
        {
            if (LettersGrid.Children.Count > 0) { LettersGrid.Children.RemoveRange(0, LettersGrid.Children.Count); }

            string[] letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            for (int i = 0; i < letters.Length; i++)
            {
                LettersTextBlocks[i].Text = letters[i];
                if (puzzle.Hints.Contains(letters[i]))
                {
                    LettersTextBlocks[i].Foreground = PaleLetterColor;
                }
                else
                {
                    LettersTextBlocks[i].Foreground = LetterColor;
                }
                LettersTextBlocks[i].Focusable = false;
                Grid.SetColumn(LettersTextBlocks[i], i % 2);
                Grid.SetRow(LettersTextBlocks[i], i / 2);
                LettersGrid.Children.Add(LettersTextBlocks[i]);
            }
        }

        private void AddGameBoard()
        {
            if (GameGrid.Children.Count > 0) { GameGrid.Children.RemoveRange(0, GameGrid.Children.Count); }

            for (int row = 0; row < 13; row++)
            {
                for (int col = 0; col < 13; col++)
                {
                    GameBoardTextBlocks[row, col].Text = puzzle.GameBoard[row, col];

                    if (GameBoardTextBlocks[row, col].Text == Games.EMPTYCHAR)
                    {
                        GameBoardTextBlocks[row, col].Focusable = false;
                        Rectangle rectangle = new Rectangle() { Fill = Brushes.DimGray };
                        Grid.SetColumn(rectangle, col);
                        Grid.SetRow(rectangle, row);
                        GameGrid.Children.Add(rectangle);
                    }
                    else
                    {

                        if (Int32.TryParse(GameBoardTextBlocks[row, col].Text, out _))
                        {
                            GameBoardTextBlocks[row, col].Foreground = NumberColor;
                            GameBoardTextBlocks[row, col].MouseDown += TextBlock_MouseDown;
                            GameBoardTextBlocks[row, col].KeyDown += TextBlock_KeyDown;
                            GameBoardTextBlocks[row, col].LostFocus += Selection_LostFocus;
                            GameBoardTextBlocks[row, col].Name = NameMarker + GameBoardTextBlocks[row, col].Text;
                            GameBoardTextBlocks[row, col].Focusable = true;
                        }
                        else
                        {
                            GameBoardTextBlocks[row, col].Foreground = LetterColor;
                            GameBoardTextBlocks[row, col].Foreground = GivenLetterColor;
                            GameBoardTextBlocks[row, col].Focusable = false;
                        }
                        Grid.SetColumn(GameBoardTextBlocks[row, col], col);
                        Grid.SetRow(GameBoardTextBlocks[row, col], row);
                        GameGrid.Children.Add(GameBoardTextBlocks[row, col]);
                    }
                }
            }
        }

        private void AddHints()
        {
            if (HintsGrid.Children.Count > 0) { HintsGrid.Children.RemoveRange(0, HintsGrid.Children.Count); }

            for (int i = 0; i < puzzle.Hints.Length; i++)
            {
                HintsTextBlocks[i].Text = puzzle.Hints[i];

                if (Int32.TryParse(HintsTextBlocks[i].Text, out _))
                {
                    HintsTextBlocks[i].MouseDown += TextBlock_MouseDown;
                    HintsTextBlocks[i].KeyDown += TextBlock_KeyDown;
                    HintsTextBlocks[i].LostFocus += Selection_LostFocus;
                    HintsTextBlocks[i].Focusable = true;
                    HintsTextBlocks[i].Foreground = NumberColor;
                    HintsTextBlocks[i].Name = NameMarker + HintsTextBlocks[i].Text;
                }
                else
                {
                    HintsTextBlocks[i].Foreground = GivenLetterColor;
                    HintsTextBlocks[i].Focusable = false;
                }
                Grid.SetColumn(HintsTextBlocks[i], i % 2);
                Grid.SetRow(HintsTextBlocks[i], i / 2);
                HintsGrid.Children.Add(HintsTextBlocks[i]);
            }
        }

        private void AddGridBorders()
        {
            foreach (Grid grid in new[] { HintsGrid, GameGrid, LettersGrid })
            {
                for (int row = 0; row < 13; row++)
                {
                    for (int col = 0; col < grid.ColumnDefinitions.Count; col++)
                    {
                        Border border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.Gray };
                        Grid.SetColumn(border, col);
                        Grid.SetRow(border, row);
                        grid.Children.Add(border);
                    }
                }
            }
        }

        private void ClearFocus(TextBlock textBlock)
        {
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBlock), null);
            Keyboard.ClearFocus();
        }

        private void Selection_LostFocus(object sender, RoutedEventArgs e)
        {
            SetColors();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock.IsFocused) { ClearFocus(textBlock); }
            else { textBlock.Focus(); }
            SetColors();
        }

        private void TextBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                return;
            }

            TextBlock textBlock = sender as TextBlock;
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                string key = e.Key.ToString();
                SetCellValue(textBlock, key);
            }
            else if (e.Key == Key.Escape)
            {
                SetColors();
            }
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                string key = textBlock.Name.Trim(NameMarker.ToArray());
                SetCellValue(textBlock, key);
            }
            ClearFocus(textBlock);
        }

        private void SaveState()
        {
            string json = JsonConvert.SerializeObject(lettersPlayed);
            File.WriteAllText(SaveData.movesFile, json);
        }

        private void CheckWin()
        {
            for (int i = 0; i < HintsTextBlocks.Length; i++)
            {
                if (HintsTextBlocks[i].Text != puzzle.Key[i]) { return; }
            }
            var again = MessageBox.Show("Congratulations, you have solved the puzzle!\nPlay again ?", "Win", MessageBoxButton.YesNo);
            if (again == MessageBoxResult.Yes) { MakeNewGame(); }
        }

        private void SetCellValue(TextBlock textBlock, string key)
        {
            for (int i = 0; i < HintsTextBlocks.Length; i++)
            {
                if (HintsTextBlocks[i].Name == textBlock.Name)
                {
                    HintsTextBlocks[i].Text = key;
                    lettersPlayed[i] = key;
                    break;
                }
            }
            foreach (TextBlock item in GameBoardTextBlocks)
            {
                if (item.Name == textBlock.Name)
                {
                    item.Text = key;
                }
            }
            ClearFocus(textBlock);
            SetColors();
            CheckWin();
            SaveState();
        }

        private void SetColors()
        {
            Brush foregroundColor;

            for (int i = 0; i < HintsTextBlocks.Length; i++)
            {
                if (!Int32.TryParse(puzzle.Hints[i], out _)) { continue; }
                if (HintsTextBlocks.Count(block => block.Text == HintsTextBlocks[i].Text) > 1) { foregroundColor = ErrorLetterColor; }
                else if (Int32.TryParse(HintsTextBlocks[i].Text, out _)) { foregroundColor = NumberColor; }
                else { foregroundColor = LetterColor; }

                HintsTextBlocks[i].Foreground = foregroundColor;
                HintsTextBlocks[i].Background = Brushes.White;

                foreach (TextBlock gameItem in GameBoardTextBlocks)
                {
                    if (gameItem.Name == HintsTextBlocks[i].Name)
                    {
                        gameItem.Foreground = foregroundColor;
                        gameItem.Background = Brushes.White;
                    }
                }
            }

            foreach (TextBlock letterItem in LettersTextBlocks)
            {
                int counter = HintsTextBlocks.Count(item => item.Text == letterItem.Text);
                if (counter > 1) { foregroundColor = ErrorLetterColor; }
                else if (counter == 1) { foregroundColor = PaleLetterColor; }
                else { foregroundColor = LetterColor; }
                letterItem.Foreground = foregroundColor;
            }

            IInputElement focusedControl = Keyboard.FocusedElement;
            if (focusedControl is TextBlock focusedTextBlock)
            {
                ColorSelected(focusedTextBlock.Name);
            }
        }

        private void ColorSelected(string name)
        {
            foreach (TextBlock item in HintsTextBlocks)
            {
                if (item.Name == name)
                {
                    item.Foreground = SelectedForegroundColor;
                    item.Background = SelectedBackgroundColor;
                    break;
                }
            }
            foreach (TextBlock item in GameBoardTextBlocks)
            {
                if (item.Name == name)
                {
                    item.Foreground = SelectedForegroundColor;
                    item.Background = SelectedBackgroundColor;
                }
            }
        }

        private void WindowResized(object sender, SizeChangedEventArgs e)
        {
            int size1 = (int)HintsGrid.ActualWidth / 4;
            int size2 = (int)HintsGrid.ActualHeight / 26;
            size1 = size1 <= size2 ? size1 : size2;
            size1 = size1 < smallestFontSize ? smallestFontSize : size1;
            size1 = size1 > 40 ? 40 : size1;
            foreach (TextBlock item in HintsTextBlocks)
            {
                item.FontSize = size1;
            }
            foreach (TextBlock item in GameBoardTextBlocks)
            {
                item.FontSize = size1;
            }
            foreach (TextBlock item in LettersTextBlocks)
            {
                item.FontSize = size1;
            }
            currentFontSize = size1;
        }

        private void GameWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.WindowLeft = gameWindow.Left;
            Properties.Settings.Default.WindowTop = gameWindow.Top;
            Properties.Settings.Default.WindowWidth = gameWindow.ActualWidth;
            Properties.Settings.Default.WindowHeight = gameWindow.ActualHeight;
            Properties.Settings.Default.Save();
        }
    }
}
