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
namespace Hypertable.Persistence.Test
{
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;

    using Hypertable;
    using Hypertable.Persistence.Serialization;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The test base.
    /// </summary>
    [TestClass]
    public class TestBase
    {
        #region Static Fields

        /// <summary>
        /// The entity manager factory.
        /// </summary>
        private static EntityManagerFactory emf;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        protected static string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the entity manager factory.
        /// </summary>
        protected static EntityManagerFactory Emf
        {
            get
            {
                if (emf == null)
                {
                    EntityManagerFactory.DefaultConfiguration.RootNamespace = NsName;
                    emf = EntityManagerFactory.CreateEntityManagerFactory(ConnectionString);
                }

                return emf;
            }
        }

        /// <summary>
        /// Gets the namespace name.
        /// </summary>
        protected static string NsName { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The assembly cleanup.
        /// </summary>
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (emf != null)
            {
                emf.Clear();
                emf.Client.DropNamespace(NsName, DropDispositions.Complete);
                emf.Dispose();
            }
        }

        /// <summary>
        /// The assembly initialize.
        /// </summary>
        /// <param name="testContext">
        /// The test context.
        /// </param>
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].Trim();
            NsName = ConfigurationManager.AppSettings["Namespace"].Trim();
            Assert.IsFalse(string.IsNullOrEmpty(NsName));
            Assert.AreNotEqual(NsName, "/"); // avoid using root namespace

            Hypertable.Logging.Logfile = Assembly.GetAssembly(typeof(TestBase)).Location + ".log";
            Hypertable.Logging.LogMessagePublished = message => Trace.WriteLine(message);

            const string Schema =
                "<Schema>" + "<AccessGroup name=\"default\">" + "<ColumnFamily>" + "<Name>e</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>"
                + "<Name>a</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>b</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>"
                + "<ColumnFamily>" + "<Name>c</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "</AccessGroup>" + "</Schema>";

            EnsureTable("EntityA", Schema);
            EnsureTable("EntityB", Schema);
            EnsureTable("TestEntityManager", Schema);
        }

        /// <summary>
        /// The test initialize.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            ClearNamespace();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The clear namespace.
        /// </summary>
        protected static void ClearNamespace()
        {
            Emf.Clear();
            var ns = Emf.RootNamespace;
            foreach (var tableName in ns.GetListing(false).Tables)
            {
                using (var table = ns.OpenTable(tableName))
                {
                    Delete(table);
                }
            }
        }

        /// <summary>
        /// Deletes all cells in the table specified.
        /// </summary>
        /// <param name="table">
        /// Table.
        /// </param>
        protected static void Delete(ITable table)
        {
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }))
            {
                using (var mutator = table.CreateMutator())
                {
                    var cell = new Cell();
                    while (scanner.Move(cell))
                    {
                        mutator.Delete(cell.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Creates and opens a table in the 'test' namespace, drops existing table. Using the specified type name as the table name.
        /// </summary>
        /// <param name="tableName">
        /// Table name.
        /// </param>
        /// <param name="schema">
        /// Table xml schema.
        /// </param>
        protected static void EnsureTable(string tableName, string schema)
        {
            Emf.RootNamespace.DropTable(tableName, DropDispositions.IfExists);
            Emf.RootNamespace.CreateTable(tableName, schema);
        }

        /// <summary>
        /// The test serialization.
        /// </summary>
        /// <param name="t">
        /// The t.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        protected static void TestSerialization<T>(T t)
        {
            var b = Serializer.ToByteArray(t);
            Assert.IsNotNull(b);
            Assert.AreEqual(t, Deserializer.FromByteArray<T>(b));
        }

        #endregion
    }
}