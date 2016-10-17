using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LOHPool
{
    public class BufferPool
    {
        private SemaphoreSlim guard;
        private List<LOHBuffer> buffers; 

        public BufferPool(int maxSize)
        {
            guard = new SemaphoreSlim(maxSize);
            buffers = new List<LOHBuffer>(maxSize);
        }

        public IBufferRegistration GetBuffer()
        {
            guard.Wait();
            // can get buffer
            lock (buffers)
            {
                IBufferRegistration freeBuffer = null;
                foreach (LOHBuffer buffer in buffers)
                {
                    if (!buffer.InUse)
                    {
                        buffer.InUse = true;
                        freeBuffer = new BufferReservation(this, buffer);
                    }
                }

                if (freeBuffer == null)
                {
                    var buffer = new LOHBuffer();
                    buffer.InUse = true;
                    buffers.Add(buffer);
                    freeBuffer = new BufferReservation(this, buffer);
                }

                return freeBuffer;
            }
        }

        private void Release(LOHBuffer buffer)
        {
            buffer.InUse = false;
            guard.Release();
        }

        class BufferReservation : IBufferRegistration
        {
            private readonly BufferPool pool;
            private readonly LOHBuffer buffer;

            public BufferReservation(BufferPool pool, LOHBuffer buffer)
            {
                this.pool = pool;
                this.buffer = buffer;
            }

            public byte[] Buffer
            {
                get { return buffer.Buffer; }
            }

            public void Dispose()
            {
                pool.Release(buffer);
            }
        }
    }
}
