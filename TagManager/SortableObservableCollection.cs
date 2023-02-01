using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TagManager
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed and allows sorting.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [Serializable]
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public new void Add(T _val)
        {
            Items.Add(_val);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _val));
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection)
            {
                Items.Add(i);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList()));
        }
        public void AddRange(IEnumerable<T> collection, bool _unique)
        {
            IEnumerable<T> uniqueColl;
            if (_unique)
            {
                uniqueColl = collection.Distinct();
                uniqueColl = uniqueColl.Except(uniqueColl.Intersect(Items));
            }
            else
            {
                uniqueColl = collection;
            }
            foreach (var i in uniqueColl)
            {
                Items.Add(i);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList()));
        }
        public void RemoveRange(IEnumerable<T> collection)
        {
            foreach (var i in collection)
            {
                if (Items.Contains(i))
                {
                    Items.Remove(i);
                }
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, collection.ToList()));
        }
        /// <summary>
        /// Sorts the items of the collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        public void Sort<TKey>(Func<T, TKey> keySelector)
        {
            InternalSort(Items.OrderBy(keySelector));
        }

        /// <summary>
        /// Sorts the items of the collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            InternalSort(Items.OrderBy(keySelector, comparer));
        }

        /// <summary>
        /// Moves the items of the collection so that their orders are the same as those of the items provided.
        /// </summary>
        /// <param name="sortedItems">An <see cref="IEnumerable{T}"/> to provide item orders.</param>
        private void InternalSort(IEnumerable<T> sortedItems)
        {
            var sortedItemsList = sortedItems.ToList();

            foreach (var item in sortedItemsList)
            {
                Move(IndexOf(item), sortedItemsList.IndexOf(item));
            }
        }
    }
    public static class SortableObservableCollectionExtension
    {
        public static SortableObservableCollection<TSource> ToSortableObservableCollection<TSource>(this IEnumerable<TSource> _collection)
        {
            SortableObservableCollection<TSource> res = new SortableObservableCollection<TSource>();
            res.AddRange(_collection);
            return res;
        }
        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> _collection)
        {
            List<TSource> res = new List<TSource>();
            res.AddRange(_collection);
            return res;
        }
    }
}
