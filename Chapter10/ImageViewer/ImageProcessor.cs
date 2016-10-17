using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO.Compression;


namespace ImageViewer
{
    public class ProcessedImageEventArgs : EventArgs
    {
        public BitmapSource Img { get; private set; }

        public ProcessedImageEventArgs(BitmapSource img)
        {
            Img = img;
        }
    }

    public class ImageProcessor 
    {
        public event EventHandler<ProcessedImageEventArgs> ProducedGrayScaleImage = delegate { };

        
        private TransformManyBlock<string,string> imageCollectionBlock;
        private TransformBlock<string, BitmapSource> loadAndToGreyBlock;
        private ActionBlock<BitmapSource> publishImageBlock;
       
        public ImageProcessor()
        {
            imageCollectionBlock =
                new TransformManyBlock<string, string>(
                    (Func<string, IEnumerable<string>>) FindImagesInDirectory);

            loadAndToGreyBlock = new TransformBlock<string, BitmapSource>((Func<string, BitmapSource>)LoadAndToGrayScale,
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 4,
                    
                });

            publishImageBlock = new ActionBlock<BitmapSource>((Action<BitmapSource>) PublishImage,
                new ExecutionDataflowBlockOptions()
                {
                    TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
                });


            //zipBlock.LinkTo(zipBlock, u => u.AbsolutePath.EndsWith(".zip"));
            //zipBlock.LinkTo(loadAndToGreyBlock);

            //imageCollectionBlock.LinkTo(zipBlock,u => u.AbsoluteUri.EndsWith(".zip"));

            imageCollectionBlock.LinkTo(loadAndToGreyBlock,u => u.EndsWith(".jpg"));
            imageCollectionBlock.LinkTo(imageCollectionBlock);

            loadAndToGreyBlock.LinkTo(publishImageBlock);

          
        }

       

        public void ProcessFile(string filename)
        {
            //Task.Run(() =>LoadAndToGrayScale( filename)
            //.ContinueWith(
            //     toGreyTask =>ProducedGrayScaleImage(this, new ProcessedImageEventArgs(toGreyTask.Result)),
            //     TaskScheduler.FromCurrentSynchronizationContext());

            loadAndToGreyBlock.Post(filename);
        }

        public void ProcessDirectory(string dir)
        {
             imageCollectionBlock.Post(dir);
            //DirectoryInfo directory = new DirectoryInfo(dir);
            //foreach (FileInfo file in directory.GetFiles("*.jpg", SearchOption.AllDirectories))
            //{
            //    loadAndToGreyBlock.Post(new Uri(file.FullName));
            //}

        }

       private IEnumerable<string> FindImagesInDirectory(string directory)
        {
           DirectoryInfo dir = new DirectoryInfo(directory);
            return dir
                .GetFiles("*.jpg")
                .Select(file => file.FullName)
                .Concat(dir.GetDirectories().Select(di => di.FullName));

        }

        private void PublishImage(BitmapSource img)
        {
            ProducedGrayScaleImage(this, new ProcessedImageEventArgs(img));
        }

        private static  BitmapSource LoadAndToGrayScale(string imagePath)
        {
            var img = new BitmapImage(new Uri(imagePath));

           
            return ToGrayScale(img);
        }
        
        private static  BitmapSource ToGrayScale(BitmapSource bitmapSource)
        {
            var img = bitmapSource;

            var width = (int)img.Width;
            var height = (int)img.Height;
            double dpi = img.DpiX;

            var pixelData = new byte[(int)img.Width * (int)img.Height * 4];
            var outputData = new byte[width * height];

            img.CopyPixels(pixelData, (int)img.Width * 4, 0);

            int totalIterations = 5 * width;

            for (int i = 0; i < 1; i++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    for (int y = 0; y < img.Height; y++)
                    {

                        byte red = pixelData[y * (int)img.Width * 4 + x * 4 + 1];
                        byte green = pixelData[y * (int)img.Width * 4 + x * 4 + 2];
                        byte blue = pixelData[y * (int)img.Width * 4 + x * 4 + 3];

                        var gray = (byte)(red * 0.299 + green * 0.587 + blue * 0.114);

                        outputData[y * width + x] = gray;
                    }
                }
            }

            var grey = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Gray8, null, outputData, width);
            grey.Freeze();
            return grey;
        }

        public async Task ShutdownAsync()
        {
            this.imageCollectionBlock.Complete();
            await this.imageCollectionBlock.Completion.ConfigureAwait(false);
            
            this.loadAndToGreyBlock.Complete();
            //this.loadAndToGreyBlock.Completion.Wait();
            await this.loadAndToGreyBlock.Completion.ConfigureAwait(false);

            this.publishImageBlock.Complete();
            //this.publishImageBlock.Completion.Wait();
            await publishImageBlock.Completion.ConfigureAwait(false);
            ProducedGrayScaleImage = null;

        }
    }
}
