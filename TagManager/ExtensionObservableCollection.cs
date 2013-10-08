﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace TagPan
{
    /// <summary> 
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {

        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). 
        /// </summary> 
        public void RemoveRange(IEnumerable<T> collection)
        {
            foreach (var i in collection) Items.Remove(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, collection));
        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified item. 
        /// </summary> 
        public void Replace(T item)
        {
            ReplaceRange(new T[] { item });
        }
        /// <summary> 
        /// Clears the current collection and replaces it with the specified collection. 
        /// </summary> 
        public void ReplaceRange(IEnumerable<T> collection)
        {
            List<T> old = new List<T>(Items);
            Items.Clear();
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, collection, old, 0));
        }
        public void RaiseCollectionChanged()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public ObservableCollection()
            : base() { }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public ObservableCollection(IEnumerable<T> collection)
            : base(collection) { }
    }
}
