using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem
{	
	[XmlElement("Configuration")]
	public class ConfigurationSection
	{
		public static ConfigurationSection Default
		{
			get
			{
				var retval = ConfigurationBlock<ConfigurationSection>.Load("Platform/VirtualFileSystem/Configuration");

				if (retval == null)
				{
					retval = new ConfigurationSection();

					retval.DefaultManager = "Platform.VirtualFileSystem.DefaultFileSystemManager, Platform.VirtualFileSystem";
				}

				return retval;
			}
		}

		[XmlElement, XmlListElement(typeof(NodeProviderConfigEntry), "NodeProvider")]
		public NodeProviderConfigEntry[] DefaultNodeProviders { get; private set; }

		[XmlElement(SerializeAsValueNode = true, ValueNodeAttributeName = "type")]
		public string DefaultManager { get; private set; }

		public class NodeProviderConfigEntry
		{
			[XmlAttribute(MakeNameLowercase = true)]
			public virtual Type Type { get; private set; }

			[XmlElement]
			public virtual XmlNode Configuration { get; private set; }

			private object configurationCache;

			public virtual object LoadConfiguration()
			{
				Type type;
				XmlSerializer<object> serializer;

				if (this.Configuration == null)
				{
					return null;
				}

				if (this.configurationCache == null)
				{
					this.configurationCache = FunctionUtils.VolatileAssign(delegate
							{
								var searchType = this.Type;

								type = null;

								while (searchType != null)
								{
									type = searchType.GetNestedType("Configuration");

									if (type != null)
									{
										break;
									}

									searchType = searchType.BaseType;
								}

								if (type == null)
								{
									return null;
								}

								serializer = XmlSerializer<object>.New(type);

								return serializer.Deserialize(new XmlNodeReader(this.Configuration));
							});
				}

				return this.configurationCache;
			}
		}

		[XmlElement, XmlListElement(typeof(Type), "ServiceProvider", SerializeAsValueNode = true, ValueNodeAttributeName = "type")]
		public Type[] NodeServiceProviders { get; set; }

		[XmlElement("NodeResolutionFilter")]
		public class NodeResolutionFilterEntry
		{
			[XmlAttribute("type")]
			public virtual Type Type { get; set; }

			[XmlAttribute(MakeNameLowercase = true)]
			public virtual string UriFilter { get; set; }
		}

		[XmlElement, XmlListElement(typeof(NodeResolutionFilterEntry), "NodeResolutionFilter")]
		public NodeResolutionFilterEntry[] NodeResolutionFilters { get; private set; }

		[XmlElement("NodeOperationFilter")]
		public class NodeOperationFilterEntry
		{
			[XmlAttribute("type")]
			public Type Type { get; set; }
		}

		[XmlElement, XmlListElement(typeof(NodeOperationFilterEntry), "NodeOperationFilter")]
		public NodeOperationFilterEntry[] NodeOperationFilters { get; private set; }

		public ConfigurationSection()
		{
			this.DefaultManager = typeof(DefaultFileSystemManager).AssemblyQualifiedName;
			this.DefaultNodeProviders = new NodeProviderConfigEntry[0];
			this.NodeOperationFilters = new NodeOperationFilterEntry[0];
			this.NodeResolutionFilters = new NodeResolutionFilterEntry[0];
			this.NodeServiceProviders = new Type[0];
		}
	}
}
