using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace TooMuchOfAGoodThing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new ViewModel();

          //Task.Run(() => CalculatePi());
            
         //   Task.Run(() => OptimalCalculatePi());


        }

        private SynchronizationContext uiCtx = SynchronizationContext.Current;
        
        private Dispatcher dispatcher = Application.Current.Dispatcher;

        private void CalculatePi()
        {
            const int iterations = 1000000000;
            double pi = 1;
            double multiplier = -1;

            DispatcherOperation updateOp = null;

            Stopwatch timer = Stopwatch.StartNew();

            for (int i = 3; i < iterations; i += 2)
            {
                pi += multiplier*(1.0/(double) i);
                multiplier *= -1;

                

                if (updateOp == null || updateOp.Status == DispatcherOperationStatus.Completed)
                {

                    updateOp = Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                                                     new Action(() => progressBar.Value = (int) ((double) i/(double) iterations*100.0)));
                }
            }

            uiCtx.Post(_ => this.result.Text = (pi*4.0).ToString() + timer.Elapsed.ToString(), null);
        }

        private void OptimalCalculatePi()
        {
            const int iterations = 1000000000;
            double pi = 1;
            double multiplier = -1;

            Stopwatch timer = Stopwatch.StartNew();

            int nBlocks = 100;
            int blockSize = iterations/nBlocks;

            for (int nBlock = 0; nBlock < nBlocks; nBlock++)
            {
                for (int i = 3 + nBlock*blockSize; i <  nBlock*blockSize + blockSize; i += 2)
                {
                    pi += multiplier*(1.0/(double) i);
                    multiplier *= -1;
                }
                uiCtx.Post(_ => progressBar.Value = nBlock, null);
            }


            uiCtx.Post(_ => this.result.Text = (pi * 4.0).ToString() + timer.Elapsed.ToString(), null);
        }
    }
}
