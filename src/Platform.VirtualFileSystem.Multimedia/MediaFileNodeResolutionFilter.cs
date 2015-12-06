using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Multimedia
{
	public class MediaFileNodeResolutionFilter
		: AbstractNodeResolutionFilter
	{
		public override INode Filter(ref INodeResolver resolver, ref INodeAddress address, ref NodeType nodeType, out bool canCache)
		{
			MediaFileNodeType mediaFileNodeType;

			if ((mediaFileNodeType = (nodeType as MediaFileNodeType)) != null)
			{
				var file = resolver.ResolveFile(address.PathAndQuery);
				var mediaFile = MediaFileFactory.Default.NewMediaFile(file, mediaFileNodeType);

				canCache = !mediaFileNodeType.UniqueInstance;

				return mediaFile;
			}
			else
			{
				canCache = false;

				return null;
			}
		}
	}
}
