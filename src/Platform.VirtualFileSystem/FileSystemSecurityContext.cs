using System;
using System.Linq;
using Platform.Collections;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem
{
	public class FileSystemSecurityContext
		: AccessPermissionVerifier, IDisposable
	{
		public virtual event EventHandler Disposed;

		/// <summary>
		/// Raises the Disposed event.		
		/// </summary>
		/// <param name="eventArgs">The <see cref="EventArgs"/> that contains the event data.</param>
		protected virtual void OnDisposed(EventArgs eventArgs)
		{
			if (Disposed != null)
			{
				Disposed(this, eventArgs);
			}
		}

		/// <summary>
		/// PreviousContext property.
		/// </summary>
		internal virtual FileSystemSecurityContext PreviousContext
		{
			get
			{
				return this.previousContext;
			}
		}
		/// <summary>
		///		<see cref="PreviousContext"/>
		/// </summary>
		internal FileSystemSecurityContext previousContext;
	
		private readonly bool inherit;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="previousContext"></param>
		/// <param name="inherit"></param>
		internal FileSystemSecurityContext(FileSystemSecurityContext previousContext, bool inherit)
		{
			this.inherit = inherit;
			this.previousContext = previousContext;

			this.verifiers = new ArrayList<Pair<object, AccessPermissionVerifier>>();
		}

		public override void Dispose()
		{
			foreach (var pair in this.verifiers)
			{
				try
				{
					pair.Right.Dispose();
				}
				catch
				{
				}
			}

			OnDisposed(EventArgs.Empty);
		}
		private IList<Pair<object, AccessPermissionVerifier>> verifiers;

		internal virtual int VerifiersCount
		{
			get
			{
				return this.verifiers.Count;
			}
		}

		public virtual bool IsEmpty
		{
			get
			{
				return this.verifiers.Count == 0 && (this.PreviousContext == null || this.PreviousContext.IsEmpty);
			}
		}

		public virtual Guid AddPermissionVerifier(AccessPermissionVerifier verifier)
		{
			var guid = new Guid();

			AddPermissionVerifier(guid, verifier);

			return guid;
		}

		private readonly object addRemoveLock = new object();

		public virtual void AddPermissionVerifier(object id, AccessPermissionVerifier verifier)
		{
			IList<Pair<object, AccessPermissionVerifier>> verifiers;

			lock (this.addRemoveLock)
			{
				if (verifier is FileSystemSecurityContext)
				{
					throw new ArgumentException("verifier");
				}

				lock (this.verifiers)
				{
					verifiers = new ArrayList<Pair<object, AccessPermissionVerifier>>(this.verifiers);
				}

				verifiers.Add(new Pair<object, AccessPermissionVerifier>(id, verifier));

				this.verifiers = verifiers;
			}
		}

		public virtual void RemovePermissionVerifier(object id)
		{
			lock (this.addRemoveLock)
			{
				if (this.verifiers == null)
				{
					return;
				}

				var localVerifiers = new ArrayList<Pair<object, AccessPermissionVerifier>>(this.verifiers.Where(x => !x.Left.Equals(id)));

				this.verifiers = localVerifiers;
			}
		}

		public override AccessVerificationResult CheckAccess(AccessVerificationContext context)
		{
			foreach (var verifierEntry in this.verifiers)
			{
				switch (verifierEntry.Right.CheckAccess(context))
				{
					case AccessVerificationResult.Granted:
						return AccessVerificationResult.Granted;
					case AccessVerificationResult.Denied:
						return AccessVerificationResult.Denied;
					case AccessVerificationResult.Undefined:
						continue;
				}
			}

			if (this.inherit && this.PreviousContext != null)
			{
				return this.PreviousContext.CheckAccess(context);
			}

			return AccessVerificationResult.Undefined;
		}
	}
}
