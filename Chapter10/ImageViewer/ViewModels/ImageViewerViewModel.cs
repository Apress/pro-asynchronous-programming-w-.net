using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MVVM;

namespace ImageViewer.ViewModels
{

    public class ImageViewerViewModel
    {
        ImageProcessor imageProcessor = new ImageProcessor();

        public ImageViewerViewModel()
        {
            GreyImages = new ObservableCollection<BitmapSource>();

            imageProcessor.ProducedGrayScaleImage += (s, e) => GreyImages.Add(e.Img);

            AddImage = new DelegateCommand(ProcessImage);
            AddDirectory = new DelegateCommand(ProcessDirectory);
            done = new DelegateCommand(EndProcessing);
        }

        private async void EndProcessing(object obj)
        {
           
            done.SetCanExecute(false);
            await imageProcessor.ShutdownAsync();

        }

        private void ProcessDirectory(object obj)
        {
            imageProcessor.ProcessDirectory(ImageDirectory);
        }

        private void ProcessImage(object obj)
        {
            imageProcessor.ProcessFile(ImageFilename);
        }

        public ObservableCollection<BitmapSource> GreyImages { get; private set; }
        
        public ICommand AddImage { get; set; } 
        public ICommand AddDirectory { get; set; }

        public ICommand Done
        {
            get { return done; }
           
        }

        private string imageFilename;

        public string ImageFilename
        {
            get { return imageFilename; }
            set { imageFilename = value; }
        }

        private string imageDirectory;
        private DelegateCommand done;

        public string ImageDirectory
        {
            get { return imageDirectory; }
            set { imageDirectory = value; }
        }
        
    }
}
