using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem
{
	public enum AccessVerificationResult
	{
		Undefined,		
		Granted,
		Denied		
	}

	public abstract class AccessPermissionVerifier
		: IDisposable
	{
		public virtual bool HasAccess(AccessVerificationContext context)
		{
			return HasAccess(context, true);
		}

		public virtual bool HasAccess(AccessVerificationContext context, bool defaultAccess)
		{
			switch (CheckAccess(context))
			{
				case AccessVerificationResult.Granted:
					return true;
				case AccessVerificationResult.Denied:
					return false;
				default:
					return defaultAccess;					
			}
		}

		public abstract AccessVerificationResult CheckAccess(AccessVerificationContext context);

		#region PredicateBased

		private class DelegateBasedAccessPermissionVerifier
			: AccessPermissionVerifier
		{
			private readonly Func<AccessVerificationContext, AccessVerificationResult> verifier;

			public DelegateBasedAccessPermissionVerifier(Func<AccessVerificationContext, AccessVerificationResult> verifier)
			{
				this.verifier = verifier;
			}

			public override AccessVerificationResult CheckAccess(AccessVerificationContext context)
			{
				return verifier(context);
			}
		}

		public static AccessPermissionVerifier FromPredicate(Func<AccessVerificationContext, AccessVerificationResult> verifier)
		{
			return new DelegateBasedAccessPermissionVerifier(verifier);
		}

		public static implicit operator AccessPermissionVerifier(Func<AccessVerificationContext, AccessVerificationResult> verifier)
		{
			return new DelegateBasedAccessPermissionVerifier(verifier);
		}

		#endregion

		public virtual void Dispose()
		{	
		}
	}
}
