using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Thrown when a node to be resolved isn't within the correct AddressScope.
	/// </summary>
	public class AddressScopeValidationException
		: Exception
	{
		public AddressScopeValidationException()
		{
		}

		public AddressScopeValidationException(string path, AddressScope scope)
			: this(path, scope, null)
		{
		}

		public AddressScopeValidationException(string path, AddressScope scope, Exception innerException)
			: base(String.Format("The path \"{0}\" is not valid within the scope \"{1}\"", path, scope), innerException)
		{
		}
	}
}