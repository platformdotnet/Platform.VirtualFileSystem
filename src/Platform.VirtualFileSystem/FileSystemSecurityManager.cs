using System;
using Platform.Collections;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem
{
	public class FileSystemSecurityManager
		: IDisposable
	{
		[ThreadStatic]
		private FileSystemSecurityContext currentContext;

		public static FileSystemSecurityManager Default
		{
			get
			{
				return null;
			}
		}

		public virtual bool IsActive
		{
			get
			{
				return true;
			}
		}

		public FileSystemSecurityContext GlobalContext { get; private set; }

		public FileSystemSecurityManager()
		{
			this.GlobalContext = new FileSystemSecurityContext(null, false);
		}

		public virtual FileSystemSecurityContext CurrentContext
		{
			get
			{
				if (this.currentContext == null)
				{
					this.currentContext = new FileSystemSecurityContext(this.GlobalContext, true);
				}

				return this.currentContext;
			}
		}

		public virtual FileSystemSecurityContext AcquireSecurityContext(bool inherit)
		{			
			this.currentContext = new FileSystemSecurityContext(this.currentContext, inherit);

			this.currentContext.Disposed += delegate
			{
				this.currentContext = this.currentContext.PreviousContext;
			};

			return this.currentContext;
		}

		public virtual void Dispose()
		{
			this.GlobalContext.Dispose();
		}
	}
}
