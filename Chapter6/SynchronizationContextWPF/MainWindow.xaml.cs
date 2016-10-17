using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SynchronizationContextWPF
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

        //private void ButtonClick(object sender, RoutedEventArgs e)
        //{

        //    string scheduler = TaskScheduler.FromCurrentSynchronizationContext().GetType().Name;

        //    SynchronizationContext ctx = SynchronizationContext.Current;

        //    Task.Factory.StartNew(() =>
        //    {
        //        decimal result = CalculateMeaningOfLife();

        //        ctx.Post(state => resultLabel.Text = result.ToString(), null);
        //    });
        //}

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Task.Factory
                .StartNew<decimal>(() => CalculateMeaningOfLife())
                .ContinueWith(t => resultLabel.Text = t.Result.ToString(),
                              TaskScheduler.FromCurrentSynchronizationContext());
        }

     
        private decimal CalculateMeaningOfLife()
        {
            Thread.Sleep(5000);
            return 42.0m;
        }

       
    }
}
