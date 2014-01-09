using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Platform.VirtualFileSystem
{
	[Serializable]
	public class FileSystemOptions
		: MarshalByRefObject, ICloneable
	{
		public virtual NameValueCollection Variables { get; set; }

		public static FileSystemOptions NewDefault()
		{
			return new FileSystemOptions();
		}

		public static FileSystemOptions NewDefaultWithReadOnly()
		{
			return new FileSystemOptions()
			{
				ReadOnly = true
			};
		}

		public virtual bool ReadOnly { get; set; }

		public virtual IList NodeResolutionFilterTypes { get; set; }

		public virtual IList NodeTransformationFilterTypes { get; set; }

		public virtual IList NodeServiceProviderTypes { get; set; }

		public virtual IList<Type> NodeOperationFilterTypes { get; set; }

		public virtual IList<Type> AccessPermissionVerifierTypes { get; set; }

		public virtual void AddNodeProviderConfiguration(Providers.NodeProviderConfiguration config)
		{
			foreach (var entry in config.NodeOperationFilters)
			{
				this.NodeOperationFilterTypes.Add(entry.Type);
			}

			foreach (var entry in config.AccessPermissionVerifiers)
			{
				this.AccessPermissionVerifierTypes.Add(entry.Type);
			}

			foreach (var var in config.Variables)
			{
				this.Variables[var.Name] = var.Value;
			}
		}

		public FileSystemOptions()
		{
			this.Variables = new NameValueCollection();
			this.NodeCacheType = typeof(DefaultNodeCache);
			this.AccessPermissionVerifierTypes = new List<Type>();

			this.NodeServiceProviderTypes = new ArrayList(ConfigurationSection.Default.NodeServiceProviders);

			this.NodeServiceProviderTypes.Add(typeof(CoreNodeServicesProvider));
			
			this.NodeResolutionFilterTypes = new ArrayList(ConfigurationSection.Default.NodeResolutionFilters.Length);
			
			foreach (var entry in ConfigurationSection.Default.NodeResolutionFilters)
			{
				this.NodeResolutionFilterTypes.Add(entry.Type);
			}

			this.NodeOperationFilterTypes = new List<Type>(ConfigurationSection.Default.NodeOperationFilters.Length);

			AddAll<Type>
			(
				this.NodeOperationFilterTypes,
				ConfigurationSection.Default.NodeOperationFilters.Convert<ConfigurationSection.NodeOperationFilterEntry, Type>(entry => entry.Type)
			);
		}

		public void AddAll<T>(IList<T> list, IEnumerable<T> enumerables)
		{
			foreach (var item in enumerables)
			{
				list.Add(item);
			}
		}

		/// <summary>
		/// Get/Set the <c>Type</c> of the cache <c>NodeCache</c> implementation to use for new file systems.
		/// </summary>
		/// <remarks>
		/// If not set, the default is <see cref="DefaultNodeCache"/>.
		/// </remarks>
		public Type NodeCacheType { get; set; }

		public virtual object Clone()
		{
			var options = (FileSystemOptions)this.MemberwiseClone();

			return options;
		}
	}
}
