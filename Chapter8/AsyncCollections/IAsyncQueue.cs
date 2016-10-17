using System.Threading.Tasks;

namespace AsyncCollections
{
    public interface IAsyncQueue<T>
    {
        void Enqueue(T item);
        Task<T> Dequeue();
    }
}