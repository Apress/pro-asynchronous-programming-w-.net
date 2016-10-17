using System.Collections;
using System.Collections.Generic;

namespace ParallelLoops
{
    public class AsEnumerableAdapter<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> source;

        public AsEnumerableAdapter(IEnumerable<T> source )
        {
            this.source = source;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}