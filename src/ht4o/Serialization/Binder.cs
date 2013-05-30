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
namespace Hypertable.Persistence.Serialization
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Hypertable.Persistence.Reflection;

    /// <summary>
    /// The binder.
    /// </summary>
    public class Binder : SerializationBinder
    {
        #region Static Fields

        /// <summary>
        /// The remove assembly culture.
        /// </summary>
        private static bool removeAssemblyCulture = true;

        /// <summary>
        /// The remove assembly version.
        /// </summary>
        private static bool removeAssemblyVersion = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether the assembly culture should be removed from the qualified type name.
        /// </summary>
        /// <value>
        /// The strict explicit type codes.
        /// </value>
        public static bool RemoveAssemblyCulture
        {
            get
            {
                return removeAssemblyCulture;
            }

            set
            {
                removeAssemblyCulture = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the assembly public key token should be removed from the qualified type name.
        /// </summary>
        /// <value>
        /// The strict explicit type codes.
        /// </value>
        public static bool RemoveAssemblyPublicKeyToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the assembly version should be removed from the qualified type name.
        /// </summary>
        /// <value>
        /// The strict explicit type codes.
        /// </value>
        public static bool RemoveAssemblyVersion
        {
            get
            {
                return removeAssemblyVersion;
            }

            set
            {
                removeAssemblyVersion = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The bind to name.
        /// </summary>
        /// <param name="serializedType">
        /// The serialized type.
        /// </param>
        /// <param name="assemblyName">
        /// The assembly name.
        /// </param>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = serializedType.Assembly.FullName;
            typeName = serializedType.FullName ?? serializedType.Name;

            if (RemoveAssemblyVersion)
            {
                if (assemblyName != null)
                {
                    assemblyName = Regex.Replace(assemblyName, @", Version=\d+.\d+.\d+.\d+", string.Empty);
                }

                typeName = Regex.Replace(typeName, @", Version=\d+.\d+.\d+.\d+", string.Empty);
            }

            if (RemoveAssemblyCulture)
            {
                if (assemblyName != null)
                {
                    assemblyName = Regex.Replace(assemblyName, @", Culture=\w+", string.Empty);
                }

                typeName = Regex.Replace(typeName, @", Culture=\w+", string.Empty);
            }

            if (RemoveAssemblyPublicKeyToken)
            {
                if (assemblyName != null)
                {
                    assemblyName = Regex.Replace(assemblyName, @", PublicKeyToken=\w+", string.Empty);
                }

                typeName = Regex.Replace(typeName, @", PublicKeyToken=\w+", string.Empty);
            }
        }

        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <returns>
        /// The type of the object the formatter creates a new instance of.
        /// </returns>
        /// <param name="assemblyName">
        /// Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object. 
        /// </param>
        /// <param name="typeName">
        /// Specifies the <see cref="T:System.Type"/> name of the serialized object. 
        /// </param>
        public override Type BindToType(string assemblyName, string typeName)
        {
            return TypeLoader.GetType(Assembly.CreateQualifiedName(assemblyName, typeName));
        }

        #endregion
    }
}