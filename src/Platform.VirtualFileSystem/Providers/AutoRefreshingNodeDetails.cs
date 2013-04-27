#region License
/*
 * AutoRefreshingNodeDetails.cs
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
	public class AutoRefreshingFileDetails
		: AutoRefreshingNodeDetails, IFileDetails
	{
		public AutoRefreshingFileDetails(INodeDetails details)
			: this(details, -1)
		{
		}

		public AutoRefreshingFileDetails(INodeDetails details, int autoRefreshCount)
			: base(details, autoRefreshCount)
		{
		}
		
		#region IFileDetails Members

		public long Length
		{
			get
			{
				Refresh();

				return ((IFileDetails)this.Wrappee).Length;
			}
		}

		#endregion
	}

	public class AutoRefreshingNodeDetails
		: NodeDetailsWrapper
	{
		private int m_AutoRefreshCount;
		private int m_OrignalAutoRefreshCount;

		/// <summary>
		/// Construct a new <c>AutoRefreshingNodeDetails</c> class that automatically
		/// refreshes the details on every query.
		/// </summary>
		/// <param name="details">The underlying details implementation to use.</param>
		public AutoRefreshingNodeDetails(INodeDetails details)
			: this(details, -1)
		{				
		}

		/// <summary>
		/// Construct a new <c>AutoRefreshingNodeDetails</c> class that automatically
		/// refreshes the details on every query for a set number of queries.
		/// </summary>
		/// <remarks>
		/// When <c>autoRefreshCount</c> is specified
		/// </remarks>
		/// <param name="details">The underlying details implementation to use.</param>
		/// <param name="autoRefreshCount">
		/// The minimum number of future queries in which to auto refresh.  -1 means infininty.
		/// </param>
		public AutoRefreshingNodeDetails(INodeDetails details, int autoRefreshCount)
			: base(details)
		{
			if (autoRefreshCount < -1)
			{
				throw new ArgumentOutOfRangeException("Can't be less than -1", "autoRefreshCount");
			}

			m_OrignalAutoRefreshCount = m_AutoRefreshCount = autoRefreshCount;
		}

		#region INodeDetails Members
		
		/// <summary>
		/// Ensures that the current object will auto refresh for at least the next
		/// <c>autoRefreshCount</c> queries.
		/// </summary>
		/// <remarks>
		/// If the current object doesn't have an <c>autoRefreshCount</c> or if the
		/// current object already has been set with a higher <c>autoRefreshCount</c>
		/// then this method will have no effect.
		/// </remarks>
		/// <param name="autoRefreshCount">The new <c>autoRefreshCount</c>.</param>
		public void EnsureAutoRefreshCount(int autoRefreshCount)
		{
			lock (this)
			{
				if (m_AutoRefreshCount >= autoRefreshCount)
				{
					return;
				}

				m_AutoRefreshCount = autoRefreshCount;
			}
		}

		public override DateTime CreationTime
		{
			get
			{
				lock (this)
				{
					if (m_AutoRefreshCount == -1 || m_AutoRefreshCount > 0)
					{
						if (m_AutoRefreshCount != -1)
						{
							m_AutoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.CreationTime;
				}
			}
			set
			{
				this.Wrappee.CreationTime = value;
			}
		}

		public override DateTime LastAccessTime
		{
			get
			{
				lock (this)
				{
					if (m_AutoRefreshCount == -1 || m_AutoRefreshCount > 0)
					{
						if (m_AutoRefreshCount != -1)
						{
							m_AutoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.LastAccessTime;
				}
			}
			set
			{
				this.Wrappee.LastAccessTime = value;
			}
		}

		public override DateTime LastWriteTime
		{
			get
			{
				lock (this)
				{
					if (m_AutoRefreshCount == -1 || m_AutoRefreshCount > 0)
					{
						if (m_AutoRefreshCount != -1)
						{
							m_AutoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.LastWriteTime;
				}
			}
			set
			{
				this.Wrappee.LastWriteTime = value;
			}
		}

		public override bool Exists
		{
			get
			{
				lock (this)
				{
					if (m_AutoRefreshCount == -1 || m_AutoRefreshCount > 0)
					{
						if (m_AutoRefreshCount != -1)
						{
							m_AutoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.Exists;
				}
			}
		}

		public override void Refresh()
		{
			lock (this)
			{
				if (m_AutoRefreshCount == 0)
				{
					m_AutoRefreshCount = m_OrignalAutoRefreshCount;
				}
			}
		}

		#endregion
	}
}
