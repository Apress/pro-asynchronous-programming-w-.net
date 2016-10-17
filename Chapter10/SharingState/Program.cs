using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharingState
{
    public class SmallBusiness
    {
        private decimal income;
        private decimal receivables;

        public virtual decimal NetWorth
        {
            get
            {
                return income + receivables;
            }
        }

        public virtual void RaisedInvoiceFor(decimal amount)
        {
            receivables += amount;
        }

        public virtual void ReceivePayments(decimal payment)
        {
            income += payment;
            receivables -= payment;
        }
    }

    public class SmallBusinessRWLock : SmallBusiness
    {

        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        public override decimal NetWorth
        {
            get
            {
                 rwLock.EnterReadLock();
                try{return base.NetWorth;}
                finally { rwLock.ExitReadLock(); }
            }
        }
        public override void RaisedInvoiceFor(decimal amount)
        {
            rwLock.EnterWriteLock();
            try { base.RaisedInvoiceFor(amount);}
            finally{ rwLock.ExitWriteLock();}
        }

        public override void ReceivePayments(decimal payment)
        {
            rwLock.EnterWriteLock();
            try { base.ReceivePayments(payment);}
            finally{rwLock.ExitWriteLock();}
        }
    }

    public class SmallBusinessAsync
    {
        private decimal income;
        private decimal receivables;
        
        private ConcurrentExclusiveSchedulerPair rwScheduler  = new ConcurrentExclusiveSchedulerPair();
        public Task<decimal> NetWorthAsync
        {
            
            get
            {
                return Task.Factory.StartNew<decimal>( () => income + receivables
                    , CancellationToken.None,TaskCreationOptions.None,rwScheduler.ConcurrentScheduler);
            }
        }

        public Task RaisedInvoiceForAsync(decimal amount)
        {
            return Task.Factory.StartNew(() => receivables += amount,
                CancellationToken.None, 
                TaskCreationOptions.None, rwScheduler.ExclusiveScheduler);
        }

        public Task ReceivePaymentsAsync(decimal payment)
        {
            return Task.Factory.StartNew(() =>
            {
                income += payment;
                receivables -= payment;
            }, CancellationToken.None,TaskCreationOptions.None,rwScheduler.ExclusiveScheduler);
        }
    }

    class Program
    {
        private static void Main(string[] args)
        {
            StressSmallBusiness();

            // StressSmallBusinessAsync();
        }

        private static void StressSmallBusinessAsync()
        {
            var acme = new SmallBusinessAsync();

            acme.RaisedInvoiceForAsync(1000).Wait();

            var audit = Task.Run(async () =>
            {
                while (true)
                {
                    if (await acme.NetWorthAsync != 1000)
                    {
                        Console.WriteLine("Corruption");
                    }
                }
            });


            for (int i = 0; i < 1000; i++)
            {
                acme.ReceivePaymentsAsync(1);
                Thread.Sleep(10);
            }
        }

        private static void StressSmallBusiness()
        {
            SmallBusiness acme = 
                //new SmallBusiness();
                new SmallBusinessRWLock();

            acme.RaisedInvoiceFor(1000);

            var audit = Task.Run(() =>
            {
                while (true)
                {
                    if (acme.NetWorth != 1000)
                    {
                        Console.WriteLine("Corruption");
                    }
                }
            });

            for (int i = 0; i < 1000; i++)
            {
                acme.ReceivePayments(1);
                Thread.Sleep(10);
            }



        }
    }
}
