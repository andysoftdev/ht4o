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

namespace Hypertable.Persistence
{
    /// <summary>
    ///     The behavior extensions.
    /// </summary>
    public static class BehaviorExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets a value indicating whether the behavior is bypass read cache.
        /// </summary>
        /// <return>
        ///     <c>true</c> if bypass read cache behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool BypassReadCache(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.BypassReadCache) > 0;
        }

        /// <summary>
        ///     Gets a value indicating whether the behavior is bypass write cache.
        /// </summary>
        /// <return>
        ///     <c>true</c> if bypass write cache behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool BypassWriteCache(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.BypassWriteCache) > 0;
        }

        /// <summary>
        ///     Gets a value indicating whether the behavior is don't cache.
        /// </summary>
        /// <return>
        ///     <c>true</c> if don't cache behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool DoNotCache(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.DoNotCache) > 0;
        }

        /// <summary>
        ///     Gets a value indicating whether the behavior is create always.
        /// </summary>
        /// <return>
        ///     <c>true</c> if create always behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool IsCreateAlways(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.CreateBehaviors) == Behaviors.CreateAlways;
        }

        /// <summary>
        ///     Gets a value indicating whether the behavior is create lazy.
        /// </summary>
        /// <return>
        ///     <c>true</c> if create lazy behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool IsCreateLazy(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.CreateBehaviors) == Behaviors.CreateLazy;
        }

        /// <summary>
        ///     Gets a value indicating whether the behavior is create new.
        /// </summary>
        /// <return>
        ///     <c>true</c> if create new behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool IsCreateNew(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.CreateBehaviors) == Behaviors.CreateNew;
        }

        /// <summary>
        ///     Gets a value indicating whether the behavior is default.
        /// </summary>
        /// <return>
        ///     <c>true</c> if default behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool IsDefault(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.CreateBehaviors) == Behaviors.Default;
        }

        /// <summary>
        ///     Gets a value indicating whether the behavior is write new only.
        /// </summary>
        /// <return>
        ///     <c>true</c> if write new only behaviors, otherwise <c>false</c>.
        /// </return>
        public static bool WriteNewOnly(this Behaviors behaviors)
        {
            return (behaviors & Behaviors.WriteNewOnly) > 0;
        }

        #endregion
    }
}