using System;
using System.Collections.Generic;

namespace CataMapGen.Utils
{
    /// <summary>
    /// Weighted random selection list (matches CDDA's weighted_int_list)
    /// </summary>
    public class WeightedList<T>
    {
        private readonly List<(T item, int weight)> _items = new List<(T, int)>();
        private int _totalWeight;
        private readonly Random _rng;

        public WeightedList(Random rng = null)
        {
            _rng = rng ?? new Random();
        }

        public void Add(T item, int weight)
        {
            if (weight <= 0) return;
            _items.Add((item, weight));
            _totalWeight += weight;
        }

        public void Clear()
        {
            _items.Clear();
            _totalWeight = 0;
        }

        public T Pick()
        {
            if (_items.Count == 0 || _totalWeight == 0)
                return default;

            int roll = _rng.Next(_totalWeight);
            int cumulative = 0;

            foreach (var (item, weight) in _items)
            {
                cumulative += weight;
                if (roll < cumulative)
                    return item;
            }

            return _items[_items.Count - 1].item;
        }

        public int Count => _items.Count;
        public int TotalWeight => _totalWeight;
    }
}
