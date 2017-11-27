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

namespace Hypertable.Persistence.Bindings
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    ///     The key binding.
    /// </summary>
    /// <typeparam name="T">
    ///     The entity type.
    /// </typeparam>
    public sealed class KeyBinding<T> : PartialKeyBinding
        where T : class
    {
        #region Fields

        /// <summary>
        ///     Indicating whether this key binding should generate new keys or not.
        /// </summary>
        private readonly bool generateKey;

        /// <summary>
        ///     The getter function.
        /// </summary>
        private readonly Func<T, string> get;

        /// <summary>
        ///     The setter method.
        /// </summary>
        private readonly Action<T, string> set;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="propertyLambda">
        ///     The property lambda.
        /// </param>
        public KeyBinding(Expression<Func<T, string>> propertyLambda)
            : this(propertyLambda, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="propertyLambda">
        ///     The property lambda.
        /// </param>
        /// <param name="generateKey">
        ///     Indicating whether this key binding should generate new keys or not.
        /// </param>
        public KeyBinding(Expression<Func<T, string>> propertyLambda, bool generateKey)
            : base(typeof(T))
        {
            FromExpression(propertyLambda, out this.get, out this.set);
            this.generateKey = generateKey;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="propertyLambda">
        ///     The property lambda.
        /// </param>
        /// <param name="columnBinding">
        ///     The column binding.
        /// </param>
        public KeyBinding(Expression<Func<T, string>> propertyLambda, IColumnBinding columnBinding)
            : this(propertyLambda, columnBinding, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="propertyLambda">
        ///     The property lambda.
        /// </param>
        /// <param name="columnBinding">
        ///     The column binding.
        /// </param>
        /// <param name="generateKey">
        ///     Indicating whether this key binding should generate new keys or not.
        /// </param>
        public KeyBinding(Expression<Func<T, string>> propertyLambda, IColumnBinding columnBinding, bool generateKey)
            : base(typeof(T), columnBinding)
        {
            FromExpression(propertyLambda, out this.get, out this.set);
            this.generateKey = generateKey;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="get">
        ///     The getter function.
        /// </param>
        /// <param name="set">
        ///     The setter method.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="get" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="set" /> is null.
        /// </exception>
        public KeyBinding(Func<T, string> get, Action<T, string> set)
            : this(get, set, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="get">
        ///     The getter function.
        /// </param>
        /// <param name="set">
        ///     The setter method.
        /// </param>
        /// <param name="generateKey">
        ///     Indicating whether this key binding should generate new keys or not.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="get" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="set" /> is null.
        /// </exception>
        public KeyBinding(Func<T, string> get, Action<T, string> set, bool generateKey)
            : base(typeof(T))
        {
            if (get == null)
            {
                throw new ArgumentNullException(nameof(get));
            }

            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            this.get = get;
            this.set = set;

            this.generateKey = generateKey;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="get">
        ///     The getter function.
        /// </param>
        /// <param name="set">
        ///     The setter method.
        /// </param>
        /// <param name="columnBinding">
        ///     The column binding.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="get" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="set" /> is null.
        /// </exception>
        public KeyBinding(Func<T, string> get, Action<T, string> set, IColumnBinding columnBinding)
            : this(get, set, columnBinding, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyBinding{T}" /> class.
        /// </summary>
        /// <param name="get">
        ///     The getter function.
        /// </param>
        /// <param name="set">
        ///     The setter method.
        /// </param>
        /// <param name="columnBinding">
        ///     The column binding.
        /// </param>
        /// <param name="generateKey">
        ///     Indicating whether this key binding should generate new keys or not.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="get" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="set" /> is null.
        /// </exception>
        public KeyBinding(Func<T, string> get, Action<T, string> set, IColumnBinding columnBinding, bool generateKey)
            : base(typeof(T), columnBinding)
        {
            if (get == null)
            {
                throw new ArgumentNullException(nameof(get));
            }

            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            this.get = get;
            this.set = set;

            this.generateKey = generateKey;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates a database key for the entity specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The database key.
        /// </returns>
        public override Key CreateKey(object entity)
        {
            if (this.generateKey)
            {
                var key = this.GenerateKey(new Key(), entity.GetType());
                this.set((T) entity, key.Row);
                return key;
            }

            return this.KeyFromEntity(entity);
        }

        /// <summary>
        ///     Gets the database key from the entity specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The database key.
        /// </returns>
        public override Key KeyFromEntity(object entity)
        {
            return this.Merge(new Key(this.get((T) entity)));
        }

        /// <summary>
        ///     Gets the database key from the value specified.
        /// </summary>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <returns>
        ///     The database key.
        /// </returns>
        public override Key KeyFromValue(object value)
        {
            var key = value as string;
            return key != null ? this.Merge(new Key(key)) : base.KeyFromValue(value);
        }

        /// <summary>
        ///     Updates the entity using the database key specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="key">
        ///     The database key.
        /// </param>
        public override void SetKey(object entity, Key key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.set((T) entity, key.Row);
            this.Timestamp(entity, key);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the setter and getter from the expression specified.
        /// </summary>
        /// <param name="propertyLambda">
        ///     The property lambda.
        /// </param>
        /// <param name="getter">
        ///     Receives the getter function.
        /// </param>
        /// <param name="setter">
        ///     Receives the setter method.
        /// </param>
        private static void FromExpression(Expression<Func<T, string>> propertyLambda, out Func<T, string> getter,
            out Action<T, string> setter)
        {
            var member = (MemberExpression) propertyLambda.Body;
            var inspector = Inspector.InspectorForType(member.Member.DeclaringType);
            var property = inspector.Properties.FirstOrDefault(p => p.Member == member.Member);
            if (property != null)
            {
                property.Ignore = true;
            }

            DelegateFactory.CreateAccessors(propertyLambda, out getter, out setter);
        }

        #endregion
    }
}