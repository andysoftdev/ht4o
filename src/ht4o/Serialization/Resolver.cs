﻿/** -*- C# -*-
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
namespace Hypertable.Persistence.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The resolver.
    /// </summary>
    public static class Resolver
    {
        #region Static Fields

        /// <summary>
        /// The assembly resolver.
        /// </summary>
        private static Func<AssemblyName, Assembly> assemblyResolver;

        /// <summary>
        /// The instance resolver.
        /// </summary>
        private static Func<Type, Type, object> instanceResolver;

        /// <summary>
        /// The obsolete property resolver.
        /// </summary>
        private static Action<object, object> obsoletePropertyResolver;

        /// <summary>
        /// The type resolver.
        /// </summary>
        private static Func<Assembly, string, bool, Type> typeResolver;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="Resolver"/> class.
        /// </summary>
        static Resolver()
        {
            assemblyResolver = assemblyName => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, assemblyName.Name));
            typeResolver = (assembly, simpleTypeName, ignoreCase) => assembly.GetType(simpleTypeName, false, ignoreCase);
            instanceResolver = (serializedType, destinationType) => null;
            obsoletePropertyResolver = (instance, value) => { };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public static Func<AssemblyName, Assembly> AssemblyResolver
        {
            get
            {
                return assemblyResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                assemblyResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the instance resolver.
        /// </summary>
        /// <value>
        /// The instance resolver.
        /// </value>
        public static Func<Type, Type, object> InstanceResolver
        {
            get
            {
                return instanceResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                instanceResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the obsolete property resolver.
        /// </summary>
        /// <value>
        /// The obsolete property resolver.
        /// </value>
        public static Action<object, object> ObsoletePropertyResolver
        {
            get
            {
                return obsoletePropertyResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                obsoletePropertyResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the type resolver.
        /// </summary>
        /// <value>
        /// The type resolver.
        /// </value>
        public static Func<Assembly, string, bool, Type> TypeResolver
        {
            get
            {
                return typeResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                typeResolver = value;
            }
        }

        #endregion
    }
}