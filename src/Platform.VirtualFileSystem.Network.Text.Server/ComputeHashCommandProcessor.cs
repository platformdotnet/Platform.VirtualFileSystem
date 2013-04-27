using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Text;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("COMPUTEHASH", NormalRunLevel.RunLevelName, RandomAccessRunLevel.NAME)]
	public class ComputeHashCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
			: RandomAccessCommandOptions
		{
			[CommandLineOption(0, Required = true)]
			public string Uri = "";

			[CommandLineOption("r", Required = false)]
			public bool Recursive = false;

			[CommandLineOption("dirattribs", Required = false)]
			public string DirAttribs = "";

			[CommandLineOption("fileattribs", Required = false)]
			public string FileAttribs = "";			
		}

		protected class RandomAccessCommandOptions
		{
			[CommandLineOption("t", Required = false)]
			public string NodeType = "f";

			[CommandLineOption("hex", Required = false)]
			public bool SerializeAsHex = false;

			[CommandLineOption("o", Required = false)]
			public long Offset = 0;

			[CommandLineOption("l", Required = false)]
			public long Length = -1;

			[CommandLineOption("a", Required = false)]
			public string Algorithm = "md5";
		}

		public ComputeHashCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			IHashingService service;
			RandomAccessCommandOptions options;
			Stream stream = null;
						
			if (Connection.RunLevel is RandomAccessRunLevel)
			{
				IFile file;

				options = (RandomAccessCommandOptions)LoadOptions(typeof(RandomAccessCommandOptions), (TextCommand)command);

				RandomAccessRunLevel randomAccessRunLevel;

				randomAccessRunLevel = ((RandomAccessRunLevel)Connection.RunLevel);

				file = randomAccessRunLevel.FileNode;

				stream = randomAccessRunLevel.Stream;
				service = (IHashingService)file.GetService(new StreamHashingServiceType(stream, options.Algorithm));
			}
			else
			{
				INode node;
				CommandOptions cmdOptions;

				options = cmdOptions = (CommandOptions)LoadOptions((TextCommand)command);
				
				var nodeType = TextNetworkProtocol.GetNodeType(options.NodeType);

				if (nodeType == NodeType.File)
				{
					node = Connection.FileSystemManager.Resolve(cmdOptions.Uri, nodeType);

					service = (IHashingService)node.GetService(new FileHashingServiceType(options.Algorithm));
				}
				else if (nodeType == NodeType.Directory)
				{
					DirectoryHashingServiceType serviceType;

					node = Connection.FileSystemManager.Resolve(cmdOptions.Uri, nodeType);
					
					serviceType = new DirectoryHashingServiceType(cmdOptions.Recursive, options.Algorithm);

					string[] ss;

					cmdOptions.DirAttribs = cmdOptions.DirAttribs.Trim();

					if (cmdOptions.DirAttribs.Length > 0)
					{
						ss = cmdOptions.DirAttribs.Split(',');

						if (ss.Length > 0)
						{
							serviceType.IncludedDirectoryAttributes = ss;
						}
					}

					cmdOptions.FileAttribs = cmdOptions.FileAttribs.Trim();

					if (cmdOptions.FileAttribs.Length > 0)
					{
						ss = cmdOptions.FileAttribs.Trim().Split(',');

						if (ss.Length > 0)
						{
							serviceType.IncludedFileAttributes = ss;
						}
					}
					
					service = (IHashingService)node.GetService(serviceType);
				}
				else
				{
					throw new NotSupportedException("ComputeHash_NodeType_" + options.NodeType);
				}
			}
						
			var hashResult = service.ComputeHash(options.Offset, options.Length);

			if (stream != null)
			{
				Connection.WriteOk
				(
					"hash", options.SerializeAsHex ? hashResult.TextValue : hashResult.Base64TextValue,
					"offset", hashResult.Offset,
					"length", hashResult.Length,
					"stream-position", stream.Position
				);
			}
			else
			{
				Connection.WriteOk
				(
					"hash", options.SerializeAsHex ? hashResult.TextValue : hashResult.Base64TextValue,
					"offset", hashResult.Offset,
					"length", hashResult.Length
				);
			}

			Connection.Flush();
		}
	}
}
