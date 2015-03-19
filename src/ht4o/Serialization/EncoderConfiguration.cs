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

namespace Hypertable.Persistence.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines how to serialize DateTime.
    /// </summary>
    public enum DateTimeBehavior
    {
        /// <summary>
        /// Serializes the date time as it is, local or utc.
        /// </summary>
        Default,

        /// <summary>
        /// Converts local date time to utc on serialization and vice versa on deseralization.
        /// </summary>
        /// <remarks>
        /// Has no effect on utc data times.
        /// </remarks>
        Utc
    }

    /// <summary>
    /// The encoder.
    /// </summary>
    public sealed class EncoderConfiguration
    {
        #region Fields

        /// <summary>
        /// The binder.
        /// </summary>
        private SerializationBinder binder;

        /// <summary>
        /// The type writer.
        /// </summary>
        private Action<BinaryWriter, Type, EncoderConfiguration> typeWriter;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderConfiguration"/> class.
        /// </summary>
        public EncoderConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderConfiguration"/> class.
        /// </summary> 
        /// <param name="configuration">
        /// The encoder configuration to take over.
        /// </param>
        internal EncoderConfiguration(EncoderConfiguration configuration)
        {
            if (configuration != null)
            {
                this.binder = configuration.binder;
                this.typeWriter = configuration.typeWriter;

                this.StrictExplicitTypeCodes = configuration.StrictExplicitTypeCodes;
                this.DateTimeBehavior = configuration.DateTimeBehavior;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderConfiguration"/> class.
        /// </summary> 
        /// <param name="configuration">
        /// The encoder configuration to take over.
        /// </param>
        /// <param name="defaultConfiguration">
        /// The encoder default configuration.
        /// </param>
        private EncoderConfiguration(EncoderConfiguration configuration, EncoderConfiguration defaultConfiguration)
            : this(configuration)
        {
            if (defaultConfiguration != null)
            {
                if (this.binder == null)
                {
                    this.binder = defaultConfiguration.binder;
                }

                if (this.typeWriter == null)
                {
                    this.typeWriter = defaultConfiguration.typeWriter;
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the binder.
        /// </summary>
        /// <value>
        /// The binder.
        /// </value>
        public SerializationBinder Binder
        {
            get
            {
                return this.binder;
            }

            set
            {
                if (value != null)
                {
                    this.binder = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the date time behavior.
        /// </summary>
        /// <value>
        /// The date time behavior.
        /// </value>
        public DateTimeBehavior DateTimeBehavior { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to strictly use explicit type codes.
        /// </summary>
        /// <value>
        /// The strict explicit type codes.
        /// </value>
        public bool StrictExplicitTypeCodes { get; set; }

        /// <summary>
        /// Gets or sets the type writer.
        /// </summary>
        /// <value>
        /// The type writer.
        /// </value>
        public Action<BinaryWriter, Type, EncoderConfiguration> TypeWriter
        {
            get
            {
                return this.typeWriter;
            }

            set
            {
                if (value != null)
                {
                    this.typeWriter = value;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of the <see cref="EncoderConfiguration"/> class.
        /// </summary> 
        /// <param name="configuration">
        /// The encoder configuration to take over.
        /// </param>
        internal static EncoderConfiguration CreateFrom(EncoderConfiguration configuration)
        {
            return configuration != null ? new EncoderConfiguration(configuration, Encoder.Configuration) : new EncoderConfiguration(Encoder.Configuration);
        }

        #endregion
    }
}