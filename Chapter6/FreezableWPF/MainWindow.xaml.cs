using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FreezableWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Task.Factory.StartNew<BitmapSource[]>(() => LoadImages(@"C:\windows\Web"))
                .ContinueWith(t =>
                    {
                        foreach (BitmapSource src in t.Result)
                        {

                            images.Children.Add(new Image() { Width = 100, Source = src });
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

       

        }



        private BitmapSource[] LoadImages(string path)
        {
            var srcs = 
                (from file in new DirectoryInfo(path).GetFiles("*.jpg", SearchOption.AllDirectories)
                select new BitmapImage(new Uri(file.FullName))).ToArray();

            foreach (BitmapSource src in srcs)
            {
                src.Freeze(); // Allows bitmap created on this thread to be referenced by a different UI thread
            }
            return srcs;
        }
    }
}
