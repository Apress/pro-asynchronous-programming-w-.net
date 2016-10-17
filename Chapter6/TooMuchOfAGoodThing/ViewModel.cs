using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TooMuchOfAGoodThing.Annotations;

namespace TooMuchOfAGoodThing
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string result;
        private int progress;

        public ViewModel()
        {
            Task.Factory.StartNew(CalculatePi);
        }

        private int getCount;

        public int GetCount
        {
            get { return getCount; }
            set { getCount = value; OnPropertyChanged();}
        }

       

        private void CalculatePi()
        {
            //const int iterations = 1000000000;
            const int iterations = 100000000;
            double pi = 1;
            double multiplier = -1;

            int lastProgress = 0;
            for (int i = 3; i < iterations; i+=2)
            {
                pi += multiplier*(1.0/(double) i);
                multiplier *= -1;

                int newProgress = (int) ((double) i/(double) (iterations-3)*100.0);

                // comment out the if statement below to update more often than needed
                // and see how much slower things are
                if (lastProgress != newProgress)
                {
                    Progress = newProgress;
                    lastProgress = newProgress;
                }

            }

            Result = (pi*4.0).ToString();
        }

        public string Result
        {
            get { return result; }
            set { result = value; OnPropertyChanged();}
        }

        public int Progress
        {
            get
            {
                GetCount++; return progress; }
            set { progress = value; OnPropertyChanged();}
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { }; 
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}