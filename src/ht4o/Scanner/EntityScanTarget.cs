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
namespace Hypertable.Persistence.Scanner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    using Hypertable;
    using Hypertable.Persistence.Collections;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    /// The entity scan target.
    /// </summary>
    internal class EntityScanTarget : EntitySpec
    {
        #region Fields

        /// <summary>
        /// The scan target setter action.
        /// </summary>
        protected Action<object, object> setter;

        /// <summary>
        /// The scan target.
        /// </summary>
        private readonly object target;

        /// <summary>
        /// The scan target references.
        /// </summary>
        private ICollection<EntityScanTarget> scanTargetRefs;

        /// <summary>
        /// The value.
        /// </summary>
        private object value;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityScanTarget"/> class.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        internal EntityScanTarget(EntityReference entityReference, object key)
            : this(entityReference, entityReference.GetKeyFromObject(key, true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityScanTarget"/> class.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        internal EntityScanTarget(EntityReference entityReference, Key key)
            : base(entityReference, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityScanTarget"/> class.
        /// </summary>
        /// <param name="property">
        /// The inspected property.
        /// </param>
        /// <param name="entitySpec">
        /// The entity specification.
        /// </param>
        /// <param name="target">
        /// The scan target.
        /// </param>
        internal EntityScanTarget(InspectedProperty property, EntitySpec entitySpec, object target)
            : base(property.PropertyType, entitySpec)
        {
            this.target = target;
            this.setter = property.Setter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityScanTarget"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="entitySpec">
        /// The entity spec.
        /// </param>
        /// <param name="setter">
        /// The scan target setter action.
        /// </param>
        internal EntityScanTarget(Type entityType, EntitySpec entitySpec, Action<object, object> setter)
            : base(entityType, entitySpec)
        {
            this.setter = setter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityScanTarget"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="entitySpec">
        /// The entity specification.
        /// </param>
        protected EntityScanTarget(Type entityType, EntitySpec entitySpec)
            : base(entityType, entitySpec)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityScanTarget"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="entitySpec">
        /// The entity specification.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        protected EntityScanTarget(Type entityType, EntitySpec entitySpec, Key key)
            : base(entityType, entitySpec, key)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        internal object Value
        {
            get
            {
                return this.value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a scan target reference.
        /// </summary>
        /// <param name="entityScanTarget">
        /// The entity scan target.
        /// </param>
        internal void AddScanTargetRef(EntityScanTarget entityScanTarget)
        {
            if (this.scanTargetRefs == null)
            {
                this.scanTargetRefs = new ChunkedCollection<EntityScanTarget>();
            }

            this.scanTargetRefs.Add(entityScanTarget);
        }

        /// <summary>
        /// Sets the value specified to the scan target and all attached scan target references.
        /// </summary>
        /// <param name="v">
        /// The value.
        /// </param>
        internal void SetValue(object v)
        {
            if (this.setter != null)
            {
                this.setter(this.target, v);
            }

            this.value = v;

            if (this.scanTargetRefs != null)
            {
                foreach (var entityScanTarget in this.scanTargetRefs)
                {
                    entityScanTarget.SetValue(v);
                }
            }
        }

        #endregion
    }
}