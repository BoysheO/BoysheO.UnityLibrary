using System;
using System.Collections;
using System.Collections.Generic;
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable CS8632 

namespace BoysheO.Collection
{
// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.Dictionary`2
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: A3B02D6F-9B49-4355-B946-095EA1F25C54
// Assembly location: E:\UnityEditor\2022.3.14f1c1\Editor\Data\MonoBleedingEdge\lib\mono\unityjit-win32\mscorlib.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.xml
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

#nullable disable

    //经测试，效能与Unity的Dictionary实现基本一致
    /// <summary>Represents a collection of keys and values.</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [DebuggerTypeProxy(typeof(IDictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class PDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDictionary,
        ICollection,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        private PList<int> _buckets;
        private PList<PDictionary<TKey, TValue>.Entry> _entries;
        private int _count;
        private int _freeList;
        private int _freeCount;
        private int _version;
        private IEqualityComparer<TKey>? _comparer;
        private PDictionary<TKey, TValue>.KeyCollection _keys;
        private PDictionary<TKey, TValue>.ValueCollection _values;
        private object _syncRoot;
        private const string VersionName = "Version";
        private const string HashSizeName = "HashSize";
        private const string KeyValuePairsName = "KeyValuePairs";
        private const string ComparerName = "Comparer";

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.</summary>
        public PDictionary()
            : this(0, (IEqualityComparer<TKey>) null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.</summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="capacity" /> is less than 0.</exception>
        public PDictionary(int capacity)
            : this(capacity, (IEqualityComparer<TKey>) null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
        public PDictionary(IEqualityComparer<TKey>? comparer)
            : this(0, comparer)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="capacity" /> is less than 0.</exception>
        public PDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
            if (capacity > 0)
                this.Initialize(capacity);
            if (comparer == EqualityComparer<TKey>.Default)
                return;
            this._comparer = comparer;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default equality comparer for the key type.</summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="dictionary" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
        public PDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, (IEqualityComparer<TKey>) null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="dictionary" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
        public PDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this(dictionary != null ? dictionary.Count : 0, comparer)
        {
            if (dictionary == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
            if (dictionary.GetType() == typeof(PDictionary<TKey, TValue>))
            {
                PDictionary<TKey, TValue> dictionary1 = (PDictionary<TKey, TValue>) dictionary;
                int count = dictionary1._count;
                var entries = dictionary1._entries;
                for (int index = 0; index < count; ++index)
                {
                    if (entries[index].hashCode >= 0)
                        this.Add(entries[index].key, entries[index].value);
                }
            }
            else
            {
                foreach (KeyValuePair<TKey, TValue> keyValuePair in
                         (IEnumerable<KeyValuePair<TKey, TValue>>) dictionary)
                    this.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public PDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, (IEqualityComparer<TKey>) null)
        {
        }

        public PDictionary(
            IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer)
            : this(collection is ICollection<KeyValuePair<TKey, TValue>> keyValuePairs ? keyValuePairs.Count : 0,
                comparer)
        {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            foreach (KeyValuePair<TKey, TValue> keyValuePair in collection)
                this.Add(keyValuePair.Key, keyValuePair.Value);
        }

        /// <summary>Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> that is used to determine equality of keys for the dictionary.</summary>
        /// <returns>The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface implementation that is used to determine equality of keys for the current <see cref="T:System.Collections.Generic.Dictionary`2" /> and to provide hash values for the keys.</returns>
        public IEqualityComparer<TKey>? Comparer
        {
            get
            {
                return this._comparer != null
                    ? this._comparer
                    : (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default;
            }
        }

        /// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
        /// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        public int Count => this._count - this._freeCount;

        /// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        public PDictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                if (this._keys == null)
                    this._keys = new PDictionary<TKey, TValue>.KeyCollection(this);
                return this._keys;
            }
        }

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of type <paramref name="TKey" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (this._keys == null)
                    this._keys = new PDictionary<TKey, TValue>.KeyCollection(this);
                return (ICollection<TKey>) this._keys;
            }
        }

        /// <summary>Gets a collection containing the keys of the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" />.</summary>
        /// <returns>A collection containing the keys of the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" />.</returns>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (this._keys == null)
                    this._keys = new PDictionary<TKey, TValue>.KeyCollection(this);
                return (IEnumerable<TKey>) this._keys;
            }
        }

        /// <summary>Gets a collection containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        public PDictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                if (this._values == null)
                    this._values = new PDictionary<TKey, TValue>.ValueCollection(this);
                return this._values;
            }
        }

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of type <paramref name="TValue" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if (this._values == null)
                    this._values = new PDictionary<TKey, TValue>.ValueCollection(this);
                return (ICollection<TValue>) this._values;
            }
        }

        /// <summary>Gets a collection containing the values of the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" />.</summary>
        /// <returns>A collection containing the values of the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" />.</returns>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                if (this._values == null)
                    this._values = new PDictionary<TKey, TValue>.ValueCollection(this);
                return (IEnumerable<TValue>) this._values;
            }
        }

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                int entry = this.FindEntry(key);
                if (entry >= 0)
                    return this._entries[entry].value;
                ThrowHelper.ThrowKeyNotFoundException((object) key);
                return default(TValue);
            }
            set => this.TryInsert(key, value, InsertionBehavior.OverwriteExisting);
        }

        /// <summary>Adds the specified key and value to the dictionary.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be <see langword="null" /> for reference types.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
        public void Add(TKey key, TValue value)
        {
            // Profiler.BeginSample("add");
            this.TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
            // Profiler.EndSample();
        }

        /// <summary>Adds the specified value to the <see cref="T:System.Collections.Generic.ICollection`1" /> with the specified key.</summary>
        /// <param name="keyValuePair">The <see cref="T:System.Collections.Generic.KeyValuePair`2" /> structure representing the key and value to add to the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">The key of <paramref name="keyValuePair" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            this.Add(keyValuePair.Key, keyValuePair.Value);
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific key and value.</summary>
        /// <param name="keyValuePair">The <see cref="T:System.Collections.Generic.KeyValuePair`2" /> structure to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="keyValuePair" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int entry = this.FindEntry(keyValuePair.Key);
            return entry >= 0 &&
                   EqualityComparer<TValue>.Default.Equals(this._entries[entry].value, keyValuePair.Value);
        }

        /// <summary>Removes a key and value from the dictionary.</summary>
        /// <param name="keyValuePair">The <see cref="T:System.Collections.Generic.KeyValuePair`2" /> structure representing the key and value to remove from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        /// <returns>
        /// <see langword="true" /> if the key and value represented by <paramref name="keyValuePair" /> is successfully found and removed; otherwise, <see langword="false" />. This method returns <see langword="false" /> if <paramref name="keyValuePair" /> is not found in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int entry = this.FindEntry(keyValuePair.Key);
            if (entry < 0 || !EqualityComparer<TValue>.Default.Equals(this._entries[entry].value, keyValuePair.Value))
                return false;
            this.Remove(keyValuePair.Key);
            return true;
        }

        /// <summary>Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
        public void Clear()
        {
            int count = this._count;
            if (count > 0)
            {
                _buckets.Span.Clear();
                // Array.Clear((Array) this._buckets, 0, this._buckets.Length);
                this._count = 0;
                this._freeList = -1;
                this._freeCount = 0;
                _entries.Span.Clear();
                // Array.Clear((Array) this._entries, 0, count);
            }

            ++this._version;
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.</summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        public bool ContainsKey(TKey key) => this.FindEntry(key) >= 0;

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains a specific value.</summary>
        /// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />. The value can be <see langword="null" /> for reference types.</param>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified value; otherwise, <see langword="false" />.</returns>
        public bool ContainsValue(TValue value)
        {
            var entries = this._entries;
            if ((object) value == null)
            {
                for (int index = 0; index < this._count; ++index)
                {
                    if (entries[index].hashCode >= 0 && (object) entries[index].value == null)
                        return true;
                }
            }
            else if ((object) default(TValue) != null)
            {
                for (int index = 0; index < this._count; ++index)
                {
                    if (entries[index].hashCode >= 0 &&
                        EqualityComparer<TValue>.Default.Equals(entries[index].value, value))
                        return true;
                }
            }
            else
            {
                EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;
                for (int index = 0; index < this._count; ++index)
                {
                    if (entries[index].hashCode >= 0 && equalityComparer.Equals(entries[index].value, value))
                        return true;
                }
            }

            return false;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if ((uint) index > (uint) array.Length)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (array.Length - index < this.Count)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            int count = this._count;
            var entries = this._entries;
            for (int index1 = 0; index1 < count; ++index1)
            {
                if (entries[index1].hashCode >= 0)
                    array[index++] = new KeyValuePair<TKey, TValue>(entries[index1].key, entries[index1].value);
            }
        }

        /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" /> structure for the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        public PDictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return new PDictionary<TKey, TValue>.Enumerator(this, 2);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<TKey, TValue>>) new PDictionary<TKey, TValue>.Enumerator(this, 2);
        }

        private int FindEntry(TKey key)
        {
            if ((object) key == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            int entry = -1;
            var buckets = this._buckets.ReadOnlySpan;
            var entries = this._entries.ReadOnlySpan;
            int num1 = 0;
            if (buckets != null)
            {
                IEqualityComparer<TKey> comparer = this._comparer;
                if (comparer == null)
                {
                    int num2 = key.GetHashCode() & int.MaxValue;
                    entry = buckets[num2 % buckets.Length] - 1;
                    if ((object) default(TKey) != null)
                    {
                        while ((uint) entry < (uint) entries.Length && (entries[entry].hashCode != num2 ||
                                                                        !EqualityComparer<TKey>.Default.Equals(
                                                                            entries[entry].key, key)))
                        {
                            entry = entries[entry].next;
                            if (num1 >= entries.Length)
                                ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                            ++num1;
                        }
                    }
                    else
                    {
                        EqualityComparer<TKey> equalityComparer = EqualityComparer<TKey>.Default;
                        while ((uint) entry < (uint) entries.Length && (entries[entry].hashCode != num2 ||
                                                                        !equalityComparer.Equals(entries[entry].key,
                                                                            key)))
                        {
                            entry = entries[entry].next;
                            if (num1 >= entries.Length)
                                ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                            ++num1;
                        }
                    }
                }
                else
                {
                    int num3 = comparer.GetHashCode(key) & int.MaxValue;
                    entry = buckets[num3 % buckets.Length] - 1;
                    while ((uint) entry < (uint) entries.Length &&
                           (entries[entry].hashCode != num3 || !comparer.Equals(entries[entry].key, key)))
                    {
                        entry = entries[entry].next;
                        if (num1 >= entries.Length)
                            ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                        ++num1;
                    }
                }
            }

            return entry;
        }

        private int Initialize(int capacity)
        {
            int prime = HashHelpers.GetPrime(capacity);
            this._freeList = -1;
            // this._buckets = new int[prime];

            if (_buckets != null)
            {
                _buckets.Dispose();
            }
            else
            {
                _buckets = new PList<int>();
            }

            _buckets.AppendSpan(prime);

            if (_entries != null)
            {
                _entries.Dispose();
            }
            else
            {
                _entries = new PList<Entry>();
            }

            _entries.AppendSpan(prime);

            return prime;
        }

        private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
        {
            if ((object) key == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            ++this._version;
            if (this._buckets == null)
                this.Initialize(0);
            var entries = this._entries.Span;
            IEqualityComparer<TKey> comparer = this._comparer;
            // int num1 = ((typeof(TKey).IsValueType && comparer == null)
            //     ? key.GetHashCode()
            //     : comparer!.GetHashCode(key));
            //此处GetHashCode在Debug模式下会产生gc，在release模式下则不会
            int num1 = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & int.MaxValue;
            int num2 = 0;
            ref int local1 = ref this._buckets!.GetRefValue(num1 % this._buckets.Length);
            int index1 = local1 - 1;
            // Profiler.BeginSample("check collision");
            if (comparer == null)
            {
                if (default(TKey) != null)
                {
                    while ((uint) index1 < (uint) entries.Length)
                    {
                        if (entries[index1].hashCode == num1 &&
                            EqualityComparer<TKey>.Default.Equals(entries[index1].key, key))
                        {
                            switch (behavior)
                            {
                                case InsertionBehavior.OverwriteExisting:
                                    entries[index1].value = value;
                                    return true;
                                case InsertionBehavior.ThrowOnExisting:
                                    ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException((object) key);
                                    break;
                            }

                            return false;
                        }

                        index1 = entries[index1].next;
                        if (num2 >= entries.Length)
                            ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                        ++num2;
                    }
                }
                else
                {
                    EqualityComparer<TKey> equalityComparer = EqualityComparer<TKey>.Default;
                    while ((uint) index1 < (uint) entries.Length)
                    {
                        if (entries[index1].hashCode == num1 && equalityComparer.Equals(entries[index1].key, key))
                        {
                            switch (behavior)
                            {
                                case InsertionBehavior.OverwriteExisting:
                                    entries[index1].value = value;
                                    return true;
                                case InsertionBehavior.ThrowOnExisting:
                                    ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException(key);
                                    break;
                            }

                            return false;
                        }

                        index1 = entries[index1].next;
                        if (num2 >= entries.Length)
                            ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                        ++num2;
                    }
                }
            }
            else
            {
                while ((uint) index1 < (uint) entries.Length)
                {
                    if (entries[index1].hashCode == num1 && comparer.Equals(entries[index1].key, key))
                    {
                        switch (behavior)
                        {
                            case InsertionBehavior.OverwriteExisting:
                                entries[index1].value = value;
                                return true;
                            case InsertionBehavior.ThrowOnExisting:
                                ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException((object) key);
                                break;
                        }

                        return false;
                    }

                    index1 = entries[index1].next;
                    if (num2 >= entries.Length)
                        ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                    ++num2;
                }
            }

            //Profiler.EndSample();

            //Profiler.BeginSample("after");

            bool flag1 = false;
            bool flag2 = false;
            int index2;
            if (this._freeCount > 0)
            {
                index2 = this._freeList;
                flag2 = true;
                --this._freeCount;
            }
            else
            {
                int count = this._count;
                if (count == entries.Length)
                {
                    this.Resize();
                    flag1 = true;
                }

                index2 = count;
                this._count = count + 1;
                entries = _entries.Span;
                // entries = this._entries;
            }

            ref int local2 = ref (flag1 ? ref this._buckets.GetRefValue(num1 % this._buckets.Length) : ref local1);
            ref Entry local3 = ref entries[index2];
            if (flag2) _freeList = local3.next;
            local3.hashCode = num1;
            local3.next = local2 - 1;
            local3.key = key;
            local3.value = value;
            local2 = index2 + 1;
            // Profiler.EndSample();
            return true;
        }

        private void Resize() => this.Resize(HashHelpers.ExpandPrime(this._count), false);

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            //因为下文要按hash重新布局，所以这个new是省部不掉的了
            var numArray = new PList<int>(newSize);
            var numArraySpan = numArray.AppendSpan(newSize);
            var destinationArray = new PList<Entry>(newSize);
            var destinationArraySpan = destinationArray.AppendSpan(newSize);
            int count = this._count;

            _entries.ReadOnlySpan.CopyTo(destinationArray.Span);
            // Array.Copy((Array) this._entries, 0, (Array) destinationArray, 0, count);
            if (default(TKey) == null & forceNewHashCodes)
            {
                for (int index = 0; index < count; ++index)
                {
                    if (destinationArraySpan[index].hashCode >= 0)
                        destinationArraySpan[index].hashCode =
                            destinationArraySpan[index].key.GetHashCode() & int.MaxValue;
                }
            }

            for (int index1 = 0; index1 < count; ++index1)
            {
                if (destinationArraySpan[index1].hashCode >= 0)
                {
                    int index2 = destinationArraySpan[index1].hashCode % newSize;
                    destinationArraySpan[index1].next = numArraySpan[index2] - 1;
                    numArraySpan[index2] = index1 + 1;
                }
            }

            _buckets.Dispose();
            _entries.Dispose();
            this._buckets = numArray;
            this._entries = destinationArray;
        }

        /// <summary>Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="key" /> is not found in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        public bool Remove(TKey key)
        {
            if ((object) key == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            if (this._buckets != null)
            {
                IEqualityComparer<TKey> comparer1 = this._comparer;
                int num = (comparer1 != null ? comparer1.GetHashCode(key) : key.GetHashCode()) & int.MaxValue;
                int index1 = num % this._buckets.Length;
                int index2 = -1;
                // ISSUE: variable of a reference type
                Entry defaultEntry = default;
                ref Entry local = ref defaultEntry;
                var entriesSpan = _entries.Span;
                var bucketsSapn = _buckets.Span;
                for (int index3 = bucketsSapn[index1] - 1; index3 >= 0; index3 = local.next)
                {
                    local = ref entriesSpan[index3];
                    if (local.hashCode == num)
                    {
                        IEqualityComparer<TKey> comparer2 = this._comparer;
                        if ((comparer2 != null
                                ? (comparer2.Equals(local.key, key) ? 1 : 0)
                                : (EqualityComparer<TKey>.Default.Equals(local.key, key) ? 1 : 0)) != 0)
                        {
                            if (index2 < 0)
                                bucketsSapn[index1] = local.next + 1;
                            else
                                entriesSpan[index2].next = local.next;
                            local.hashCode = -1;
                            local.next = this._freeList;
#if NETSTANDARD2_0
                            if (global::RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                                local.key = default(TKey);
                            if (global::RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                                local.value = default(TValue);
#else
                            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                                local.key = default(TKey);
                            if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                                local.value = default(TValue);
#endif
                            this._freeList = index3;
                            ++this._freeCount;
                            ++this._version;
                            return true;
                        }
                    }

                    index2 = index3;
                }
            }

            return false;
        }

        public bool Remove(TKey key, out TValue value)
        {
            if ((object) key == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            if (this._buckets != null)
            {
                IEqualityComparer<TKey> comparer1 = this._comparer;
                int num = (comparer1 != null ? comparer1.GetHashCode(key) : key.GetHashCode()) & int.MaxValue;
                int index1 = num % this._buckets.Length;
                int index2 = -1;
                // ISSUE: variable of a reference type
                Entry defaultEntry = default;
                ref Entry local = ref defaultEntry;
                var bucketsSpan = _buckets.Span;
                var entriesSpan = _entries.Span;
                for (int index3 = bucketsSpan[index1] - 1; index3 >= 0; index3 = local.next)
                {
                    local = ref entriesSpan[index3];
                    if (local.hashCode == num)
                    {
                        IEqualityComparer<TKey> comparer2 = this._comparer;
                        if ((comparer2 != null
                                ? (comparer2.Equals(local.key, key) ? 1 : 0)
                                : (EqualityComparer<TKey>.Default.Equals(local.key, key) ? 1 : 0)) != 0)
                        {
                            if (index2 < 0)
                                bucketsSpan[index1] = local.next + 1;
                            else
                                entriesSpan[index2].next = local.next;
                            value = local.value;
                            local.hashCode = -1;
                            local.next = this._freeList;
#if NETSTANDARD2_0
                            if (global::RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                                local.key = default(TKey);
                            if (global::RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                                local.value = default(TValue);
#else
                            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                                local.key = default(TKey);
                            if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                                local.value = default(TValue);
#endif
                            this._freeList = index3;
                            ++this._freeCount;
                            ++this._version;
                            return true;
                        }
                    }

                    index2 = index3;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            int entry = this.FindEntry(key);
            if (entry >= 0)
            {
                value = this._entries[entry].value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return this.TryInsert(key, value, InsertionBehavior.None);
        }

        /// <summary>Gets a value that indicates whether the dictionary is read-only.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />. In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns <see langword="false" />.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an array of type <see cref="T:System.Collections.Generic.KeyValuePair`2" />, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional array of type <see cref="T:System.Collections.Generic.KeyValuePair`2" /> that is the destination of the <see cref="T:System.Collections.Generic.KeyValuePair`2" /> elements copied from the <see cref="T:System.Collections.Generic.ICollection`1" />. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(
            KeyValuePair<TKey, TValue>[] array,
            int index)
        {
            this.CopyTo(array, index);
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an array, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///         <paramref name="array" /> is multidimensional.
        /// -or-
        /// <paramref name="array" /> does not have zero-based indexing.
        /// -or-
        /// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.
        /// -or-
        /// The type of the source <see cref="T:System.Collections.Generic.ICollection`1" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (array.Rank != 1)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            if (array.GetLowerBound(0) != 0)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
            if ((uint) index > (uint) array.Length)
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            if (array.Length - index < this.Count)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            switch (array)
            {
                case KeyValuePair<TKey, TValue>[] array1:
                    this.CopyTo(array1, index);
                    break;
                case DictionaryEntry[] dictionaryEntryArray:
                    var entries1 = this._entries.Span;
                    for (int index1 = 0; index1 < this._count; ++index1)
                    {
                        if (entries1[index1].hashCode >= 0)
                        {
                            int index2 = index++;
                            DictionaryEntry dictionaryEntry =
                                new DictionaryEntry(entries1[index1].key, entries1[index1].value);
                            dictionaryEntryArray[index2] = dictionaryEntry;
                        }
                    }

                    break;
                case object[] objArray:
                    label_18:
                    try
                    {
                        int count = this._count;
                        var entries2 = this._entries.Span;
                        for (int index3 = 0; index3 < count; ++index3)
                        {
                            if (entries2[index3].hashCode >= 0)
                            {
                                int index4 = index++;
                                // ISSUE: variable of a boxed type
                                // __Boxed<KeyValuePair<TKey, TValue>> local =
                                //     (ValueType) ;
                                array.SetValue(new KeyValuePair<TKey, TValue>(entries2[index3].key,
                                    entries2[index3].value), index4);
                                // objArray[index4] = (object) new KeyValuePair<TKey, TValue>(entries2[index3].key,
                                //     entries2[index3].value);
                            }
                        }

                        break;
                    }
                    catch (ArrayTypeMismatchException ex)
                    {
                        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                        break;
                    }
                default:
                    ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                    goto label_18;
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, 2);
        }

        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
            int length = this._entries == null ? 0 : this._entries.Length;
            if (length >= capacity)
                return length;
            if (this._buckets == null)
                return this.Initialize(capacity);
            int prime = HashHelpers.GetPrime(capacity);
            this.Resize(prime, false);
            return prime;
        }

        public void TrimExcess() => this.TrimExcess(this.Count);

        public void TrimExcess(int capacity)
        {
            if (capacity < this.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
            int prime = HashHelpers.GetPrime(capacity);
            var entries1 = this._entries.Span;
            int length = _entries == null ? 0 : entries1.Length;
            if (prime >= length)
                return;
            int count = this._count;
            this.Initialize(prime);
            var entries2 = this._entries.Span;
            var buckets = this._buckets.Span;
            int index1 = 0;
            for (int index2 = 0; index2 < count; ++index2)
            {
                int hashCode = entries1[index2].hashCode;
                if (hashCode >= 0)
                {
                    ref Entry local = ref entries2[index1];
                    local = entries1[index2];
                    int index3 = hashCode % prime;
                    local.next = buckets[index3] - 1;
                    buckets[index3] = index1 + 1;
                    ++index1;
                }
            }

            this._count = index1;
            this._freeCount = 0;
        }

        /// <summary>Gets a value that indicates whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
        /// <returns>
        /// <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns <see langword="false" />.</returns>
        bool ICollection.IsSynchronized => false;

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        object ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                    Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), (object) null);
                return this._syncRoot;
            }
        }

        /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.IDictionary" /> has a fixed size.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.IDictionary" /> has a fixed size; otherwise, <see langword="false" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns <see langword="false" />.</returns>
        bool IDictionary.IsFixedSize => false;

        /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.IDictionary" /> is read-only.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.IDictionary" /> is read-only; otherwise, <see langword="false" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns <see langword="false" />.</returns>
        bool IDictionary.IsReadOnly => false;

        /// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</summary>
        /// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</returns>
        ICollection IDictionary.Keys => (ICollection) this.Keys;

        /// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</summary>
        /// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</returns>
        ICollection IDictionary.Values => (ICollection) this.Values;

        /// <summary>Gets or sets the value with the specified key.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key, or <see langword="null" /> if <paramref name="key" /> is not in the dictionary or <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// -or-
        /// A value is being assigned, and <paramref name="value" /> is of a type that is not assignable to the value type <paramref name="TValue" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
        object IDictionary.this[object key]
        {
            get
            {
                if (PDictionary<TKey, TValue>.IsCompatibleKey(key))
                {
                    int entry = this.FindEntry((TKey) key);
                    if (entry >= 0)
                        return (object) this._entries[entry].value;
                }

                return (object) null;
            }
            set
            {
                if (key == null)
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
                try
                {
                    TKey key1 = (TKey) key;
                    try
                    {
                        this[key1] = (TValue) value;
                    }
                    catch (InvalidCastException ex)
                    {
                        ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                    }
                }
                catch (InvalidCastException ex)
                {
                    ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                }
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            return key is TKey;
        }

        /// <summary>Adds the specified key and value to the dictionary.</summary>
        /// <param name="key">The object to use as the key.</param>
        /// <param name="value">The object to use as the value.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///         <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// -or-
        /// <paramref name="value" /> is of a type that is not assignable to <paramref name="TValue" />, the type of values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.
        /// -or-
        /// A value with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
        void IDictionary.Add(object key, object value)
        {
            if (key == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
            try
            {
                TKey key1 = (TKey) key;
                try
                {
                    this.Add(key1, (TValue) value);
                }
                catch (InvalidCastException ex)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                }
            }
            catch (InvalidCastException ex)
            {
                ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
            }
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified key.</summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary" />.</param>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        bool IDictionary.Contains(object key)
        {
            return PDictionary<TKey, TValue>.IsCompatibleKey(key) && this.ContainsKey((TKey) key);
        }

        /// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</summary>
        /// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (IDictionaryEnumerator) new PDictionary<TKey, TValue>.Enumerator(this, 1);
        }

        /// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" />.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        void IDictionary.Remove(object key)
        {
            if (!PDictionary<TKey, TValue>.IsCompatibleKey(key))
                return;
            this.Remove((TKey) key);
        }

        private struct Entry
        {
            public int hashCode;
            public int next;
            public TKey key;
            public TValue value;
        }

        /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
        /// <typeparam name="TKey" />
        /// <typeparam name="TValue" />
        [Serializable]
        public struct Enumerator :
            IEnumerator<KeyValuePair<TKey, TValue>>,
            IDisposable,
            IEnumerator,
            IDictionaryEnumerator
        {
            private PDictionary<TKey, TValue> _dictionary;
            private int _version;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;
            private int _getEnumeratorRetType;
            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(PDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this._dictionary = dictionary;
                this._version = dictionary._version;
                this._index = 0;
                this._getEnumeratorRetType = getEnumeratorRetType;
                this._current = new KeyValuePair<TKey, TValue>();
            }

            /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
            /// <returns>
            /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public bool MoveNext()
            {
                if (this._version != this._dictionary._version)
                    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                while ((uint) this._index < (uint) this._dictionary._count)
                {
                    ref Entry local = ref this._dictionary._entries.GetRefValue(this._index++);
                    if (local.hashCode >= 0)
                    {
                        this._current = new KeyValuePair<TKey, TValue>(local.key, local.value);
                        return true;
                    }
                }

                this._index = this._dictionary._count + 1;
                this._current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2" /> at the current position of the enumerator.</returns>
            public KeyValuePair<TKey, TValue> Current => this._current;

            /// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" />.</summary>
            public void Dispose()
            {
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator, as an <see cref="T:System.Object" />.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
            object IEnumerator.Current
            {
                get
                {
                    if (this._index == 0 || this._index == this._dictionary._count + 1)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    return this._getEnumeratorRetType == 1
                        ? (object) new DictionaryEntry((object) this._current.Key, (object) this._current.Value)
                        : (object) new KeyValuePair<TKey, TValue>(this._current.Key, this._current.Value);
                }
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            void IEnumerator.Reset()
            {
                if (this._version != this._dictionary._version)
                    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                this._index = 0;
                this._current = new KeyValuePair<TKey, TValue>();
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the dictionary at the current position of the enumerator, as a <see cref="T:System.Collections.DictionaryEntry" />.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (this._index == 0 || this._index == this._dictionary._count + 1)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    return new DictionaryEntry((object) this._current.Key, (object) this._current.Value);
                }
            }

            /// <summary>Gets the key of the element at the current position of the enumerator.</summary>
            /// <returns>The key of the element in the dictionary at the current position of the enumerator.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (this._index == 0 || this._index == this._dictionary._count + 1)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    return (object) this._current.Key;
                }
            }

            /// <summary>Gets the value of the element at the current position of the enumerator.</summary>
            /// <returns>The value of the element in the dictionary at the current position of the enumerator.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (this._index == 0 || this._index == this._dictionary._count + 1)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    return (object) this._current.Value;
                }
            }
        }

        /// <summary>Represents the collection of keys in a <see cref="T:System.Collections.Generic.Dictionary`2" />. This class cannot be inherited.</summary>
        /// <typeparam name="TKey" />
        /// <typeparam name="TValue" />
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [Serializable]
        public sealed partial class KeyCollection :
            ICollection<TKey>,
            IEnumerable<TKey>,
            IEnumerable,
            ICollection,
            IReadOnlyCollection<TKey>
        {
            private PDictionary<TKey, TValue> _dictionary;

            /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> class that reflects the keys in the specified <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
            /// <param name="dictionary">The <see cref="T:System.Collections.Generic.Dictionary`2" /> whose keys are reflected in the new <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="dictionary" /> is <see langword="null" />.</exception>
            public KeyCollection(PDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                this._dictionary = dictionary;
            }

            /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
            /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator" /> for the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</returns>
            public PDictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator()
            {
                return new PDictionary<TKey, TValue>.KeyCollection.Enumerator(this._dictionary);
            }

            /// <summary>Copies the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
            /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
            /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="array" /> is <see langword="null" />.</exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            /// <paramref name="index" /> is less than zero.</exception>
            /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                if (index < 0 || index > array.Length)
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                if (array.Length - index < this._dictionary.Count)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                int count = this._dictionary._count;
                var entries = this._dictionary._entries.Span;
                for (int index1 = 0; index1 < count; ++index1)
                {
                    if (entries[index1].hashCode >= 0)
                        array[index++] = entries[index1].key;
                }
            }

            /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.
            /// Retrieving the value of this property is an O(1) operation.</returns>
            public int Count => this._dictionary.Count;

            /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
            /// <returns>
            /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />, this property always returns <see langword="true" />.</returns>
            bool ICollection<TKey>.IsReadOnly => true;

            /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />. This implementation always throws <see cref="T:System.NotSupportedException" />.</summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
            void ICollection<TKey>.Add(TKey item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />. This implementation always throws <see cref="T:System.NotSupportedException" />.</summary>
            /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
            void ICollection<TKey>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
            bool ICollection<TKey>.Contains(TKey item) => this._dictionary.ContainsKey(item);

            /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />. This implementation always throws <see cref="T:System.NotSupportedException" />.</summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if item was not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
            /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
            bool ICollection<TKey>.Remove(TKey item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
                return false;
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return (IEnumerator<TKey>) new PDictionary<TKey, TValue>.KeyCollection.Enumerator(this._dictionary);
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator) new PDictionary<TKey, TValue>.KeyCollection.Enumerator(this._dictionary);
            }

            /// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
            /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
            /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="array" /> is <see langword="null" />.</exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            /// <paramref name="index" /> is less than zero.</exception>
            /// <exception cref="T:System.ArgumentException">
            ///         <paramref name="array" /> is multidimensional.
            /// -or-
            /// <paramref name="array" /> does not have zero-based indexing.
            /// -or-
            /// The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.
            /// -or-
            /// The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                if (array.Rank != 1)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                if (array.GetLowerBound(0) != 0)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                if ((uint) index > (uint) array.Length)
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                if (array.Length - index < this._dictionary.Count)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                switch (array)
                {
                    case TKey[] array1:
                        this.CopyTo(array1, index);
                        break;
                    case object[] objArray:
                        label_13:
                        int count = this._dictionary._count;
                        var entries = this._dictionary._entries.Span;
                        try
                        {
                            for (int index1 = 0; index1 < count; ++index1)
                            {
                                if (entries[index1].hashCode >= 0)
                                {
                                    int index2 = index++;
                                    // ISSUE: variable of a boxed type
                                    // __Boxed<TKey> key = (object) entries[index1].key;
                                    // objArray[index2] = (object) key;
                                    array.SetValue(entries[index1].key, index2);
                                }
                            }

                            break;
                        }
                        catch (ArrayTypeMismatchException ex)
                        {
                            ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                            break;
                        }
                    default:
                        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                        goto label_13;
                }
            }

            /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
            /// <returns>
            /// <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />, this property always returns <see langword="false" />.</returns>
            bool ICollection.IsSynchronized => false;

            /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />, this property always returns the current instance.</returns>
            object ICollection.SyncRoot => ((ICollection) this._dictionary).SyncRoot;

            /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
            /// <typeparam name="TKey" />
            /// <typeparam name="TValue" />
            [Serializable]
            public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
            {
                private PDictionary<TKey, TValue> _dictionary;
                private int _index;
                private int _version;
                private TKey _currentKey;

                internal Enumerator(PDictionary<TKey, TValue> dictionary)
                {
                    this._dictionary = dictionary;
                    this._version = dictionary._version;
                    this._index = 0;
                    this._currentKey = default(TKey);
                }

                /// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator" />.</summary>
                public void Dispose()
                {
                }

                /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
                /// <returns>
                /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
                /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
                public bool MoveNext()
                {
                    if (this._version != this._dictionary._version)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    while ((uint) this._index < (uint) this._dictionary._count)
                    {
                        ref PDictionary<TKey, TValue>.Entry local =
                            ref this._dictionary._entries.GetRefValue(this._index++);
                        if (local.hashCode >= 0)
                        {
                            this._currentKey = local.key;
                            return true;
                        }
                    }

                    this._index = this._dictionary._count + 1;
                    this._currentKey = default(TKey);
                    return false;
                }

                /// <summary>Gets the element at the current position of the enumerator.</summary>
                /// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> at the current position of the enumerator.</returns>
                public TKey Current => this._currentKey;

                /// <summary>Gets the element at the current position of the enumerator.</summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
                object IEnumerator.Current
                {
                    get
                    {
                        if (this._index == 0 || this._index == this._dictionary._count + 1)
                            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                        return (object) this._currentKey;
                    }
                }

                /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
                /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
                void IEnumerator.Reset()
                {
                    if (this._version != this._dictionary._version)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    this._index = 0;
                    this._currentKey = default(TKey);
                }
            }
        }

        /// <summary>Represents the collection of values in a <see cref="T:System.Collections.Generic.Dictionary`2" />. This class cannot be inherited.</summary>
        /// <typeparam name="TKey" />
        /// <typeparam name="TValue" />
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [Serializable]
        public sealed class ValueCollection :
            ICollection<TValue>,
            IEnumerable<TValue>,
            IEnumerable,
            ICollection,
            IReadOnlyCollection<TValue>
        {
            private PDictionary<TKey, TValue> _dictionary;

            /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> class that reflects the values in the specified <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
            /// <param name="dictionary">The <see cref="T:System.Collections.Generic.Dictionary`2" /> whose values are reflected in the new <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="dictionary" /> is <see langword="null" />.</exception>
            public ValueCollection(PDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                this._dictionary = dictionary;
            }

            /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
            /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection.Enumerator" /> for the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</returns>
            public PDictionary<TKey, TValue>.ValueCollection.Enumerator GetEnumerator()
            {
                return new PDictionary<TKey, TValue>.ValueCollection.Enumerator(this._dictionary);
            }

            /// <summary>Copies the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
            /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
            /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="array" /> is <see langword="null" />.</exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            /// <paramref name="index" /> is less than zero.</exception>
            /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                if (index < 0 || index > array.Length)
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                if (array.Length - index < this._dictionary.Count)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                int count = this._dictionary._count;
                var entries = this._dictionary._entries.Span;
                for (int index1 = 0; index1 < count; ++index1)
                {
                    if (entries[index1].hashCode >= 0)
                        array[index++] = entries[index1].value;
                }
            }

            /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</returns>
            public int Count => this._dictionary.Count;

            /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
            /// <returns>
            /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />, this property always returns <see langword="true" />.</returns>
            bool ICollection<TValue>.IsReadOnly => true;

            /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.  This implementation always throws <see cref="T:System.NotSupportedException" />.</summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
            void ICollection<TValue>.Add(TValue item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />. This implementation always throws <see cref="T:System.NotSupportedException" />.</summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> was not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
            /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
            bool ICollection<TValue>.Remove(TValue item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
                return false;
            }

            /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.  This implementation always throws <see cref="T:System.NotSupportedException" />.</summary>
            /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
            void ICollection<TValue>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
            bool ICollection<TValue>.Contains(TValue item) => this._dictionary.ContainsValue(item);

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return (IEnumerator<TValue>) new PDictionary<TKey, TValue>.ValueCollection.Enumerator(this._dictionary);
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator) new PDictionary<TKey, TValue>.ValueCollection.Enumerator(this._dictionary);
            }

            /// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
            /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
            /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="array" /> is <see langword="null" />.</exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            /// <paramref name="index" /> is less than zero.</exception>
            /// <exception cref="T:System.ArgumentException">
            ///         <paramref name="array" /> is multidimensional.
            /// -or-
            /// <paramref name="array" /> does not have zero-based indexing.
            /// -or-
            /// The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.
            /// -or-
            /// The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                if (array.Rank != 1)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                if (array.GetLowerBound(0) != 0)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                if ((uint) index > (uint) array.Length)
                    ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
                if (array.Length - index < this._dictionary.Count)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                switch (array)
                {
                    case TValue[] array1:
                        this.CopyTo(array1, index);
                        break;
                    case object[] objArray:
                        label_13:
                        int count = this._dictionary._count;
                        var entries = this._dictionary._entries.Span;
                        try
                        {
                            for (int index1 = 0; index1 < count; ++index1)
                            {
                                if (entries[index1].hashCode >= 0)
                                {
                                    int index2 = index++;
                                    // ISSUE: variable of a boxed type
                                    // __Boxed<TValue> local = (object) entries[index1].value;
                                    // objArray[index2] = (object) local;
                                    array.SetValue(entries[index1].value, index2);
                                }
                            }

                            break;
                        }
                        catch (ArrayTypeMismatchException ex)
                        {
                            ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                            break;
                        }
                    default:
                        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                        goto label_13;
                }
            }

            /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
            /// <returns>
            /// <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />, this property always returns <see langword="false" />.</returns>
            bool ICollection.IsSynchronized => false;

            /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />, this property always returns the current instance.</returns>
            object ICollection.SyncRoot => ((ICollection) this._dictionary).SyncRoot;

            /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
            /// <typeparam name="TKey" />
            /// <typeparam name="TValue" />
            [Serializable]
            public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
            {
                private PDictionary<TKey, TValue> _dictionary;
                private int _index;
                private int _version;
                private TValue _currentValue;

                internal Enumerator(PDictionary<TKey, TValue> dictionary)
                {
                    this._dictionary = dictionary;
                    this._version = dictionary._version;
                    this._index = 0;
                    this._currentValue = default(TValue);
                }

                /// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection.Enumerator" />.</summary>
                public void Dispose()
                {
                }

                /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
                /// <returns>
                /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
                /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
                public bool MoveNext()
                {
                    if (this._version != this._dictionary._version)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    while ((uint) this._index < (uint) this._dictionary._count)
                    {
                        ref PDictionary<TKey, TValue>.Entry local =
                            ref this._dictionary._entries.GetRefValue(this._index++);
                        if (local.hashCode >= 0)
                        {
                            this._currentValue = local.value;
                            return true;
                        }
                    }

                    this._index = this._dictionary._count + 1;
                    this._currentValue = default(TValue);
                    return false;
                }

                /// <summary>Gets the element at the current position of the enumerator.</summary>
                /// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> at the current position of the enumerator.</returns>
                public TValue Current => this._currentValue;

                /// <summary>Gets the element at the current position of the enumerator.</summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
                object IEnumerator.Current
                {
                    get
                    {
                        if (this._index == 0 || this._index == this._dictionary._count + 1)
                            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                        return _currentValue;
                    }
                }

                /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
                /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
                void IEnumerator.Reset()
                {
                    if (this._version != this._dictionary._version)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    this._index = 0;
                    this._currentValue = default(TValue);
                }
            }
        }
    }
}