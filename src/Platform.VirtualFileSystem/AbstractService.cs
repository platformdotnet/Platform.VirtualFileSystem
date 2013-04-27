using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Summary description for AbstractService.
	/// </summary>
	public abstract class AbstractService
	{
		/// <summary>
		/// Property Name (string)
		/// </summary>
		public string Name
		{
			get
			{
				return m_Name;
			}
			
			set
			{
				m_Name = value;
			}
		}
		/// <summary>
		/// <see cref="%%name:u"/>
		/// </summary>
		private string m_Name;

		protected INode m_ServiceRoot;

		public AbstractService(INode serviceRoot)
		{
			m_ServiceRoot = serviceRoot;
		}
	}
}
