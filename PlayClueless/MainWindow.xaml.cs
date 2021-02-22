﻿using System;
using System.Windows;
using System.Windows.Controls;

using CluelessCrosswords;

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
            game.Show();
            menuWindow.Close();
        }
    }
}