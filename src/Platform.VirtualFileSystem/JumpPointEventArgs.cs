using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem
{
	public delegate void JumpPointEventHandler(object sender, JumpPointEventArgs eventArgs);

	public class JumpPointEventArgs
		: EventArgs
	{
		public virtual string Name { get; set; }
		public virtual INode Target { get; set; }
		public virtual INode JumpPointNode { get; set; }

		public JumpPointEventArgs(string name, INode target, INode jumpPointNode)
		{
			this.Name = name;
			this.Target = target;
			this.JumpPointNode = jumpPointNode;
		}
	}
}
