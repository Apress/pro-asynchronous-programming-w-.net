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

namespace EAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebClient client;

        public MainWindow()
        {
            InitializeComponent();
            client = new WebClient();

            client.DownloadStringCompleted += ProcessResult;
        }

        private void ProcessResult(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                webPage.Text = e.Error.Message;
            else
                webPage.Text = e.Result;
        }

        private void DownloadIt(object sender, RoutedEventArgs e)
        {  
          
            client.DownloadStringAsync(new Uri("http://www.rocksolidknowledge.com/5SecondPage.aspx"));   
        }
    }
}
