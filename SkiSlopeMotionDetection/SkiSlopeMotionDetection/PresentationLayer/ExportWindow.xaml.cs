﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window, INotifyPropertyChanged
    {
        private ExportSettings _settings;

        public ExportSettings ExportSettings 
        { 
            get { return _settings; }
            set { _settings = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = ExportSettings.ExportEntireVideo ? "Video files (*.mp4;*.avi)|*.mp4;*.avi" : "Bitmap (*.bmp)|*.bmp";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if(saveFileDialog.ShowDialog() == true)
            {
                // TODO: Export
            }
        }

        public ExportWindow(bool exportEntireVideo = false, bool includeMarking = true)
        {
            InitializeComponent();

            ExportSettings = new ExportSettings(exportEntireVideo, includeMarking);
        }
    }
}