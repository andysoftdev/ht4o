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
    using Hypertable.Persistence.Attributes;
    using Hypertable.Persistence.Extensions;

    //// TODO define and implement an embed attribute (embed entities instead of writing entity references)

    /// <summary>
    ///     The inspected property.
    /// </summary>
    internal sealed class InspectedProperty
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedProperty" /> class.
        /// </summary>
        /// <param name="type">
        ///     The inspected type.
        /// </param>
        /// <param name="propertyInfo">
        ///     The property info.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="type" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="propertyInfo" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="propertyInfo" /> 's declaring type has not been set.
        /// </exception>
        internal InspectedProperty(Type type, PropertyInfo propertyInfo)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (propertyInfo.DeclaringType == null)
            {
                throw new ArgumentException("fieldInfo declaring type has not been set", nameof(propertyInfo));
            }

            this.Name = propertyInfo.SerializableName();
            this.InspectedType = type;
            this.PropertyType = propertyInfo.PropertyType;
            this.IsNotNullableValueType = this.PropertyType.IsNotNullableValueType();

            if (propertyInfo.DeclaringType != propertyInfo.ReflectedType)
            {
                propertyInfo = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            try
            {
                this.Getter = DelegateFactory.CreateGetter(propertyInfo);
                this.Setter = DelegateFactory.CreateSetter(propertyInfo);
            }
            catch (Exception exception)
            {
                Logging.TraceException(exception);
            }

            this.Member = propertyInfo;

#if!HT4O_SERIALIZATION

            this.IdAttribute = propertyInfo.GetAttribute<IdAttribute>();

#endif

            this.IsTransient = propertyInfo.HasAttribute<TransientAttribute>() ||
                               propertyInfo.HasAttribute<IgnoreDataMemberAttribute>();
            this.Ignore = propertyInfo.HasAttribute<IgnoreAttribute>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedProperty" /> class.
        /// </summary>
        /// <param name="type">
        ///     The inspected type.
        /// </param>
        /// <param name="fieldInfo">
        ///     The field info.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="type" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="fieldInfo" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="fieldInfo" /> 's declaring type has not been set.
        /// </exception>
        internal InspectedProperty(Type type, FieldInfo fieldInfo)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            if (fieldInfo.DeclaringType == null)
            {
                throw new ArgumentException("fieldInfo declaring type has not been set", nameof(fieldInfo));
            }

            this.Name = fieldInfo.SerializableName();
            this.InspectedType = type;
            this.PropertyType = fieldInfo.FieldType;
            this.IsNotNullableValueType = this.PropertyType.IsNotNullableValueType();

            if (fieldInfo.DeclaringType != fieldInfo.ReflectedType)
            {
                fieldInfo = fieldInfo.DeclaringType.GetField(fieldInfo.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            try
            {
                this.Getter = DelegateFactory.CreateGetter(fieldInfo);
                this.Setter = DelegateFactory.CreateSetter(fieldInfo);
            }
            catch (Exception exception)
            {
                Logging.TraceException(exception);
            }

            this.Member = fieldInfo;

#if!HT4O_SERIALIZATION

            this.IdAttribute = fieldInfo.GetAttribute<IdAttribute>();

#endif
            this.IsTransient = fieldInfo.HasAttribute<TransientAttribute>() ||
                               fieldInfo.HasAttribute<NonSerializedAttribute>()
                               || fieldInfo.HasAttribute<IgnoreDataMemberAttribute>();

            this.Ignore = fieldInfo.HasAttribute<IgnoreAttribute>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the getter function.
        /// </summary>
        /// <value>
        ///     The getter function.
        /// </value>
        internal Func<object, object> Getter { get; }

        /// <summary>
        ///     Gets a value indicating whether the inspected property has a getter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the inspected property has a getter, otherwise <c>false</c>.
        /// </value>
        internal bool HasGetter => this.Getter != null;

#if!HT4O_SERIALIZATION

        /// <summary>
        ///     Gets a value indicating whether the inspected property is of type <see cref="Hypertable.Key" />.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the inspected property is of type <see cref="Hypertable.Key" />, otherwise <c>false</c>.
        /// </value>
        internal bool HasKey => this.PropertyType == typeof(Key);

#endif

        /// <summary>
        ///     Gets a value indicating whether the inspected property has a setter.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the inspected property has a setter, otherwise <c>false</c>.
        /// </value>
        internal bool HasSetter => this.Setter != null;

#if!HT4O_SERIALIZATION

        /// <summary>
        ///     Gets the identifier attribute.
        /// </summary>
        /// <value>
        ///     The identifier attribute or null.
        /// </value>
        internal IdAttribute IdAttribute { get; }

#endif

        /// <summary>
        ///     Gets or sets a value indicating whether to ignore this property on serialization.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the serializer should ignore this property, otherwise <c>false</c>.
        /// </value>
        internal bool Ignore { get; set; }

        /// <summary>
        ///     Gets the inspected type.
        /// </summary>
        /// <value>
        ///     The inspected type.
        /// </value>
        internal Type InspectedType { get; }

        /// <summary>
        ///     Gets a value indicating whether the inspected property type is a not nullable value type.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the inspected property type is a not nullable value type, otherwise <c>false</c>.
        /// </value>
        internal bool IsNotNullableValueType { get; }

        /// <summary>
        ///     Gets a value indicating whether IsTransient.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the property is transient, otherwise <c>false</c>.
        /// </value>
        internal bool IsTransient { get; }

        /// <summary>
        ///     Gets the member info.
        /// </summary>
        /// <value>
        ///     The member info.
        /// </value>
        internal MemberInfo Member { get; }

        /// <summary>
        ///     Gets the property name.
        /// </summary>
        /// <value>
        ///     The property name.
        /// </value>
        internal string Name { get; }

        /// <summary>
        ///     Gets the property type.
        /// </summary>
        /// <value>
        ///     The property type.
        /// </value>
        internal Type PropertyType { get; }

        /// <summary>
        ///     Gets the setter.
        /// </summary>
        /// <value>
        ///     The setter.
        /// </value>
        internal Action<object, object> Setter { get; }

        #endregion
    }
}