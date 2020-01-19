﻿using System.Windows.Controls;

namespace TTech.Muvox.Features.Recorder
{
    /// <summary>
    /// Interaction logic for RecorderView.xaml
    /// </summary>
    public partial class RecorderView : UserControl
    {
        public RecorderView()
        {
            InitializeComponent();

            this.Loaded += RecorderView_Loaded;
        }

        void RecorderView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
        }
    }
}