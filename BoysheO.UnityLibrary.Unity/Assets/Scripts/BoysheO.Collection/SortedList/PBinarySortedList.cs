using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BoysheO.Collection
{
    // The SortedDictionary class implements a generic sorted list of keys
    // and values. Entries in a sorted list are sorted by their keys and
    // are accessible both by key and by index. The keys of a sorted dictionary
    // can be ordered either according to a specific IComparer
    // implementation given when the sorted dictionary is instantiated, or
    // according to the IComparable implementation provided by the keys
    // themselves. In either case, a sorted dictionary does not allow entries
    // with duplicate or null keys.
    //
    // A sorted list internally maintains two arrays that store the keys and
    // values of the entries. The capacity of a sorted list is the allocated
    // length of these internal arrays. As elements are added to a sorted list, the
    // capacity of the sorted list is automatically increased as required by
    // reallocating the internal arrays.  The capacity is never automatically
    // decreased, but users can call either TrimExcess or
    // Capacity explicitly.
    //
    // The GetKeyList and GetValueList methods of a sorted list
    // provides access to the keys and values of the sorted list in the form of
    // List implementations. The List objects returned by these
    // methods are aliases for the underlying sorted list, so modifications
    // made to those lists are directly reflected in the sorted list, and vice
    // versa.
    //
    // The SortedList class provides a convenient way to create a sorted
    // copy of another dictionary, such as a Hashtable. For example:
    //
    // Hashtable h = new Hashtable();
    // h.Add(...);
    // h.Add(...);
    // ...
    // SortedList s = new SortedList(h);
    //
    // The last line above creates a sorted list that contains a copy of the keys
    // and values stored in the hashtable. In this particular example, the keys
    // will be ordered according to the IComparable interface, which they
    // all must implement. To impose a different ordering, SortedList also
    // has a constructor that allows a specific IComparer implementation to
    // be specified.
    //
    /// <summary>
    /// *特别注意，如果传入的Compare不能保证正确性(指根据compare降序排序后的列表，任意表中元素比它的前面的元素大，比它后面的元素小，如不满足此条件，则一定会出bug)，
    /// 则二分查找算法失效，此类将不能正常使用。
    /// *Notice This type can not work with incorrect comparer.There is no exception throw.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [DebuggerTypeProxy(typeof(IDictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class PBinarySortedList<TKey, TValue> :
        IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly PList<TKey> keys; // Do not rename (binary serialization)
        private readonly PList<TValue> values; // Do not rename (binary serialization)
        // private int _size; // Do not rename (binary serialization)
        // private int version; // Do not rename (binary serialization)
        private IComparer<TKey> _comparer; // Do not rename (binary serialization)

        private KeyList? keyList; // Do not rename (binary serialization)
        private ValueList? valueList; // Do not rename (binary serialization)

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to DefaultCapacity, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        public PBinarySortedList()
        {
            keys = new PList<TKey>();
            values = new PList<TValue>();
            _comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to 16, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        //
        public PBinarySortedList(int capacity)
        {
            if (capacity < 0) ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(nameof(capacity));
            keys = new PList<TKey>(capacity);
            values = new PList<TValue>(capacity);
            _comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list with a given IComparer
        // implementation. The sorted list is initially empty and has a capacity of
        // zero. Upon adding the first element to the sorted list the capacity is
        // increased to 16, and then increased in multiples of two as required. The
        // elements of the sorted list are ordered according to the given
        // IComparer implementation. If comparer is null, the
        // elements are compared to each other using the IComparable
        // interface, which in that case must be implemented by the keys of all
        // entries added to the sorted list.
        //
        public PBinarySortedList(IComparer<TKey>? comparer)
            : this()
        {
            if (comparer != null)
            {
                this._comparer = comparer;
            }
        }

        // Constructs a new sorted dictionary with a given IComparer
        // implementation and a given initial capacity. The sorted list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required. The elements of the sorted list
        // are ordered according to the given IComparer implementation. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by the keys of all entries added to the sorted list.
        //
        public PBinarySortedList(int capacity, IComparer<TKey>? comparer)
            : this(comparer)
        {
            Capacity = capacity;
        }

        // // Constructs a new sorted list containing a copy of the entries in the
        // // given dictionary. The elements of the sorted list are ordered according
        // // to the IComparable interface, which must be implemented by the
        // // keys of all entries in the given dictionary as well as keys
        // // subsequently added to the sorted list.
        // //
        // public SortedList(IDictionary<TKey, TValue> dictionary)
        //     : this(dictionary, null)
        // {
        // }

        // // Constructs a new sorted list containing a copy of the entries in the
        // // given dictionary. The elements of the sorted list are ordered according
        // // to the given IComparer implementation. If comparer is
        // // null, the elements are compared to each other using the
        // // IComparable interface, which in that case must be implemented
        // // by the keys of all entries in the given dictionary as well as keys
        // // subsequently added to the sorted list.
        // //
        // public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey>? comparer)
        //     : this(dictionary?.Count ?? throw new ArgumentNullException(nameof(dictionary)), comparer)
        // {
        //     int count = dictionary.Count;
        //     if (count != 0)
        //     {
        //         TKey[] keys = this.keys.Span;
        //         dictionary.Keys.CopyTo(keys, 0);
        //         dictionary.Values.CopyTo(values, 0);
        //         Debug.Assert(count == this.keys.Length);
        //         if (count > 1)
        //         {
        //             comparer = Comparer; // obtain default if this is null.
        //             Array.Sort<TKey, TValue>(keys, values, comparer);
        //             for (int i = 1; i < keys.Length; ++i)
        //             {
        //                 if (comparer.Compare(keys[i - 1], keys[i]) == 0)
        //                 {
        //                     throw new ArgumentException(SR.Format(SR.Argument_AddingDuplicate, keys[i]));
        //                 }
        //             }
        //         }
        //     }
        //
        //     _size = count;
        // }

        // Adds an entry with the given key and value to this sorted list. An
        // ArgumentException is thrown if the key is already present in the sorted list.
        //
        public void Add(TKey key, TValue value)
        {
            if(key == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            // int i = Array.BinarySearch<TKey>(keys.Span, 0, _size, key, comparer);
            int i = keys.BinarySearch(key, _comparer);
            if (i >= 0)
                throw new ArgumentException(SR.Format(SR.Argument_AddingDuplicate, key), nameof(key));
            Insert(~i, key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index], keyValuePair.Value))
            {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index], keyValuePair.Value))
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        // Returns the capacity of this sorted list. The capacity of a sorted list
        // represents the allocated length of the internal arrays used to store the
        // keys and values of the list, and thus also indicates the maximum number
        // of entries the list can contain before a reallocation of the internal
        // arrays is required.
        //
        public int Capacity
        {
            get
            {
                return keys.Capacity;
            }
            set
            {
                keys.Capacity = value;
                values.Capacity = value;
            }
        }

        [NotNull]
        public IComparer<TKey> Comparer
        {
            get
            {
                return _comparer;
            }
        }

        void IDictionary.Add(object key, object? value)
        {
            if(key == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);

            if (value == null && default(TValue) != null)    // null is an invalid value for Value types
                throw new ArgumentNullException(nameof(value));

            if (!(key is TKey))
                throw new ArgumentException(SR.Format(SR.Arg_WrongType, key, typeof(TKey)), nameof(key));

            if (!(value is TValue) && value != null)            // null is a valid value for Reference Types
                throw new ArgumentException(SR.Format(SR.Arg_WrongType, value, typeof(TValue)), nameof(value));

            Add((TKey)key, (TValue)value!);
        }

        // Returns the number of entries in this sorted list.
        public int Count
        {
            get
            {
                return keys.Count;
            }
        }

        // Returns a collection representing the keys of this sorted list. This
        // method returns the same object as GetKeyList, but typed as an
        // ICollection instead of an IList.
        public IReadOnlyList<TKey> Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        // Returns a collection representing the values of this sorted list. This
        // method returns the same object as GetValueList, but typed as an
        // ICollection instead of an IList.
        //
        public IList<TValue> Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        private KeyList GetKeyListHelper() => keyList ??= new KeyList(this);

        private ValueList GetValueListHelper() => valueList ??= new ValueList(this);

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        // Synchronization root for this object.
        object ICollection.SyncRoot => this;

        // Removes all entries from this sorted list.
        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
            {
                return ContainsKey((TKey)key);
            }
            return false;
        }

        // Checks if this sorted list contains an entry with the given key.
        public bool ContainsKey(TKey key)
        {
            return IndexOfKey(key) >= 0;
        }

        // Checks if this sorted list contains an entry with the given value. The
        // values of the entries of the sorted list are compared to the given value
        // using the Object.Equals method. This method performs a linear
        // search and is substantially slower than the Contains
        // method.
        public bool ContainsValue(TValue value)
        {
            return IndexOfValue(value) >= 0;
        }

        // Copies the values in this SortedList to an array.
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if(array == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, SR.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
            }

            for (int i = 0; i < Count; i++)
            {
                KeyValuePair<TKey, TValue> entry = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                array[arrayIndex + i] = entry;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if(array == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.Arg_RankMultiDimNotSupported, nameof(array));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException(SR.Arg_NonZeroLowerBound, nameof(array));
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
            }

            if (array is KeyValuePair<TKey, TValue>[] keyValuePairArray)
            {
                for (int i = 0; i < Count; i++)
                {
                    keyValuePairArray[i + index] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                }
            }
            else
            {
                object[]? objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException(SR.Argument_IncompatibleArrayType, nameof(array));
                }

                try
                {
                    for (int i = 0; i < Count; i++)
                    {
                        objects[i + index] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(SR.Argument_IncompatibleArrayType, nameof(array));
                }
            }
        }

        // Ensures that the capacity of this sorted list is at least the given
        // minimum value. The capacity is increased to twice the current capacity
        // or to min, whichever is larger.
        private void EnsureCapacity(int min)
        {
            keys.EnsureCapacity(min);
            values.EnsureCapacity(min);
        }

        /// <summary>
        /// Gets the value corresponding to the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value within the entire <see cref="PBinarySortedList{TKey,TValue}"/>.</param>
        /// <returns>The value corresponding to the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified index was out of range.</exception>
        public TValue GetValueAtIndex(int index)
        {
            // if (index < 0 || index >= keys.Count)
                // throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_IndexMustBeLess);
            return values[index];
        }

        /// <summary>
        /// Updates the value corresponding to the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value within the entire <see cref="PBinarySortedList{TKey,TValue}"/>.</param>
        /// <param name="value">The value with which to replace the entry at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified index was out of range.</exception>
        public void SetValueAtIndex(int index, TValue value)
        {
            values[index] = value;
            // if (index < 0 || index >= _size)
                // throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_IndexMustBeLess);
            // values[index] = value;
            // version++;
        }

        public Enumerator GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
            Count == 0 ? CollectionHelper<KeyValuePair<TKey,TValue>>.EmptyEnumerator :
            GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();

        /// <summary>
        /// Gets the key corresponding to the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the key within the entire <see cref="PBinarySortedList{TKey,TValue}"/>.</param>
        /// <returns>The key corresponding to the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified index is out of range.</exception>
        public TKey GetKeyAtIndex(int index)
        {
            // if (index < 0 || index >= _size)
                // throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_IndexMustBeLess);
            return keys[index];
        }

        // Returns the value associated with the given key. If an entry with the
        // given key is not found, the returned value is null.
        public TValue this[TKey key]
        {
            get
            {
                int i = IndexOfKey(key);
                if (i >= 0)
                    return values[i];

                throw new KeyNotFoundException(SR.Format(SR.Arg_KeyNotFoundWithKey, key.ToString()));
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                // int i = Array.BinarySearch<TKey>(keys, 0, _size, key, comparer);
                int i = keys.BinarySearch(key, _comparer);
                if (i >= 0)
                {
                    values[i] = value;
                    return;
                }
                Insert(~i, key, value);
            }
        }

        object? IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    int i = IndexOfKey((TKey)key);
                    if (i >= 0)
                    {
                        return values[i];
                    }
                }

                return null;
            }
            set
            {
                if (!IsCompatibleKey(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null && default(TValue) != null)
                    throw new ArgumentNullException(nameof(value));

                TKey tempKey = (TKey)key;
                try
                {
                    this[tempKey] = (TValue)value!;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException(SR.Format(SR.Arg_WrongType, value, typeof(TValue)), nameof(value));
                }
            }
        }

        // Returns the index of the entry with a given key in this sorted list. The
        // key is located through a binary search, and thus the average execution
        // time of this method is proportional to Log2(size), where
        // size is the size of this sorted list. The returned value is -1 if
        // the given key does not occur in this sorted list. Null is an invalid
        // key value.
        public int IndexOfKey(TKey key)
        {
            if(key == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);

            // int ret = Array.BinarySearch<TKey>(keys, 0, _size, key, comparer);
            int ret = keys.Span.BinarySearch(key, _comparer);
            return ret >= 0 ? ret : -1;
        }

        // Returns the index of the first occurrence of an entry with a given value
        // in this sorted list. The entry is located through a linear search, and
        // thus the average execution time of this method is proportional to the
        // size of this sorted list. The elements of the list are compared to the
        // given value using the Object.Equals method.
        public int IndexOfValue(TValue value)
        {
            return values.IndexOf(value);
            // return Array.IndexOf(values, value, 0, _size);
        }

        // Inserts an entry with a given key and value at a given index.
        private void Insert(int index, TKey key, TValue value)
        {
            keys.Insert(index,key);
            values.Insert(index,value);
        }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            int i = IndexOfKey(key);
            if (i >= 0)
            {
                value = values[i];
                return true;
            }

            value = default;
            return false;
        }

        // Removes the entry at the given index. The size of the sorted list is
        // decreased by one.
        public void RemoveAt(int index)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        // Removes an entry from this sorted list. If an entry with the specified
        // key exists in the sorted list, it is removed. An ArgumentException is
        // thrown if the key is null.
        public bool Remove(TKey key)
        {
            int i = IndexOfKey(key);
            if (i >= 0)
                RemoveAt(i);
            return i >= 0;
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        // Sets the capacity of this sorted list to the size of the sorted list.
        // This method can be used to minimize a sorted list's memory overhead once
        // it is known that no new elements will be added to the sorted list. To
        // completely clear a sorted list and release all memory referenced by the
        // sorted list, execute the following statements:
        //
        // SortedList.Clear();
        // SortedList.TrimExcess();
        public void TrimExcess()
        {
            keys.TrimExcess();
            values.TrimExcess();
            // int threshold = (int)(keys.Length * 0.9);
            // if (_size < threshold)
            // {
                // Capacity = _size;
            // }
        }

        private static bool IsCompatibleKey(object key)
        {
            if(key == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);

            return (key is TKey);
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly PBinarySortedList<TKey, TValue> _binarySortedList;
            private TKey? _key;
            private TValue? _value;
            private int _index;
            private readonly int _version;
            private readonly int _getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int KeyValuePair = 1;
            internal const int DictEntry = 2;

            internal Enumerator(PBinarySortedList<TKey, TValue> binarySortedList, int getEnumeratorRetType)
            {
                _binarySortedList = binarySortedList;
                _index = 0;
                _version = _binarySortedList.keys.Version;
                _getEnumeratorRetType = getEnumeratorRetType;
                _key = default;
                _value = default;
            }

            public void Dispose()
            {
                _index = 0;
                _key = default;
                _value = default;
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (_index == 0 || (_index == _binarySortedList.Count + 1))
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }

                    return _key!;
                }
            }

            public bool MoveNext()
            {
                if (_version != _binarySortedList.keys.Version) throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);

                if ((uint)_index < (uint)_binarySortedList.Count)
                {
                    _key = _binarySortedList.keys[_index];
                    _value = _binarySortedList.values[_index];
                    _index++;
                    return true;
                }

                _index = _binarySortedList.Count + 1;
                _key = default;
                _value = default;
                return false;
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (_index == 0 || (_index == _binarySortedList.Count + 1))
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }

                    return new DictionaryEntry(_key!, _value);
                }
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_key!, _value!);

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _binarySortedList.Count + 1))
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }

                    if (_getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(_key!, _value);
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>(_key!, _value!);
                    }
                }
            }

            object? IDictionaryEnumerator.Value
            {
                get
                {
                    if (_index == 0 || (_index == _binarySortedList.Count + 1))
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }

                    return _value;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _binarySortedList.keys.Version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }

                _index = 0;
                _key = default;
                _value = default;
            }
        }

        private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, IEnumerator
        {
            private readonly PBinarySortedList<TKey, TValue> _binarySortedList;
            private int _index;
            private readonly int _version;
            private TKey? _currentKey;

            internal SortedListKeyEnumerator(PBinarySortedList<TKey, TValue> binarySortedList)
            {
                _binarySortedList = binarySortedList;
                _version = binarySortedList.keys.Version;
            }

            public void Dispose()
            {
                _index = 0;
                _currentKey = default;
            }

            public bool MoveNext()
            {
                if (_version != _binarySortedList.keys.Version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }

                if ((uint)_index < (uint)_binarySortedList.Count)
                {
                    _currentKey = _binarySortedList.keys[_index];
                    _index++;
                    return true;
                }

                _index = _binarySortedList.Count + 1;
                _currentKey = default;
                return false;
            }

            public TKey Current => _currentKey!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _binarySortedList.Count + 1))
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }

                    return _currentKey;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _binarySortedList.keys.Version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }
                _index = 0;
                _currentKey = default;
            }
        }

        private sealed class SortedListValueEnumerator : IEnumerator<TValue>, IEnumerator
        {
            private readonly PBinarySortedList<TKey, TValue> _binarySortedList;
            private int _index;
            private readonly int _version;
            private TValue? _currentValue;

            internal SortedListValueEnumerator(PBinarySortedList<TKey, TValue> binarySortedList)
            {
                _binarySortedList = binarySortedList;
                _version = binarySortedList.keys.Version;
            }

            public void Dispose()
            {
                _index = 0;
                _currentValue = default;
            }

            public bool MoveNext()
            {
                if (_version != _binarySortedList.keys.Version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }

                if ((uint)_index < (uint)_binarySortedList.Count)
                {
                    _currentValue = _binarySortedList.values[_index];
                    _index++;
                    return true;
                }

                _index = _binarySortedList.Count + 1;
                _currentValue = default;
                return false;
            }

            public TValue Current => _currentValue!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _binarySortedList.Count + 1))
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }

                    return _currentValue;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _binarySortedList.keys.Version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }
                _index = 0;
                _currentValue = default;
            }
        }

        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        [Serializable]
        public sealed class KeyList : IList<TKey>, ICollection,IReadOnlyList<TKey>
        {
            private readonly PBinarySortedList<TKey, TValue> _dict; // Do not rename (binary serialization)

            internal KeyList(PBinarySortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            public int Count
            {
                get { return _dict.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            public void Add(TKey key)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey key)
            {
                return _dict.ContainsKey(key);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                // defer error checking to Array.Copy
                _dict.keys.CopyTo(array,arrayIndex);
            }

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException(nameof(array));
                if (array != null && array.Rank != 1)
                    throw new ArgumentException(SR.Arg_RankMultiDimNotSupported, nameof(array));

                try
                {
                    // defer error checking to Array.Copy
                    // Array.Copy(_dict.keys, 0, array!, arrayIndex, _dict.Count);
                    for (int i = 0; i < _dict.Count; i++)
                    {
                        array!.SetValue(_dict.keys[i],arrayIndex+i);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(SR.Argument_IncompatibleArrayType, nameof(array));
                }
            }

            public void Insert(int index, TKey value)
            {
                throw new NotSupportedException();
            }

            public TKey this[int index]
            {
                get
                {
                    return _dict.GetKeyAtIndex(index);
                }
                set
                {
                    throw new NotSupportedException(SR.NotSupported_KeyCollectionSet);
                }
            }

            public IEnumerator<TKey> GetEnumerator() =>
                Count == 0 ? CollectionHelper<TKey>.EmptyEnumerator :
                new SortedListKeyEnumerator(_dict);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int IndexOf(TKey key)
            {
                if(key == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);

                var i = _dict.IndexOfKey(key);
                // int i = Array.BinarySearch<TKey>(_dict.keys, 0,
                //                           _dict.Count, key, _dict.comparer);
                if (i >= 0) return i;
                return -1;
            }

            public bool Remove(TKey key)
            {
                throw new NotSupportedException();
                // return false;
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
        }

        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        [Serializable]
        public sealed class ValueList : IList<TValue>, ICollection
        {
            private readonly PBinarySortedList<TKey, TValue> _dict; // Do not rename (binary serialization)

            internal ValueList(PBinarySortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            public int Count
            {
                get { return _dict.keys.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            public void Add(TValue key)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TValue value)
            {
                return _dict.ContainsValue(value);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                // defer error checking to Array.Copy
                _dict.values.CopyTo(array,arrayIndex);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null) throw new ArgumentNullException(nameof(array));
                if (array != null && array.Rank != 1)
                    throw new ArgumentException(SR.Arg_RankMultiDimNotSupported, nameof(array));

                try
                {
                    // defer error checking to Array.Copy
                    // Array.Copy(_dict.values, 0, array!, index, _dict.Count);
                    for (int i = 0; i < _dict.Count; i++)
                    {
                        array!.SetValue(_dict.values[i],index + i);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(SR.Argument_IncompatibleArrayType, nameof(array));
                }
            }

            public void Insert(int index, TValue value)
            {
                throw new NotSupportedException();
            }

            public TValue this[int index]
            {
                get
                {
                    return _dict.GetValueAtIndex(index);
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            public IEnumerator<TValue> GetEnumerator() =>
                Count == 0 ? CollectionHelper<TValue>.EmptyEnumerator:
                new SortedListValueEnumerator(_dict);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int IndexOf(TValue value)
            {
                return _dict.values.IndexOf(value);
                // return Array.IndexOf(_dict.values, value, 0, _dict.Count);
            }

            public bool Remove(TValue value)
            {
                throw new NotSupportedException();
                // return false;
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
        }
    }
}
