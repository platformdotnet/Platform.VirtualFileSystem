using System;
using System.Collections.Generic;
using System.Text;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Providers
{
	[XmlElement("Configuration")]
	public class NodeProviderConfiguration
	{
		[XmlElement]
		public class Variable
		{
			[XmlAttribute(MakeNameLowercase=true)]
			public virtual string Name { get; set; }

			[XmlAttribute(MakeNameLowercase=true)]
			public virtual string Value { get; set; }
		}

		[XmlElement]
		public virtual Variable[] Variables { get; set; }
		
		[XmlElement, XmlListElement(typeof(ConfigurationSection.NodeOperationFilterEntry), "NodeOperationFilter")]
		public virtual ConfigurationSection.NodeOperationFilterEntry[] NodeOperationFilters { get; set; }

		[XmlElement, XmlListElement("AccessPermissionVerifier")]
		public virtual AccessPermissionVerifierConfigurationEntry[] AccessPermissionVerifiers { get; set; }

		public NodeProviderConfiguration()
		{
			this.NodeOperationFilters = new ConfigurationSection.NodeOperationFilterEntry[0];
			this.AccessPermissionVerifiers = new AccessPermissionVerifierConfigurationEntry[0];
			this.Variables = new Variable[0];
		}
	}
}
