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
namespace Hypertable.Persistence
{
    using System;
    using System.Diagnostics;

#if DEBUG

    using System.Collections.Generic;
    using System.Linq;

#endif

    /// <summary>
    /// The logging.
    /// </summary>
    public static class Logging
    {
        #region Static Fields

        /// <summary>
        /// The synchronization object.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The trace source.
        /// </summary>
        private static TraceSource traceSource;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets trace source.
        /// </summary>
        /// <value>
        /// The trace source.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is null.
        /// </exception>
        public static TraceSource TraceSource
        {
            get
            {
                lock (SyncRoot)
                {
                    return GetTraceSource();
                }
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                lock (SyncRoot)
                {
                    traceSource = value;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Checks if the specified trace event type is enabled.
        /// </summary>
        /// <param name="traceEventType">
        /// The trace event type.
        /// </param>
        /// <returns>
        /// <c>true</c> if  the specified trace event type is enabled, otherwise <c>false</c>.
        /// </returns>
        public static bool IsEnabled(TraceEventType traceEventType)
        {
            lock (SyncRoot)
            {
                var ts = GetTraceSource();
                if (ts != null && ts.Switch != null)
                {
                    try
                    {
                        return ts.Switch.ShouldTrace(traceEventType);
                    }
                    catch
                    {
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Writes a trace message to the trace source.
        /// </summary>
        /// <param name="traceEventType">
        /// The trace event type.
        /// </param>
        /// <param name="message">
        /// The trace message.
        /// </param>
        public static void TraceEvent(TraceEventType traceEventType, string message)
        {
            if (message != null)
            {
                lock (SyncRoot)
                {
                    var ts = GetTraceSource();
                    if (ts != null && ts.Switch != null)
                    {
                        try
                        {
                            if (ts.Switch.ShouldTrace(traceEventType))
                            {
                                ts.TraceEvent(traceEventType, 0, message);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes a trace message to the trace source.
        /// </summary>
        /// <param name="traceEventType">
        /// The trace event type.
        /// </param>
        /// <param name="func">
        /// The trace message function.
        /// </param>
        public static void TraceEvent(TraceEventType traceEventType, Func<string> func)
        {
            if (func != null)
            {
                lock (SyncRoot)
                {
                    var ts = GetTraceSource();
                    if (ts != null && ts.Switch != null)
                    {
                        try
                        {
                            if (ts.Switch.ShouldTrace(traceEventType))
                            {
                                ts.TraceEvent(traceEventType, 0, func());
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes a trace error message to the trace source.
        /// </summary>
        /// <param name="exception">
        /// The exception to trace.
        /// </param>
        public static void TraceException(Exception exception)
        {
            if (exception != null)
            {
                lock (SyncRoot)
                {
                    var ts = GetTraceSource();
                    if (ts != null && ts.Switch != null)
                    {
                        try
                        {
                            if (ts.Switch.ShouldTrace(TraceEventType.Error))
                            {
                                ts.TraceEvent(TraceEventType.Error, 0, exception.ToString());
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the trace source.
        /// </summary>
        /// <returns>
        /// The trace source.
        /// </returns>
        private static TraceSource GetTraceSource()
        {
            if (traceSource == null)
            {
                traceSource = new TraceSource("ht4o", SourceLevels.All);
#if DEBUG
                var listeners = new List<TraceListener>();
                listeners.AddRange(Trace.Listeners.OfType<TraceListener>());
                listeners.AddRange(Debug.Listeners.OfType<TraceListener>());

                foreach (var listener in listeners)
                {
                    if (traceSource.Listeners.OfType<TraceListener>().All(l => l.Name != listener.Name))
                    {
                        traceSource.Listeners.Add(listener);
                    }
                }
#endif
            }

            return traceSource;
        }

        #endregion
    }
}