using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CloneBehave
{
    /// <summary>
    /// The CloneableDictionary should be used instead of the builtin default Dictionary when a class will be cloned.
    /// It holds a Temporary Dictionary which will automatically be set to null when the class gets cloned.
    /// So the TemporaryDictionary is excluded from clone.
    /// Accessing the CloneableDictionary after clone will recreate the TemporaryDictionary.
    /// </summary>
    public class CloneableDictionary<TKey, TValue> : Collection<Tuple<TKey, TValue>>
    {
        [DeepClone(DeepCloneBehavior.SetToDefault)]
        private IDictionary<TKey, TValue> _tempDictionary;

        public CloneableDictionary(IDictionary<TKey, TValue> aDict)
            : base(aDict.Select(d => Tuple.Create(d.Key, d.Value)).ToList())
        {
            BuildTemporaryDictionary();
        }

        public CloneableDictionary()
        {
        }

        public IDictionary<TKey, TValue> TempDictionary
        {
            get
            {
                if (_tempDictionary == null) BuildTemporaryDictionary();
                return _tempDictionary;
            }
            set { _tempDictionary = value; }
        }

        public void Add(TKey key, TValue value)
        {
            Add(Tuple.Create(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return TempDictionary.ContainsKey(key);
        }

        public TValue GetValue(TKey key, TValue fallbackValue)
        {
            TValue value;
            TempDictionary.TryGetValue(key, out value);
            if (value == null) return fallbackValue;

            return value;
        }

        public void Remove(TKey key)
        {
            Tuple<TKey, TValue> item = Items.FirstOrDefault(i => i.Item1.Equals(key));
            Remove(item);
        }

        protected override void ClearItems()
        {
            TempDictionary.Clear();
            base.ClearItems();
        }

        protected override void InsertItem(int index, Tuple<TKey, TValue> item)
        {
            if (!TempDictionary.ContainsKey(item.Item1))
            {
                base.InsertItem(index, item);
                TempDictionary.Add(item.Item1, item.Item2);
            }
        }

        protected override void RemoveItem(int index)
        {
            TKey key = Items[index].Item1;
            TempDictionary.Remove(key);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Tuple<TKey, TValue> item)
        {
            TKey keyAtIndex = Items[index].Item1;
            if (TempDictionary.ContainsKey(keyAtIndex))
            {
                TempDictionary.Remove(keyAtIndex);
                TempDictionary[item.Item1] = item.Item2;
                base.SetItem(index, item);
            }
        }

        private void BuildTemporaryDictionary()
        {
            TempDictionary = Items.ToDictionary(i => i.Item1, i => i.Item2);
        }
    }
}