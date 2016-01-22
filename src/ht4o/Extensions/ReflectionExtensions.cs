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
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// The reflection extensions.
    /// </summary>
    internal static class ReflectionExtensions
    {
        #region Methods

        /// <summary>
        /// Gets a single attribute from the member info specified.
        /// </summary>
        /// <param name="memberInfo">
        /// The member info.
        /// </param>
        /// <typeparam name="T">
        /// The attribute type.
        /// </typeparam>
        /// <returns>
        /// The attribute instance or null.
        /// </returns>
        internal static T GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Gets a single attribute from the assembly specified.
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <typeparam name="T">
        /// The attribute type.
        /// </typeparam>
        /// <returns>
        /// The attribute instance or null.
        /// </returns>
        internal static T GetAttribute<T>(this Assembly assembly) where T : Attribute
        {
            var attributes = assembly.GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Returns a value indicating whether the assembly has the attribute declared.
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <typeparam name="T">
        /// The attribute type.
        /// </typeparam>
        /// <returns>
        /// <c>true</c> if the assembly has the attribute declared, otherwise <c>false</c>.
        /// </returns>
        internal static bool HasAttribute<T>(this Assembly assembly) where T : Attribute
        {
            return assembly.GetCustomAttributes(typeof(T), false).Length > 0;
        }

        /// <summary>
        /// Returns a value indicating whether the member info has the attribute declared.
        /// </summary>
        /// <param name="memberInfo">
        /// The member info.
        /// </param>
        /// <typeparam name="T">
        /// The attribute type.
        /// </typeparam>
        /// <returns>
        /// <c>true</c> if the member info has the attribute declared, otherwise <c>false</c>.
        /// </returns>
        internal static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), false).Length > 0;
        }

        /// <summary>
        /// Creates a serializable name for the member info specified.
        /// </summary>
        /// <param name="memberInfo">
        /// The member info.
        /// </param>
        /// <returns>
        /// The serializable name.
        /// </returns>
        internal static string SerializableName(this MemberInfo memberInfo)
        {
            var dataMemberAttribute = memberInfo.GetAttribute<DataMemberAttribute>();
            if (dataMemberAttribute != null && !string.IsNullOrEmpty(dataMemberAttribute.Name))
            {
                return dataMemberAttribute.Name;
            }

            var fieldInfo = memberInfo as FieldInfo;
            return fieldInfo != null ? EscapeFieldInfoName(fieldInfo) : memberInfo.Name;
        }

        /// <summary>
        /// Escapes the field info name.
        /// </summary>
        /// <param name="fieldInfo">
        /// The field info.
        /// </param>
        /// <returns>
        /// The escaped field info name.
        /// </returns>
        private static string EscapeFieldInfoName(FieldInfo fieldInfo)
        {
            var name = fieldInfo.Name;
            if (name.StartsWith("<", StringComparison.Ordinal))
            {
                var closure = name.IndexOf('>');
                if (closure > 1)
                {
                    name = name.StartsWith("<backing_store>", StringComparison.Ordinal) ? name.Substring(closure + 1) : name.Substring(1, closure - 1);
                }
            }

            return name;
        }

        #endregion
    }
}