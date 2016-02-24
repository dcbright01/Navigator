using System.Collections.Generic;

namespace Navigator.Primitives
{
    public class FixedSizeQueue<T> : Queue<T>
    {
        public FixedSizeQueue(int limit)
            : base(limit)
        {
            Limit = limit;
        }

        public int Limit { get; set; } = -1;

        public new void Enqueue(T item)
        {
            if (Count >= Limit)
            {
                Dequeue();
            }
            base.Enqueue(item);
        }
    }
}