/** -*- C# -*-
 * Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4o.
 *
 * ht4o is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */
namespace Hypertable.Persistence.Test.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Hypertable;

    internal static class Equatable
    {
        #region Public Methods and Operators

        public static bool AreEqual(object a, object b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is DictionaryEntry)
            {
                var dea = (DictionaryEntry)a;
                if (!(b is DictionaryEntry))
                {
                    return false;
                }

                var deb = (DictionaryEntry)b;
                return AreEqual(dea.Key, deb.Key) && AreEqual(dea.Value, deb.Value);
            }

            if ((a is Array) && (b is Array))
            {
                return AreArrayEqual(a as Array, b as Array);
            }

            if (a is IDictionary)
            {
                return AreDictionaryEqual(a as IDictionary, b as IDictionary);
            }

            if (a is IEnumerable && !(a is string))
            {
                return AreEnumerableEqual(a as IEnumerable, b as IEnumerable);
            }

            return Convert.ChangeType(b, a.GetType()).Equals(a);
        }

        public static bool AreEqual<T>(T[] a, T[] b) where T : class
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (a.Rank != b.Rank)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (var i = 0; i < a.Length; ++i)
            {
                if ((a[i] == null) != (b[i] == null))
                {
                    return false;
                }

                if (a[i] != null && !a[i].Equals(b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AreEqual<T>(ICollection<T> a, ICollection<T> b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            return a.SequenceEqual(b);
        }

        public static bool AreEqual<T>(T a, T? b) where T : struct
        {
            return (b.HasValue && a.Equals(b)) || (!b.HasValue && a.Equals(default(T)));
        }

        public static bool AreEqual<T>(T? a, T? b) where T : struct {
            return (a.HasValue && b.HasValue && a.Equals(b)) || (!a.HasValue && !b.HasValue);
        }

        public static bool AreEqual<T>(T? a, T b) where T : struct {
            return (a.HasValue && a.Equals(b)) || (!a.HasValue && b.Equals(default(T)));
        }

        public static bool AreEqual<T>(ISet<T> a, ISet<T> b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            return a.SetEquals(b);
        }

        #endregion

        #region Methods

        private static bool AreArrayEqual(Array a, Array b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (a.Rank != b.Rank)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            return AreEnumerableEqual(a, b);
        }

        private static bool AreDictionaryEqual(IDictionary a, IDictionary b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            foreach (DictionaryEntry item in a)
            {
                if (!b.Contains(item.Key))
                {
                    return false;
                }

                if (!AreEqual(item.Value, b[item.Key]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreEnumerableEqual(IEnumerable a, IEnumerable b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            var la = a.Cast<object>().ToList();
            var lb = b.Cast<object>().ToList();

            if (la.Count != lb.Count)
            {
                return false;
            }

            var n = 0;
            return la.All(item => AreEqual(item, lb[n++]));
        }

        #endregion
    }
}