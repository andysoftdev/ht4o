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

namespace Hypertable.Persistence.Reflection
{
    using System;
    using System.IO;
    using Hypertable.Persistence.Collections.Concurrent;
    using Hypertable.Persistence.Serialization;

    /// <summary>
    ///     The type loader.
    /// </summary>
    public static class TypeLoader
    {
        #region Static Fields

        /// <summary>
        ///     The types.
        /// </summary>
        private static readonly ConcurrentStringDictionary<Type> Types = new ConcurrentStringDictionary<Type>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets type for the type name specified.
        /// </summary>
        /// <param name="typeName">
        ///     The type name.
        /// </param>
        /// <returns>
        ///     The resolved type.
        /// </returns>
        public static Type GetType(string typeName)
        {
            return Types.GetOrAdd(
                typeName,
                tn =>
                {
                    Type type = null;

                    // Catching any exceptions that could be thrown from a failure on assembly load 
                    // This is necessary, for example, if there are generic parameters that are qualified with a version of the assembly that predates the one available
                    try
                    {
                        type = Type.GetType(tn, false, false);
                    }
                    catch (TypeLoadException)
                    {
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    catch (FileLoadException)
                    {
                    }
                    catch (BadImageFormatException)
                    {
                    }

                    return type ?? Type.GetType(tn, Resolver.AssemblyResolver, Resolver.TypeResolver);
                });
        }

        #endregion
    }
}