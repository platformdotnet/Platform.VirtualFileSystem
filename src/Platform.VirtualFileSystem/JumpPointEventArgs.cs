using System;

namespace Platform.VirtualFileSystem
{
	public delegate void JumpPointEventHandler(object sender, JumpPointEventArgs eventArgs);

	public class JumpPointEventArgs
		: EventArgs
	{
		public string Name { get; set; }
		public INode Target { get; set; }
		public INode JumpPointNode { get; set; }

		public JumpPointEventArgs(string name, INode target, INode jumpPointNode)
		{
			this.Name = name;
			this.Target = target;
			this.JumpPointNode = jumpPointNode;
		}
	}
}
