using System;
using System.IO;
using System.Collections.Generic;
using Platform.VirtualFileSystem.Providers.Imaginary;

namespace Platform.VirtualFileSystem.Providers.MyComputer
{
	public class MyComputerNodeProvider
		: ImaginaryNodeProvider
	{
		public MyComputerNodeProvider(IFileSystemManager manager)
			: this(manager, "mycomputer")
		{
		}

		public MyComputerNodeProvider(IFileSystemManager manager, string scheme)
			: base(manager, scheme)
		{
			PopulateRoot((ImaginaryDirectory)this.ImaginaryFileSystem.RootDirectory);
		}

		public MyComputerNodeProvider(IFileSystemManager manager, ConstructionOptions options)
			: this(manager, options.Scheme.IsNullOrEmpty() ? "mycomputer" : options.Scheme)
		{
		}

		private IEnumerable<Pair<string, INode>> GetDriveNodes(Predicate<INode> acceptNode)
		{
			foreach (var driveInfo in DriveInfo.GetDrives())
			{
				var node = this.Manager.ResolveDirectory(driveInfo.Name);
				var name = driveInfo.Name;

				name = name.TrimRight(Path.DirectorySeparatorChar);

				name = name.Replace(Path.DirectorySeparatorChar, '_');

				name = name.Filter(Local.LocalNodeAddress.IsValidFileNameChar);

				if (acceptNode(node))
				{
					yield return new Pair<string, INode>(name, node);
				}
			}
		}

		private void PopulateRoot(ImaginaryDirectory root)
		{
			int cdindex = 1, portableindex = 1, floppyindex = 1;

			root.DeleteAllJumpPoints();

			foreach (var driveNode in GetDriveNodes(PredicateUtils<INode>.AlwaysTrue))
			{
				root.AddJumpPoint(driveNode.Name, driveNode.Value);

				if ((((string)driveNode.Value.Attributes["DriveType"]) ?? "").Equals("cdrom", StringComparison.CurrentCultureIgnoreCase))
				{
					root.AddJumpPoint("CDROM-" + (cdindex++), driveNode.Value);
				}
				else if ((((string)driveNode.Value.Attributes["DriveType"]) ?? "").Equals("floppy", StringComparison.CurrentCultureIgnoreCase))
				{
					root.AddJumpPoint("FLOPPY-" + (floppyindex++), driveNode.Value);
				}
				else if ((((string)driveNode.Value.Attributes["DriveType"]) ?? "").Equals("removable", StringComparison.CurrentCultureIgnoreCase))
				{
					root.AddJumpPoint("REMOVABLE-" + (portableindex++), driveNode.Value);
				}
			}
		}

		public override INode Find(INodeResolver resolver, string uri, NodeType nodeType, FileSystemOptions options)
		{
			var node = base.Find(resolver, uri, nodeType, options);

			if (node.Equals(this.ImaginaryFileSystem.RootDirectory))
			{
				var dir = (ImaginaryDirectory)node;

				dir.Refreshed += delegate
				{
					PopulateRoot(dir);
				};

				PopulateRoot(dir);
			}

			return node;
		}
	}
}
