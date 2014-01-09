using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	[Serializable]
	public class FileSystemOptions
		: MarshalByRefObject
	{
		private static volatile FileSystemOptions DefaultOptions;

		public static FileSystemOptions Default
		{
			get
			{
				if (DefaultOptions == null)
				{
					lock (typeof(FileSystemOptions))
					{
						if (DefaultOptions == null)
						{
							var nodeServiceProviderTypes = new List<Type>(ConfigurationSection.Default.NodeServiceProviders);

							nodeServiceProviderTypes.Add(typeof(CoreNodeServicesProvider));

							FileSystemOptions.DefaultOptions = new FileSystemOptions(typeof(DefaultNodeCache), nodeServiceProviderTypes, ConfigurationSection.Default.NodeResolutionFilters.Select(c => c.Type).ToList(), ConfigurationSection.Default.NodeOperationFilters.Select(c => c.Type).ToList(), new List<Type>(), new FileSystemVariablesCollection())
							{
								IsDefault = true
							};
						}
					}
				}

				return FileSystemOptions.DefaultOptions;
			}
		}

		public bool IsDefault { get; private set; }
		public bool ReadOnly { get; private set; }
		public FileSystemVariablesCollection Variables { get; set; }
		public ReadOnlyCollection<Type> NodeResolutionFilterTypes { get; private set; }
		public ReadOnlyCollection<Type> NodeTransformationFilterTypes { get; private set; }
		public ReadOnlyCollection<Type> NodeServiceProviderTypes { get; private set; }
		public ReadOnlyCollection<Type> NodeOperationFilterTypes { get; private set; }
		public ReadOnlyCollection<Type> AccessPermissionVerifierTypes { get; private set; }

		public override int GetHashCode()
		{
			var retval = 0;

			retval ^= this.IsDefault ? 1 : 0;
			retval ^= this.ReadOnly ? 1 : 0;
			retval ^= this.Variables.Count;
			retval ^= this.NodeResolutionFilterTypes.Count;
			retval ^= this.NodeTransformationFilterTypes.Count;
			retval ^= this.NodeServiceProviderTypes.Count;
			retval ^= this.NodeOperationFilterTypes.Count;
			retval ^= this.AccessPermissionVerifierTypes.Count;

			return retval;
		}
		public override bool Equals(object obj)
		{
			var typedObj = obj as FileSystemOptions;

			if (typedObj == null)
			{
				return false;
			}

			return this.IsDefault == typedObj.IsDefault
			       && this.ReadOnly == typedObj.ReadOnly
			       && this.Variables.CollectionEquals(typedObj.Variables)
			       && this.NodeResolutionFilterTypes.ElementsAreEqual(typedObj.NodeResolutionFilterTypes)
			       && this.NodeTransformationFilterTypes.ElementsAreEqual(typedObj.NodeTransformationFilterTypes)
			       && this.NodeServiceProviderTypes.ElementsAreEqual(typedObj.NodeServiceProviderTypes)
			       && this.NodeOperationFilterTypes.ElementsAreEqual(typedObj.NodeOperationFilterTypes)
				   && this.AccessPermissionVerifierTypes.ElementsAreEqual(typedObj.AccessPermissionVerifierTypes);
		}

		public virtual FileSystemOptions CreateWithAdditionalConfig(NodeProviderConfiguration config)
		{
			var nodeOperationFileTypes = new List<Type>(this.NodeOperationFilterTypes);

			nodeOperationFileTypes.AddRange(config.NodeOperationFilters.Select(c => c.Type));

			var accessPermissionVerifierTypes = new List<Type>(this.AccessPermissionVerifierTypes);

			accessPermissionVerifierTypes.AddRange(config.AccessPermissionVerifiers.Select(c => c.Type));

			var variables = new NameValueCollection(this.Variables);

			foreach (var variable in config.Variables)
			{
				variables[variable.Name] = variable.Value;
			}

			return new FileSystemOptions(this.NodeCacheType, this.NodeServiceProviderTypes, this.NodeResolutionFilterTypes, nodeOperationFileTypes, accessPermissionVerifierTypes, variables)
			{
				IsDefault = false
			};
		}

		private FileSystemOptions(Type nodeCacheType, IList<Type> nodeServiceProviderTypes, IList<Type> nodeResolutionFilterTypes, IList<Type> nodeOperationFilterTypes, IList<Type> accessPermissionVerifierTypes, NameValueCollection variables)
		{
			this.NodeCacheType = nodeCacheType;
			this.Variables = new FileSystemVariablesCollection(variables);
			this.NodeTransformationFilterTypes = new ReadOnlyCollection<Type>(new List<Type>());
			this.AccessPermissionVerifierTypes = new ReadOnlyCollection<Type>(accessPermissionVerifierTypes.Distinct().ToList());
			this.NodeServiceProviderTypes = new ReadOnlyCollection<Type>(nodeServiceProviderTypes.Distinct().ToList());
			this.NodeResolutionFilterTypes = new ReadOnlyCollection<Type>(nodeResolutionFilterTypes.Distinct().ToList());
			this.NodeOperationFilterTypes = new ReadOnlyCollection<Type>(nodeOperationFilterTypes.Distinct().ToList());
		}

		/// <summary>
		/// Get/Set the <c>Type</c> of the cache <c>NodeCache</c> implementation to use for new file systems.
		/// </summary>
		/// <remarks>
		/// If not set, the default is <see cref="DefaultNodeCache"/>.
		/// </remarks>
		public Type NodeCacheType { get; set; }
	}
}
