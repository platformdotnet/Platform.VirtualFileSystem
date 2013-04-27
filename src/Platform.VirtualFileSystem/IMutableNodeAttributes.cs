#region License
/*
 * INodeAttributes.cs
 * 
 * Copyright (c) 2004 Thong Nguyen (tum@veridicus.com)
 * 
 * This program is free software; you can redistribute it and/or modify it under
 * the terms of the GNU General Public License as published by the Free Software
 * Foundation; either version 2 of the License, or (at your option) any later
 * version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, write to the Free Software Foundation, Inc., 59 Temple
 * Place, Suite 330, Boston, MA 02111-1307 USA
 * 
 * The license is packaged with the program archive in a file called LICENSE.TXT
 * 
 * You can also view a copy of the license online at:
 * http://www.opensource.org/licenses/gpl-license.php
 */
#endregion

using System;
using Platform;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Represents mutable attributes of a node.
	/// </summary>
	public interface INodeDetails
		: IRefreshable
	{
		/// <summary>
		/// Gets/sets the creation time.
		/// </summary>
		DateTime CreationTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the last access time.
		/// </summary>
		DateTime LastAccessTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/Sets the last write time.
		/// </summary>
		DateTime LastWriteTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets whether this node exists or not.
		/// </summary>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to check for the file's existance.
		/// </exception>
		bool Exists
		{
			get;
		}

		
		/// <summary>
		/// Refresh the attributes of this node.
		/// </summary>
		/// <remarks>
		/// The attributes of nodes  (CreationTime, LastAccessTime, LastWriteTime, Exists) may be cached by some file
		/// systems.  Attributes are implicitly refreshed when an object is resolved but subsequent queries to an 
		/// attribute may be stale unless <c>Refresh</c> is called.  Although the <c>Refresh</c> method exists, it does
		/// not guarantee that the state of attributes will be stable between calls to <c>Refresh</c>.
		/// Attributes may be change value at any time without an explicit call to <c>Refresh</c>.  Two
		/// consecutive calls to <c>LastAccessTime</c> may return completely different values.
		/// </remarks>
		new void Refresh();
	}
}
