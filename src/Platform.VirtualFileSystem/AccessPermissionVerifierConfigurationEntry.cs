using System;
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

