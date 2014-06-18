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

using System;
using System.Reflection;
using System.Runtime.InteropServices;

#if X64
[assembly: AssemblyTitle("ht4o (x64)")]
#else

[assembly: AssemblyTitle("ht4o (x86)")]
#endif

[assembly: AssemblyProduct("ht4o")]
[assembly: AssemblyDescription("Hypertable for Objects")]
[assembly: AssemblyCompany("ht4o.softdev.ch")]
[assembly: AssemblyCopyright("Copyright © 2010-2014")]
[assembly: AssemblyVersion("0.9.7.19")]
[assembly: AssemblyFileVersion("0.9.7.19")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("a065c528-0e86-454d-85fb-93f98225a81a")]

#pragma warning disable 1699 // Use command line option '/keyfile' or appropriate project settings instead of 'AssemblyKeyFile'

[assembly: AssemblyKeyFile("../ht4o.snk")]
[assembly: AssemblyDelaySign(false)]

#pragma warning restore 1699