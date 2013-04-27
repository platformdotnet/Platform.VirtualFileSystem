namespace Platform.VirtualFileSystem
{
	public class DelegatingResolver
		: AbstractResolver
	{
		private readonly INodeResolver resolver;

		public DelegatingResolver(INodeResolver resolver)
		{
			this.resolver = resolver;
		}

		public override INode Resolve(string uri, NodeType nodeType, AddressScope scope)
		{
			return this.resolver.Resolve(uri, nodeType, scope);
		}
	}
}