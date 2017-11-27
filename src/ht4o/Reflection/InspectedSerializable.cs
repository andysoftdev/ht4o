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
    using System.Reflection.Emit;
    using System.Runtime.Serialization;

    /// <summary>
    ///     The inspected serializable.
    /// </summary>
    internal sealed class InspectedSerializable
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedSerializable" /> class.
        /// </summary>
        /// <param name="type">
        ///     The type to inspect.
        /// </param>
        internal InspectedSerializable(Type type)
        {
            this.InspectedType = type;
            try
            {
                this.CreateInstance = CreateCreateInstanceMethod(type);
            }
            catch (Exception exception)
            {
                Logging.TraceException(exception);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the CreateInstance function.
        /// </summary>
        /// <value>
        ///     The CreateInstance function.
        /// </value>
        internal Func<SerializationInfo, StreamingContext, object> CreateInstance { get; }

        /// <summary>
        ///     Gets the inspected type.
        /// </summary>
        /// <value>
        ///     The inspected type.
        /// </value>
        internal Type InspectedType { get; }

        #endregion

        #region Methods

        /// <summary>
        ///     Create the CreateInstance function for the type specified.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     The newly created CreateInstance function or null.
        /// </returns>
        private static Func<SerializationInfo, StreamingContext, object> CreateCreateInstanceMethod(Type type)
        {
            var constructorInfo = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                new[] {typeof(SerializationInfo), typeof(StreamingContext)}, null);

            if (constructorInfo == null)
            {
                return null;
            }

            var method = new DynamicMethod(
                "Ctor" + constructorInfo.Name, typeof(object),
                new[] {typeof(SerializationInfo), typeof(StreamingContext)}, constructorInfo.Module, true);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Newobj, constructorInfo);
            generator.Emit(OpCodes.Ret);

            return (Func<SerializationInfo, StreamingContext, object>) method.CreateDelegate(
                typeof(Func<SerializationInfo, StreamingContext, object>));
        }

        #endregion
    }
}