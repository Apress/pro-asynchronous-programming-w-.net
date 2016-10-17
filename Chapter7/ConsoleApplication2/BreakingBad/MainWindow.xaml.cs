using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

namespace CompositeAsyncAwait
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

        private async void LoadContentV1(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            string url = "http://www.msdn.com";

            string pageContent = await client.DownloadStringTaskAsync(url);
                                         
            pageContent = RemoveAdverts(pageContent);

            pageText.Text = pageContent;
        }


        private async void LoadContentV2(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            string url = "http://www.msdn.com";

            string pageContent = await client.DownloadStringTaskAsync(url);

            pageContent = await Task.Run<string>(() => RemoveAdverts(pageContent));

            pageText.Text = pageContent;
        }

        private async void LoadContentV3(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            string url = "http://www.msdn.com";

            string pageContent = await client
                                           .DownloadStringTaskAsync(url)
                                           .ContinueWith(dt => RemoveAdverts(dt.Result));

            pageText.Text = pageContent;
        }

        private async void LoadContentV4(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            string url = "http://www.msdn.com";

            string pageContent = await client
                                           .DownloadStringTaskAsync(url)
                                           .ConfigureAwait(continueOnCapturedContext: false);

            // Continues on the background thread
            // 
            pageContent = RemoveAdverts(pageContent);


            pageText.Text = pageContent;
        }

        private async void LoadContent(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            string url = "http://www.msdn.com";

            string pageContent = await LoadPageAndRemoveAdvertsAsync(url);
            
            //Runs on Ui thread
            pageText.Text = pageContent;
        }

        public async Task<string> LoadPageAndRemoveAdvertsAsync(string url)
        {
            WebClient client = new WebClient();
            
            string pageContent = await client.DownloadStringTaskAsync(url)
                                             .ConfigureAwait(continueOnCapturedContext: false);

            // Continues on the background thread
            // 
            pageContent = RemoveAdverts(pageContent);
            return pageContent;
        }

        private string RemoveAdverts(string pageContent)
        {
            Thread.Sleep(1000);
            return pageContent;
        }
    }
}
