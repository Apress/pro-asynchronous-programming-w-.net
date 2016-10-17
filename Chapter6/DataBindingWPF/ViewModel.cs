using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using DataBindingWPF.Annotations;


// BindingOperations.EnableCollectionSynchronization(times, times);


namespace DataBindingWPF
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            Task.Factory.StartNew(() => Result = CalculateMeaningOfLife());
        }

        private decimal result;
        public decimal Result
        {
            get { return result; }
            private set { result = value; OnPropertyChanged("Result"); }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private decimal CalculateMeaningOfLife()
        {
            Thread.Sleep(5000);
            return 42m;
        }
    }
}