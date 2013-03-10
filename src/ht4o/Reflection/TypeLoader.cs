/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Collections.Concurrent;
    using System.Reflection;

    /// <summary>
    /// The type loader.
    /// </summary>
    internal static class TypeLoader
    {
        #region Static Fields

        /// <summary>
        /// The types.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> Types = new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// Indicating whether the code is running on the expected thread.
        /// </summary>
        [ThreadStatic]
        private static bool runningOnThisThread;

        #endregion

        #region Methods

        /// <summary>
        /// Gets type for the type name specified.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <returns>
        /// The resolved type.
        /// </returns>
        internal static Type GetType(string typeName)
        {
            return Types.GetOrAdd(
                typeName, 
                tn =>
                    {
                        try
                        {
                            // Attach our custom assembly name resolver, attempt to resolve again, and detach it
                            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;
                            runningOnThisThread = true;
                            return Type.GetType(typeName);
                        }
                        finally
                        {
                            runningOnThisThread = false;
                            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolver;
                        }
                    });
        }

        /// <summary>
        /// The assembly resolver event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender object.
        /// </param>
        /// <param name="args">
        /// The event arguments.
        /// </param>
        /// <returns>
        /// The resolved assembly.
        /// </returns>
        private static Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            // Only process events from the thread that started it, not any other thread
            if (runningOnThisThread)
            {
                // Extract assembly name, and checking it's the same as args.Name to prevent an infinite loop
                var assemblyName = new AssemblyName(args.Name);
                if (assemblyName.Name != args.Name)
                {
                    return ((AppDomain)sender).Load(assemblyName.Name);
                }
            }

            return null;
        }

        #endregion
    }
}