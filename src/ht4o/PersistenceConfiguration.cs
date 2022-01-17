﻿/** -*- C# -*-
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

namespace Hypertable.Persistence
{
    using System;

    /// <summary>
    ///     The persistence configuration.
    /// </summary>
    public sealed class PersistenceConfiguration
    {
        #region Fields

        /// <summary>
        ///     The default mutator specification.
        /// </summary>
        private MutatorSpec mutatorSpec = new MutatorSpec(MutatorKind.Chunked)
        {
            Queued = true,
            MaxChunkSize = 32 * 1024 * 1024,
            FlushEachChunk = true,
            Capacity = 32768
        };

        /// <summary>
        ///     The root namespace.
        /// </summary>
        private string rootNamespace = "/";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistenceConfiguration" /> class.
        /// </summary>
        public PersistenceConfiguration()
        {
            this.Binding = new BindingContext();

            ////this.UseAsyncTableScanner = true;
            this.UseParallelDeserialization = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistenceConfiguration" /> class.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="configuration" /> is null.
        /// </exception>
        public PersistenceConfiguration(PersistenceConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.rootNamespace = configuration.RootNamespace;
            this.Binding = new BindingContext(configuration.Binding);
            this.mutatorSpec = new MutatorSpec(configuration.MutatorSpec);
            this.UseAsyncTableScanner = configuration.UseAsyncTableScanner;
            this.UseParallelDeserialization = configuration.UseParallelDeserialization;
            this.ReviewScanSpec = configuration.ReviewScanSpec;
            this.Context = configuration.Context;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistenceConfiguration" /> class.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="bindingContext">
        ///     The binding context.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="configuration" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="bindingContext" /> is null.
        /// </exception>
        internal PersistenceConfiguration(PersistenceConfiguration configuration, BindingContext bindingContext)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            this.rootNamespace = configuration.RootNamespace;
            this.Binding = bindingContext;
            this.mutatorSpec = new MutatorSpec(configuration.MutatorSpec);
            this.UseAsyncTableScanner = configuration.UseAsyncTableScanner;
            this.UseParallelDeserialization = configuration.UseParallelDeserialization;
            this.ReviewScanSpec = configuration.ReviewScanSpec;
            this.Context = configuration.Context;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets binding context.
        /// </summary>
        /// <value>
        ///     The binding context.
        /// </value>
        public BindingContext Binding { get; }

        /// <summary>
        ///     Gets or sets mutator specification.
        /// </summary>
        /// <value>
        ///     The mutator specification.
        /// </value>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="value" /> is null.
        /// </exception>
        public MutatorSpec MutatorSpec
        {
            get { return this.mutatorSpec; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.mutatorSpec = value;
            }
        }

        /// <summary>
        ///     Gets or sets the review scan spec action, if set the action is called before any scan.
        /// </summary>
        /// <value>
        ///     The scanner callback.
        /// </value>
        /// <remarks>
        ///     Allows scan spec updates right before a table scan starts.
        /// </remarks>
        public Action<ITable, ScanSpec> ReviewScanSpec { get; set; }

        /// <summary>
        ///     Gets or sets the root namespace.
        /// </summary>
        /// <value>
        ///     The root namespace.
        /// </value>
        public string RootNamespace
        {
            get { return this.rootNamespace; }

            set { this.rootNamespace = NormalizeNamespace(value); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to use an async table scanner.
        /// </summary>
        /// <value>
        ///     If <c>true</c> the entity scanner uses an async table scanner, otherwise <c>false</c>.
        /// </value>
        public bool UseAsyncTableScanner { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use parallel deserialization.
        /// </summary>
        /// <value>
        ///     If <c>true</c> the entity scanner may deserialize entities in parallel, otherwise <c>false</c>.
        /// </value>
        public bool UseParallelDeserialization{ get; set; }

        /// <summary>
        ///     Gets or sets a context, any additional information to be associated with the persistence configuration and the streaming context.
        /// </summary>
        /// <value>
        ///     Any additional information to be associated with the persistence configuration and the streaming context.
        /// </value>
        public object Context { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Normalizes the root namespace.
        /// </summary>
        /// <param name="ns">
        ///     The root namespace.
        /// </param>
        /// <returns>
        ///     The normalized root namespace.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="ns" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the trimed <paramref name="ns" /> is empty.
        /// </exception>
        public static string NormalizeNamespace(string ns)
        {
            if (ns == null)
            {
                throw new ArgumentNullException("ns");
            }

            ns = ns.Trim();
            if (string.IsNullOrEmpty(ns))
            {
                throw new ArgumentException(@"Invalid root namespace name", nameof(ns));
            }

            if (ns != "/")
            {
                if (!ns.EndsWith("/", StringComparison.Ordinal))
                {
                    ns += '/';
                }

                if (!ns.StartsWith("/", StringComparison.Ordinal))
                {
                    ns = ns.Insert(0, "/");
                }
            }

            return ns;
        }

        #endregion
    }
}