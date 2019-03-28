// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class DictionaryImpl<TKey, TValue, TComparer>
        : DictionaryImpl where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Static Fields

        internal static
            Func<ConcurrentDictionary<TKey, TValue, TComparer>, int, DictionaryImpl<TKey, TValue, TComparer>>
            CreateRefUnsafe =
                (topDict, capacity) =>
                {
                    var method = typeof(DictionaryImpl)
                        .GetMethod("CreateRef", BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(typeof(TKey), typeof(TValue), typeof(TComparer));

                    var del =
                    (Func<ConcurrentDictionary<TKey, TValue, TComparer>, int,
                        DictionaryImpl<TKey, TValue, TComparer>>) method
                        .CreateDelegate(
                            typeof(Func<ConcurrentDictionary<TKey, TValue, TComparer>, int,
                                DictionaryImpl<TKey, TValue, TComparer>>));

                    var result = del(topDict, capacity);
                    CreateRefUnsafe = del;

                    return result;
                };

        internal static
            Func<ConcurrentDictionary<TKey, TValue, TComparer>, int, DictionaryImpl<TKey, TValue, TComparer>>
            CreateIntUnsafe =
                (topDict, capacity) =>
                {
                    var method = typeof(DictionaryImpl)
                        .GetMethod("CreateInt", BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(typeof(TValue), typeof(TComparer));

                    var del =
                    (Func<ConcurrentDictionary<TKey, TValue, TComparer>, int,
                        DictionaryImpl<TKey, TValue, TComparer>>) method
                        .CreateDelegate(
                            typeof(Func<ConcurrentDictionary<TKey, TValue, TComparer>, int,
                                DictionaryImpl<TKey, TValue, TComparer>>));

                    var result = del(topDict, capacity);
                    CreateRefUnsafe = del;

                    return result;
                };


        internal static
            Func<ConcurrentDictionary<TKey, TValue, TComparer>, int, DictionaryImpl<TKey, TValue, TComparer>>
            CreateLongUnsafe =
                (topDict, capacity) =>
                {
                    var method = typeof(DictionaryImpl)
                        .GetMethod("CreateLong", BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(typeof(TValue), typeof(TComparer));

                    var del =
                    (Func<ConcurrentDictionary<TKey, TValue, TComparer>, int,
                        DictionaryImpl<TKey, TValue, TComparer>>) method
                        .CreateDelegate(
                            typeof(Func<ConcurrentDictionary<TKey, TValue, TComparer>, int,
                                DictionaryImpl<TKey, TValue, TComparer>>));

                    var result = del(topDict, capacity);
                    CreateRefUnsafe = del;

                    return result;
                };

        #endregion

        #region Fields

        // TODO: move to leafs
        internal readonly TComparer _keyComparer = default(TComparer);

        #endregion

        #region Properties

        internal abstract int Count { get; }

        #endregion

        #region Methods

        internal abstract void Clear();

        internal abstract IDictionaryEnumerator GetDictionaryEnumerator();

        internal abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        internal abstract TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

        internal abstract TValue GetOrAdd<TArgument>(TKey key, TArgument argument, Func<TKey, TArgument, TValue> valueFactory);

        internal abstract TValue GetOrAdd<TArgument1, TArgument2>(TKey key, TArgument1 argument1, TArgument2 argument2, Func<TKey, TArgument1, TArgument2, TValue> valueFactory);

        internal abstract bool PutIfMatch(TKey key, object newVal, ref object oldValue, ValueMatch match);

        internal abstract object TryGetValue(TKey key);

        #endregion
    }
}