namespace Platform.VirtualFileSystem.Providers
{
	public class JumpPointResolver
		: AbstractResolver
	{
		private readonly INodeResolver resolver;

		public JumpPointResolver(INodeResolver resolver)
		{
			this.resolver = resolver;
		}

		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			return this.resolver.Resolve(name, nodeType, scope);
		}
	}
}