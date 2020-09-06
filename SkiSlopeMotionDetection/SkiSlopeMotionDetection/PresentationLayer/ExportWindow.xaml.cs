using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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

        public ExportWindow(bool exportEntireVideo = false, bool includeMarking = true)
        {
            InitializeComponent();

            ExportSettings = new ExportSettings(exportEntireVideo, includeMarking);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var videoFilter = "Video files (*.mp4;*.avi)|*.mp4;*.avi";
            
            var imageFilter = 
                "Bitmap (*.bmp)|*.bmp|" +
                "Graphics Interchange Format (*.gif)|*.gif|" +
                "Exchangeable Image File Format (*.exif)|*.exif|" +
                "JPEG Image (*.jpg)|*.jpg|" +
                "Portable Network Graphics (*.png)|*.png|" +
                "Tagged Image File Format (*.tiff)|*.tiff";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = ExportSettings.ExportEntireVideo ? videoFilter : imageFilter,
                InitialDirectory = Environment.CurrentDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExportFrame(saveFileDialog.FileName);

                Owner = null;
                Close();
            }
        }

        private void ExportFrame(string outputFileName)
        {
            if (!(Owner is MainWindow mainWindow))
                throw new ApplicationException("Unable to get current frame");

            var extension = Path.GetExtension(outputFileName);
            Bitmap currentFrame;

            if(!ExportSettings.IncludeMarking)
            {
                var reader = FrameReaderSingleton.GetInstance();
                currentFrame = reader.GetFrame(mainWindow.CurrentFrameNumber);
            }
            else
            {
                currentFrame = mainWindow.CurrentFrame;
            }

            currentFrame.Save(outputFileName, extension.ToImageFormat());
        }
    }
}
