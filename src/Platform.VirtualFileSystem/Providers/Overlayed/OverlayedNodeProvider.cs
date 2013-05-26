using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.VirtualFileSystem.Providers.Overlayed
{
	public class OverlayedNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		/* This needs to be prime */
		private const int MaxFileSystems = 2147483629;

		/* This needs to be prime */
		private const int PrimeAdder = 1147483189;

		/* Total number of file systems */
		private static int fileSystemsCount = 0;

		/* Salt to allow randomness everytime */
		private static int current = Environment.TickCount % (MaxFileSystems - 1);

		public OverlayedNodeProvider(IFileSystemManager manager)
			: base(manager)
		{
		}

		public static string NextUniqueName()
		{
			lock (typeof(OverlayedNodeProvider))
			{
				if (fileSystemsCount >= MaxFileSystems)
				{
					throw new OverflowException("Too many unique file systems.  Overflow in name generation.");
				}

				fileSystemsCount++;

				current = (int)(((long)(current) + (long)PrimeAdder) % (long)MaxFileSystems);

				var name = current.ToString().ToCharArray();

				for (var i = 0; i < name.Length; i++)
				{
					name[i] = (char)((int)name[i] - (int)'0' + (int)'A');
				}

				return new string(name);
			}
		}

		protected override IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options, out bool cache)
		{
			var layeredAddress = (LayeredNodeAddress)rootAddress;
			var components = layeredAddress.InnerUri.Split(';');
			var fileSystems = components.Select(s => this.Manager.Resolve(s).FileSystem).ToList();

			cache = true;

			return new OverlayedFileSystem(fileSystems, options);
		}

		protected override INodeAddress ParseUri(string uri)
		{
			return LayeredNodeAddress.Parse(uri);
		}

		public override string[] SupportedUriSchemas
		{
			get
			{
				return new []
				{
					"overlayed"
				};
			}
		}
	}
}
