using System;
using System.Collections.Generic;
using System.Text;
using Platform;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem
{
	[XmlElement("AccessPermissionVerifier")]
	public class AccessPermissionVerifierConfigurationEntry
	{
		[XmlAttribute(MakeNameLowercase = true)]
		public virtual Type Type { get; set; }
	}
}

