using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CustomBlocks
{
    public class Tap<T> : IPropagatorBlock<T,T>
    {
        private T toPropergate;

        public bool IsOpen { get; set; }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source,
                                                  bool consumeToAccept)
        {
            if (!IsOpen) return DataflowMessageStatus.Declined;

            if (consumeToAccept)
            {
                bool messageConsumed;

                source.ConsumeMessage(messageHeader, this, out messageConsumed);
                if (!messageConsumed) return DataflowMessageStatus.Declined;
            }

            toPropergate = messageValue;
            foreach (ITargetBlock<T> target in this.links)
            {
                target.OfferMessage(messageHeader, messageValue, this, true);
            }

            return DataflowMessageStatus.Accepted;
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        public Task Completion { get; private set; }

        List<ITargetBlock<T>> links = new List<ITargetBlock<T>>(); 
        public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
        {
            links.Add(target);

            return null;
        }

        public T ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
        {
            messageConsumed = true;
            return toPropergate;
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            throw new NotImplementedException();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            BufferBlock<int> bufferBlock = new BufferBlock<int>();


            Tap<int> tap = new Tap<int>();

            tap.LinkTo(new ActionBlock<int>(i =>
                {
                    Console.WriteLine("Tap sent me {0}", i);
                    Thread.Sleep(5000);
                }));

            bufferBlock.LinkTo(tap);
            bufferBlock.LinkTo(new ActionBlock<int>(i => Console.WriteLine(i)));
            while (true)
            {
                bool postResult = bufferBlock.Post(1);
                Console.WriteLine(postResult);

                Console.ReadLine();
                tap.IsOpen = !tap.IsOpen;
            }
        }
    }
}
