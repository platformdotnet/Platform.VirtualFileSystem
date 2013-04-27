using System.IO;

namespace Platform.VirtualFileSystem.Providers.Overlayed
{
	internal class OverlayedNodeContent
		: AbstractNodeContent
	{
		private readonly OverlayedFile file;

		public OverlayedNodeContent(OverlayedFile file)
		{
			this.file = file;
		}

		public override void Delete()
		{
			this.file.GetBaseContent().Delete();
		}

		protected override Stream DoGetInputStream(out string encoding, FileMode mode, FileShare sharing)
		{
			return this.file.GetBaseContent().GetInputStream(out encoding, mode, sharing);
		}

		protected override Stream DoGetOutputStream(string encoding, FileMode mode, FileShare sharing)
		{
			return this.file.GetBaseContent().GetOutputStream(encoding, mode, sharing);
		}
	}
}