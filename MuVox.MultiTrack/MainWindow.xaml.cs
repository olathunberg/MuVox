﻿using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;

namespace MuVox.MultiTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Width = (DataContext as MainViewModel).Faders.Sum(x => x.Width) + 38;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel && DataContext is ICleanup cleanup)
                cleanup.Cleanup();
        }
    }
}
