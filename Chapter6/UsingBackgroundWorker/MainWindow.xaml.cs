using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace UsingBackgroundWorker
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

        private BackgroundWorker backgroundWorker;

        private void StartProcesing(object sender, RoutedEventArgs e)
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += PerformCalculation;
            backgroundWorker.RunWorkerCompleted += CalculationDone;
            backgroundWorker.ProgressChanged += UpdateProgress;

            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            backgroundWorker.RunWorkerAsync();
        }

        private void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            this.AsyncProgressBar.Value = e.ProgressPercentage;
        }

        private void CalculationDone(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.ResultLabel.Text = e.Error.Message;
            }
            else if (e.Cancelled)
                this.ResultLabel.Text = "CANCELLED";
            else
                this.ResultLabel.Text = e.Result.ToString();
        }

        private void PerformCalculation(object sender, DoWorkEventArgs e)
        {
           //throw new Exception("Boom!");
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(50);
                backgroundWorker.ReportProgress(i);

                if (backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
            }

            e.Result = 42;
        }

        private void CancelProcessing(object sender, RoutedEventArgs e)
        {
           backgroundWorker.CancelAsync();
        }
    }
}




