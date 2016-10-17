using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DispatcherWPF
{
    public class ViewModel
    {
        private DispatcherTimer timer;
        private Random rnd = new Random();
        public ViewModel()
        {
            Values = new ObservableCollection<int>();

           // Task.Run(() => GenerateValues());

            timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            timer.Tick += (s, e) => Values.Add(rnd.Next(1, 100));
            timer.Start();
        }
        public ObservableCollection<int> Values { get; private set; }
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        private void GenerateValues()
        {
            var rnd = new Random();
            while (true)
            {
                dispatcher.BeginInvoke(new Action(() => Values.Add(rnd.Next(1, 100))));

                Thread.Sleep(1000);
            }
        }
 
    }
}