using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DataBindingCollectionsWPF
{
    public class ViewModel
    {
        ReaderWriterLockSlim valuesLock = new ReaderWriterLockSlim();

        //private object valuesLock = new object();
        public ViewModel()
        {
            Values = new ObservableCollection<int>();

          //BindingOperations.EnableCollectionSynchronization(Values, valuesLock);

            
            BindingOperations.EnableCollectionSynchronization(Values,valuesLock,ControlAccessToValues);
            Task.Factory.StartNew(GenerateValues);
        }

        private void ControlAccessToValues(IEnumerable collection, object context, Action accessmethod, bool writeaccess)
        {
            
            var collectionLock = (ReaderWriterLockSlim) context;
            Action enterLock = writeaccess
                                   ? new Action(collectionLock.EnterWriteLock)
                                   : new Action(collectionLock.EnterReadLock);

            Action exitLock = writeaccess
                                  ? new Action(collectionLock.ExitWriteLock)
                                  : new Action(collectionLock.ExitReadLock);

            enterLock();
                try
                {
                    accessmethod(); 
                }
                finally
                {
                    exitLock();
                }
        }

        public ObservableCollection<int> Values { get; private set; }

        private SynchronizationContext uiCtx = SynchronizationContext.Current;

        //private void GenerateValues()
        //{
        //    var rnd = new Random();
        //    while (true)
        //    {
        //      uiCtx.Post( _ => Values.Add(rnd.Next(1, 100)),null);
               
        //      Thread.Sleep(1000);
        //    }
        //}

        //private void GenerateValues()
        //{
        //    var rnd = new Random();
        //    while (true)
        //    {
        //        lock (Values)
        //        {
        //            Values.Add(rnd.Next(1, 100));
        //        }

        //        Thread.Sleep(1000);
        //    }
        //}

        private void GenerateValues()
        {
            var rnd = new Random();
            while (true)
            {
                valuesLock.EnterWriteLock();
                try{Values.Add(rnd.Next(1, 100));}
                finally{valuesLock.ExitWriteLock();}

                Thread.Sleep(1000);
            }
        }
    }
}