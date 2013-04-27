namespace Platform.VirtualFileSystem
{
	public static class NodeExtensions
	{
		public static string GetNativePath(this INode node)
		{
			var service = node.GetService<INativePathService>();
			
			return service == null ? null : service.GetNativePath();
		}

		public static string GetNativeShortPath(this INode node)
		{
			var service = node.GetService<INativePathService>();

			return service == null ? null : service.GetNativeShortPath();
		}
	}
}
