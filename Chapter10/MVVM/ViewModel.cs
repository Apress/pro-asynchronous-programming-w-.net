using System.ComponentModel;

namespace MVVM
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        protected virtual void OnPropertyChanged(string property)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { }; 
    }
}