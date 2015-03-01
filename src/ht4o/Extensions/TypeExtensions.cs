/** -*- C# -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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
namespace Hypertable.Persistence.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The type extensions.
    /// </summary>
    internal static class TypeExtensions
    {
        #region Methods

        /// <summary>
        /// Converts a value to the type specified.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="value">
        /// The value to convert.
        /// </param>
        /// <returns>
        /// The converted value.
        /// </returns>
        internal static object Convert(this Type destinationType, object value)
        {
            if (value == null || destinationType == null || destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            if (destinationType.IsNullable())
            {
                destinationType = Nullable.GetUnderlyingType(destinationType);

                if (destinationType.IsInstanceOfType(value))
                {
                    return value;
                }
            }

            if (destinationType.IsEnum)
            {
                return Enum.ToObject(destinationType, value);
            }

            if (destinationType == typeof(string))
            {
                return value.ToString();
            }

            var s = value as string;
            if (s != null)
            {
                return destinationType == typeof(StringBuilder) ? new StringBuilder(s) : System.Convert.ChangeType(s, destinationType, CultureInfo.CurrentCulture);
            }

            if (destinationType.IsPrimitive)
            {
                return System.Convert.ChangeType(value, destinationType, CultureInfo.CurrentCulture);
            }

            if (destinationType.IsArray)
            {
                var elementType = destinationType.GetElementType();
                var array = Array.CreateInstance(elementType, 1);
                array.SetValue(Convert(elementType, value), 0);
                return array;
            }

            if (typeof(IList).IsAssignableFrom(destinationType))
            {
                var list = (IList)Activator.CreateInstance(destinationType, true);
                list.Add(Convert(destinationType.IsGenericType ? destinationType.GetGenericArguments()[0] : typeof(object), value));
                return list;
            }

            return value;
        }

        /// <summary>
        /// Gets a single attribute from the type specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="inherit">
        /// <c>true</c> to search this member's inheritance chain to find the attributes, otherwise <C>False</C>.
        /// </param>
        /// <typeparam name="T">
        /// The attribute type.
        /// </typeparam>
        /// <returns>
        /// The attribute instance or null.
        /// </returns>
        internal static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(T), inherit);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Get all methods with attribute specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <typeparam name="T">
        /// The attribute type.
        /// </typeparam>
        /// <returns>
        /// The method list or null.
        /// </returns>
        internal static List<MethodInfo> GetMethodsWithAttribute<T>(this Type type) where T : Attribute
        {
            return GetMethodsWithAttribute(type, typeof(T));
        }

        /// <summary>
        /// Get all methods with attribute specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="attribute">
        /// The attribute type.
        /// </param>
        /// <returns>
        /// The method list or null.
        /// </returns>
        internal static List<MethodInfo> GetMethodsWithAttribute(this Type type, Type attribute)
        {
            var methodInfos = new List<MethodInfo>();
            var t = type;
            while (t != null && t != typeof(object))
            {
                var methods = t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                methodInfos.AddRange(methods.Where(methodInfo => methodInfo.IsDefined(attribute, false)));
                t = t.BaseType;
            }

            methodInfos.Reverse(); // We should invoke the methods starting from base
            return methodInfos.Count > 0 ? methodInfos : null;
        }

        /// <summary>
        /// Returns a value indicating whether the type has the attribute declared.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="inherit">
        /// <c>true</c> to search this member's inheritance chain to find the attributes, otherwise <C>False</C>.
        /// </param>
        /// <typeparam name="T">
        /// The attribute type.
        /// </typeparam>
        /// <returns>
        /// <c>true</c> if the type has the attribute declared, otherwise <c>false</c>.
        /// </returns>
        internal static bool HasAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        /// <summary>
        /// Returns a value indicating whether the type implements the interface specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="typeInterface">
        /// The interface type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type implements the interface specified, otherwise <c>false</c>.
        /// </returns>
        internal static bool HasInterface(this Type type, Type typeInterface)
        {
            if (SameMetadataToken(type, typeInterface))
            {
                return true;
            }

            return type.GetInterfaces().Any(ifc => SameMetadataToken(ifc, typeInterface));
        }

        /// <summary>
        /// Gets a value indicating whether the is complex.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type is complex, otherwise <c>false</c>.
        /// </returns>
        internal static bool IsComplex(this Type type)
        {
            return type != typeof(string) && type != typeof(Type).GetType() && type.IsClass && !type.IsValueType;
        }

        /// <summary>
        /// Gets a value indicating whether the type is a delegate.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type is a delegate, otherwise <c>false</c>.
        /// </returns>
        internal static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type)
                   ||
                   (type.IsGenericType
                    &&
                    (type.GetGenericTypeDefinition() == typeof(Action<>) || type.GetGenericTypeDefinition() == typeof(Func<>)
                     || type.GetGenericTypeDefinition() == typeof(Expression<>)));
        }

        /// <summary>
        /// Gets a value indicating whether the type is the generic type definition specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="genericTypeDefinition">
        /// The generic type definition.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type is the generic type definition specified, otherwise <c>false</c>.
        /// </returns>
        internal static bool IsGenericTypeDefinition(this Type type, Type genericTypeDefinition)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
        }

        /// <summary>
        /// Gets a value indicating whether the type is not nullable value type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type is not nullable value type, otherwise <c>false</c>.
        /// </returns>
        internal static bool IsNotNullableValueType(this Type type)
        {
            return type.IsValueType && !IsNullable(type);
        }

        /// <summary>
        /// Gets a value indicating whether the type is a nullable type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type is a nullable type, otherwise <c>false</c>.
        /// </returns>
        internal static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Gets a value indicating whether the type is transient.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type is transient, otherwise <c>false</c>.
        /// </returns>
        internal static bool IsTransient(this Type type)
        {
            return IsDelegate(type) || type == typeof(IntPtr);
        }

        /// <summary>
        /// Gets a value indicating whether the member info have the same metadata token.
        /// </summary>
        /// <param name="x">
        /// The first member info.
        /// </param>
        /// <param name="y">
        /// The second member info.
        /// </param>
        /// <returns>
        /// <c>true</c> if the member info have the same metadata token, otherwise <c>false</c>.
        /// </returns>
        private static bool SameMetadataToken(MemberInfo x, MemberInfo y)
        {
            return x.Module.Assembly == y.Module.Assembly && x.Module == y.Module && x.MetadataToken == y.MetadataToken;
        }

        #endregion
    }
}