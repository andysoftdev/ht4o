/** -*- C# -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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

    /// <summary>
    /// The inspected enumerable.
    /// </summary>
    internal sealed class InspectedEnumerable
    {
        #region Fields

        /// <summary>
        /// The add function.
        /// </summary>
        private Func<object, object, object> addFunc;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectedEnumerable"/> class.
        /// </summary>
        /// <param name="type">
        /// The type to inspect.
        /// </param>
        internal InspectedEnumerable(Type type)
        {
            this.InspectedType = type;
            this.ElementType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
            try
            {
                this.Add = this.CreateAddMethod(type);
                this.Count = CreateCountMethod(type);
                this.Capacity = CreateCapacityMethod(type);
                this.Indexer = CreateIndexerMethod(type);
            }
            catch (Exception exception)
            {
                Logging.TraceException(exception);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the add action.
        /// </summary>
        /// <value>
        /// The add action.
        /// </value>
        internal Action<object, object> Add { get; private set; }

        /// <summary>
        /// Gets the capacity action.
        /// </summary>
        /// <value>
        /// The capacity action.
        /// </value>
        internal Action<object, object> Capacity { get; private set; }

        /// <summary>
        /// Gets the count function.
        /// </summary>
        /// <value>
        /// The count function.
        /// </value>
        internal Func<object, object> Count { get; private set; }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        /// <value>
        /// The element type.
        /// </value>
        internal Type ElementType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the inspected enumerable has an add action.
        /// </summary>
        /// <value>
        /// <c>true</c> if the inspected enumerable has an add action, otherwise <c>false</c>.
        /// </value>
        internal bool HasAdd
        {
            get
            {
                return this.Add != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the inspected enumerable has an capacity action.
        /// </summary>
        /// <value>
        /// <c>true</c> if the inspected enumerable has an capacity action, otherwise <c>false</c>.
        /// </value>
        internal bool HasCapacity
        {
            get
            {
                return this.Capacity != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the inspected enumerable has an count function.
        /// </summary>
        /// <value>
        /// <c>true</c> if the inspected enumerable has an count function, otherwise <c>false</c>.
        /// </value>
        internal bool HasCount
        {
            get
            {
                return this.Count != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the inspected enumerable has an indexer.
        /// </summary>
        /// <value>
        /// <c>true</c> if the inspected enumerable has an indexer, otherwise <c>false</c>.
        /// </value>
        internal bool HasIndexer
        {
            get
            {
                return this.Indexer != null;
            }
        }

        /// <summary>
        /// Gets the indexer action.
        /// </summary>
        /// <value>
        /// The indexer action.
        /// </value>
        internal Action<object, int, object> Indexer { get; private set; }

        /// <summary>
        /// Gets the inspected type.
        /// </summary>
        /// <value>
        /// The inspected type.
        /// </value>
        internal Type InspectedType { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Create the capacity action for the type specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The newly created capacity action or null.
        /// </returns>
        private static Action<object, object> CreateCapacityMethod(Type type)
        {
            return
                DelegateFactory.CreateSetterAction(
                    type.GetProperty("Capacity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
        }

        /// <summary>
        /// Create the count function for the type specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The newly created count function or null.
        /// </returns>
        private static Func<object, object> CreateCountMethod(Type type)
        {
            return DelegateFactory.CreateGetterFunc(type.GetProperty("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
        }

        /// <summary>
        /// Create the indexer action for the type specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The newly created indexer action or null.
        /// </returns>
        private static Action<object, int, object> CreateIndexerMethod(Type type)
        {
            return
                DelegateFactory.CreateIndexerAction(type.GetProperty("Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
        }

        /// <summary>
        /// Create the add action for the type specified.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The newly created add action or null.
        /// </returns>
        private Action<object, object> CreateAddMethod(Type type)
        {
            var methodInfo = type.GetMethod(
                "Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, new[] { this.ElementType }, null);

            if (methodInfo == null)
            {
                return null;
            }

            if (methodInfo.ReturnType == typeof(void))
            {
                return DelegateFactory.CreateAction(methodInfo);
            }

            if ((this.addFunc = DelegateFactory.CreateFunc(methodInfo)) == null)
            {
                return null;
            }

            return this.InvokeAddFunc;
        }

        /// <summary>
        /// Invokes the add function on the target instance specified.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="param">
        /// The value to add.
        /// </param>
        private void InvokeAddFunc(object target, object param)
        {
            this.addFunc(target, param);
        }

        #endregion
    }
}