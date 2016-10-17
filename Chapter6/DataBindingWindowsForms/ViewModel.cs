using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataBindingWindowsForms
{

    public class ViewModel : INotifyPropertyChanged
    {
        private SynchronizationContext uiCtx;

        public ViewModel()
        {
            uiCtx = SynchronizationContext.Current;
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Result = CalculateNexResult();
                    }
                });
        }

        private decimal result;
        

        public decimal Result
        {
            get { return result; }
            private set
            {
                result = value;
                OnPropertyChanged("Result");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            uiCtx.Send(_ => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)),null);
        }

        private int i = 1;
        private decimal CalculateNexResult()
        {
            Thread.Sleep(5000);
            return i++;
        }
    }
}