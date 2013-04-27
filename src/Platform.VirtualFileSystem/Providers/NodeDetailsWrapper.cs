#region License
/*
 * NodeDetailsWrapper.cs
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

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// NodeDetailsWrapper.
	/// </summary>
	public class NodeDetailsWrapper
		: INodeDetails
	{
		private INodeDetails m_Wrappee;

		public NodeDetailsWrapper(INodeDetails innerMutableNodeAttributes)
		{
			m_Wrappee = innerMutableNodeAttributes;
		}

		protected virtual INodeDetails Wrappee
		{
			get
			{
				return m_Wrappee;
			}
		}

		#region INodeDetails Members

		public virtual DateTime CreationTime
		{
			get
			{
				return this.Wrappee.CreationTime;
			}
			set
			{
				this.Wrappee.CreationTime = value;
			}
		}

		public virtual DateTime LastAccessTime
		{
			get
			{
				return this.Wrappee.LastAccessTime;
			}
			set
			{
				this.Wrappee.LastAccessTime = value;
			}
		}

		public virtual DateTime LastWriteTime
		{
			get
			{
				return this.Wrappee.LastWriteTime;
			}
			set
			{
				this.Wrappee.LastWriteTime = value;
			}
		}

		public virtual bool Exists
		{
			get
			{
				return this.Wrappee.Exists;
			}
		}

		public virtual void Refresh()
		{
			this.Wrappee.Refresh();
		}

		#endregion

		

		public override bool Equals(object obj)
		{
			return this.Wrappee.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.Wrappee.GetHashCode ();
		}

		public override string ToString()
		{
			return this.Wrappee.ToString();
		}
	}
}
