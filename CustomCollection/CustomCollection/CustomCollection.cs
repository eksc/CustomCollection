using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CustomCollection.CustomCollection
{
    internal class CustomCollection<TId, TName, TItem> : ICollection<KeyValuePair<CustomKeyPair<TId, TName>, TItem>>
    {
        private readonly Dictionary<CustomKeyPair<TId, TName>, TItem> _items = new Dictionary<CustomKeyPair<TId, TName>, TItem>();
        private readonly Dictionary<TId, List<TItem>> _idItems = new Dictionary<TId, List<TItem>>();
        private readonly Dictionary<TName, List<TItem>> _nameItems = new Dictionary<TName, List<TItem>>();
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public CustomCollection() { }

        /// <summary>
        /// Create new collection with element
        /// </summary>
        /// <param name="id">Key ID</param>
        /// <param name="name">Key Name</param>
        /// <param name="item">Item in collection</param>
        /// <exception cref="ArgumentNullException">CustomKeyPair or Id or Name is Null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        public CustomCollection(TId id, TName name, TItem item)
        {
            AddWithCustomKeyPair(new CustomKeyPair<TId, TName>(id, name), item);
        }

        /// <summary>
        /// Create new collection with element
        /// </summary>
        /// <param name="customKeyPair">Pair keys ID and Name</param>
        /// <param name="item">Item in collection</param>
        /// <exception cref="ArgumentNullException">CustomKeyPair or Id or Name is Null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        public CustomCollection(CustomKeyPair<TId, TName> customKeyPair, TItem item)
        {
            AddWithCustomKeyPair(customKeyPair, item);
        }

        public int Count => this._items.Count;
        public Dictionary<CustomKeyPair<TId, TName>, TItem> Items => this._items;
        public bool IsReadOnly => false;

        public TItem this[TId id, TName name]
        {
            get => GetValue(id, name);
            set => SetValue(id, name, value);
        }

        /// <summary>
        /// Add new element in collection
        /// </summary>
        /// <param name="id">Key ID</param>
        /// <param name="name">Key Name</param>
        /// <param name="item">Value in collection</param>
        /// <exception cref="ArgumentNullException">CustomKeyPair or Id or Name is Null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        public void Add(TId id, TName name, TItem item)
        {
            AddWithCustomKeyPair(new CustomKeyPair<TId, TName>(id, name), item);
        }

        /// <summary>
        /// Add new element in collection
        /// </summary>
        /// <param name="item">Pair Keys and Item</param>
        /// <exception cref="ArgumentNullException">CustomKeyPair or Id or Name is Null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        public void Add(KeyValuePair<CustomKeyPair<TId, TName>, TItem> item)
        {
            AddWithCustomKeyPair(item.Key, item.Value);
        }

        /// <summary>
        /// Determines whether the CustomCollection.CustomCollection contains a specific item.
        /// </summary>
        /// <param name="item">Pair Keys and Item</param>
        /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Item is null</exception>
        public bool Contains(KeyValuePair<CustomKeyPair<TId, TName>, TItem> item)
        {
            this.locker.EnterReadLock();
            try
            {
                return this._items.Contains(item);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Copies the elements of the CustomCollection.CustomCollection to an System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from CustomCollection.CustomCollection. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex out of range</exception>
        /// <exception cref="ArgumentException">Count elements in source collection more than available from arrayIndex in array</exception>
        public void CopyTo(KeyValuePair<CustomKeyPair<TId, TName>, TItem>[] array, int arrayIndex)
        {
            if (array is null)
                throw new ArgumentNullException("array");
            if (arrayIndex > array.Length || arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("The number of elements in the source CustomCollection.CustomCollection is greater than the available space from arrayIndex to the end of the destination array.");

            this.locker.EnterReadLock();
            try
            {
                foreach (var item in _items)
                {
                    array[arrayIndex++] = new KeyValuePair<CustomKeyPair<TId, TName>, TItem>(item.Key, item.Value);
                }
            }
            finally
            {
                this.locker.ExitReadLock();
            }


        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the CustomCollection.CustomCollection.
        /// </summary>
        /// <param name="item">Pair Keys and Item</param>
        /// <returns>true if item was successfully removed from the CustomCollection.CustomCollection; otherwise, false. This method also returns false if item is not found in the original CustomCollection.CustomCollection.</returns>
        /// <exception cref="ArgumentNullException">Dictionary or Key is null</exception>
        /// <exception cref="Exception">Item not exists in dictionary</exception>
        public bool Remove(KeyValuePair<CustomKeyPair<TId, TName>, TItem> item)
        {
            this.locker.EnterWriteLock();
            try
            {
                if (this._items.ContainsKey(item.Key) &&
                    this._idItems.ContainsKey(item.Key.Id) &&
                    this._nameItems.ContainsKey(item.Key.Name))
                {
                    return this._items.Remove(item.Key) &&
                           RemoveItemFromHelperDictionary(this._idItems, item.Key.Id, item.Value) &&
                           RemoveItemFromHelperDictionary(this._nameItems, item.Key.Name, item.Value);
                }
                else return false;
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Search items by key Id
        /// </summary>
        /// <param name="id">Key ID</param>
        /// <returns>Return readonly collection with items by id</returns>
        /// <exception cref="ArgumentNullException">Key Id is null</exception>
        public IReadOnlyCollection<TItem> GetById(TId id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            this.locker.EnterReadLock();
            try
            {
                return this._idItems[id];
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Search items by key Name
        /// </summary>
        /// <param name="name">Key Name</param>
        /// <returns>Return readonly collection with items by name</returns>
        /// <exception cref="ArgumentNullException">Key Name is null</exception>
        public IReadOnlyCollection<TItem> GetByName(TName name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.locker.EnterReadLock();
            try
            {
                return this._nameItems[name];
            }
            finally
            {
                this.locker.ExitReadLock();
            }

        }

        /// <summary>
        /// Clear CustomCollection.CustomCollection
        /// </summary>
        public void Clear()
        {
            this.locker.EnterWriteLock();
            try
            {
                this._items.Clear();
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<CustomKeyPair<TId, TName>, TItem>> GetEnumerator()
        {
            this.locker.EnterReadLock();
            try
            {
                return this._items.ToList().GetEnumerator();
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Get Value in collection by Id and Name
        /// </summary>
        /// <param name="id">Key ID</param>
        /// <param name="name">Key Name</param>
        /// <returns>Return TItem if element with Pair Keys contains in Collection</returns>
        /// <exception cref="ArgumentNullException">Id or Name is Null</exception>
        private TItem GetValue(TId id, TName name)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (name == null)
                throw new ArgumentNullException("name");

            var key = new CustomKeyPair<TId, TName>(id, name);

            this.locker.EnterReadLock();

            try
            {
                if (this._items.ContainsKey(key))
                {
                    return this._items[key];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Set value if element contains in Collection, else create new with pair keys Id and Name
        /// </summary>
        /// <param name="id">Key ID</param>
        /// <param name="name">Key Name</param>
        /// <param name="value">Item to set</param>
        /// <exception cref="ArgumentNullException">Id or Name is Null</exception>
        /// <exception cref="Exception">Item not exists in dictionary</exception>
        private void SetValue(TId id, TName name, TItem value)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (name == null)
                throw new ArgumentNullException("name");

            var key = new CustomKeyPair<TId, TName>(id, name);

            this.locker.EnterWriteLock();

            try
            {
                ChangeItemInHelperDictionary(this._idItems, key.Id, this._items[key], value);
                ChangeItemInHelperDictionary(this._nameItems, key.Name, this._items[key], value);

                this._items[key] = value;

            }
            finally
            {
                this.locker.ExitWriteLock();
            }

        }

        /// <summary>
        /// Local function to add new element in Collection
        /// </summary>
        /// <param name="customKeyPair">Pair keys Id and Name</param>
        /// <param name="item">New item in collection</param>
        /// <exception cref="ArgumentNullException">CustomKeyPair or Id or Name is Null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        private void AddWithCustomKeyPair(CustomKeyPair<TId, TName> customKeyPair, TItem item)
        {
            if (customKeyPair == null)
                throw new ArgumentNullException("customKeyPair");
            if (customKeyPair.Id == null)
                throw new ArgumentNullException("id");
            if (customKeyPair.Name == null)
                throw new ArgumentNullException("name");
            if (this._items.ContainsKey(customKeyPair))
                throw new Exception($"Item with key «{customKeyPair.Id}, {customKeyPair.Name}» already exists");

            this.locker.EnterWriteLock();

            try
            {
                this._items.Add(customKeyPair, item);
                AddToHelperIdDictionary(customKeyPair.Id, item);
                AddToHelperNameDictionary(customKeyPair.Name, item);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }

        }

        /// <summary>
        /// Add item to _idItems dictionary
        /// </summary>
        /// <param name="id">Key ID</param>
        /// <param name="item">New item in dictionary</param>
        /// <exception cref="ArgumentNullException">Dictionary or Key is null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        private void AddToHelperIdDictionary(TId id, TItem item)
        {
            AddToSimpleDictionary(this._idItems, id, item);
        }

        /// <summary>
        /// Add item to _nameItems dictionary
        /// </summary>
        /// <param name="id">Key Name</param>
        /// <param name="item">New item in dictionary</param>
        /// <exception cref="ArgumentNullException">Dictionary or Key is null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        private void AddToHelperNameDictionary(TName name, TItem item)
        {
            AddToSimpleDictionary(this._nameItems, name, item);
        }

        /// <summary>
        /// General method add new item dictionary
        /// </summary>
        /// <typeparam name="TKey">Key</typeparam>
        /// <param name="dictionary">Dictionary in which add new item</param>
        /// <param name="key">Key in collection</param>
        /// <param name="item">New item in dictionary</param>
        /// <exception cref="ArgumentNullException">Dictionary or Key is null</exception>
        /// <exception cref="Exception">Item already exists</exception>
        private void AddToSimpleDictionary<TKey>(Dictionary<TKey, List<TItem>> dictionary, TKey key, TItem item)
        {
            if (dictionary is null)
                throw new ArgumentNullException("dictionary");
            if (key == null)
                throw new ArgumentNullException("key");
            if (dictionary.ContainsKey(key))
                throw new Exception($"Item with key «{key}» already exists");

            if (dictionary.ContainsKey(key))
                dictionary[key].Add(item);
            else
                dictionary.Add(key, new List<TItem>() { item });
        }

        /// <summary>
        /// General method remove item from dictionary
        /// </summary>
        /// <typeparam name="TKey">Key</typeparam>
        /// <param name="dictionary">Dictionary in which remove item</param>
        /// <param name="key">Key in collection</param>
        /// <param name="item">Remove item in dictionary</param>
        /// <exception cref="ArgumentNullException">Dictionary or Key is null</exception>
        /// <exception cref="Exception">Item not exists in dictionary</exception>
        private bool RemoveItemFromHelperDictionary<TKey>(Dictionary<TKey, List<TItem>> dictionary, TKey key, TItem item)
        {
            if (dictionary is null)
                throw new ArgumentNullException("dictionary");
            if (key == null)
                throw new ArgumentNullException("key");
            if (!dictionary.ContainsKey(key))
                throw new Exception($"Item with key «{key}» not exists");

            if (dictionary[key].Count == 1)
                return dictionary.Remove(key);
            else
                return dictionary[key].Remove(item);
        }

        /// <summary>
        /// General method change item in dictionary
        /// </summary>
        /// <typeparam name="TKey">Key</typeparam>
        /// <param name="dictionary">Dictionary in which change item</param>
        /// <param name="key">Key in collection</param>
        /// <param name="oldItem">Removed item in dictionary</param>
        /// <param name="newItem">Add item in dictionary</param>
        /// <exception cref="ArgumentNullException">Dictionary or Key is null</exception>
        /// <exception cref="Exception">Item not exists in dictionary</exception>
        private void ChangeItemInHelperDictionary<TKey>(Dictionary<TKey, List<TItem>> dictionary, TKey key, TItem oldItem, TItem newItem)
        {
            if (dictionary is null)
                throw new ArgumentNullException("dictionary");
            if (key == null)
                throw new ArgumentNullException("key");
            if (!dictionary.ContainsKey(key))
                throw new Exception($"Item with key «{key}» not exists");

            dictionary[key].Remove(oldItem);
            dictionary[key].Add(newItem);
        }

    }
}
