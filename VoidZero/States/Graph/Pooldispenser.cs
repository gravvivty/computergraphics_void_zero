namespace VoidZero.States.Graph
{
    /// <summary>
    /// Shuffles a pool of factory functions once per run and cycles through them
    /// in order, wrapping back to the start when the pool is exhausted.
    /// This ensures the most variety possible: you see every entry before any repeats.
    /// </summary>
    internal sealed class PoolDispenser<T>
    {
        private readonly List<Func<T>> _shuffled;
        private int _index;

        public PoolDispenser(IEnumerable<Func<T>> pool, Random rng)
        {
            _shuffled = Shuffle(pool.ToList(), rng);
            _index = 0;
        }

        /// <summary>Returns the next item from the shuffled pool, cycling if exhausted.</summary>
        public T Next()
        {
            var item = _shuffled[_index % _shuffled.Count]();
            _index++;
            return item;
        }

        private static List<Func<T>> Shuffle(List<Func<T>> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }
}