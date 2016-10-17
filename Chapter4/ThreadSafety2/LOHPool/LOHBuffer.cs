using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOHPool
{


    public class LOHBuffer : IBuffer
    {
        private readonly byte[] buffer;
        private const int LOHBufferMin = 85000;
        internal bool InUse { get; set; }

        public LOHBuffer()
        {
            buffer = new byte[LOHBufferMin];

            Console.WriteLine(GC.GetGeneration(buffer));
        }

        public byte[] Buffer { get { return buffer; } }
    }
}
