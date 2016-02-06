using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

							FileSystemOptions.DefaultOptions = new FileSystemOptions(typeof(DefaultNodeCache), nodeServiceProviderTypes, ConfigurationSection.Default.NodeResolutionFilters.Select(c => c.Type).ToList(), ConfigurationSection.Default.NodeOperationFilters.Select(c => c.Type).ToList(), new List<Type>(), new FileSystemVariablesCollection(), true);
						}
					}
				}

				return FileSystemOptions.DefaultOptions;
			}
		}

		public bool ReadOnly => false;
		public bool IsDefault { get; }

		public FileSystemVariablesCollection Variables { get; }
		public ReadOnlyCollection<Type> NodeResolutionFilterTypes { get; }
		public ReadOnlyCollection<Type> NodeTransformationFilterTypes { get; }
		public ReadOnlyCollection<Type> NodeServiceProviderTypes { get; }
		public ReadOnlyCollection<Type> NodeOperationFilterTypes { get; }
		public ReadOnlyCollection<Type> AccessPermissionVerifierTypes { get; }
		public Type NodeCacheType { get; set; }

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

			return new FileSystemOptions(this.NodeCacheType, this.NodeServiceProviderTypes, this.NodeResolutionFilterTypes, nodeOperationFileTypes, accessPermissionVerifierTypes, variables, false);
		}

		private FileSystemOptions(Type nodeCacheType, IList<Type> nodeServiceProviderTypes, IList<Type> nodeResolutionFilterTypes, IList<Type> nodeOperationFilterTypes, IList<Type> accessPermissionVerifierTypes, NameValueCollection variables, bool isDefault = false)
		{
			this.IsDefault = IsDefault;
			this.NodeCacheType = nodeCacheType;
			this.Variables = new FileSystemVariablesCollection(variables);
			this.NodeTransformationFilterTypes = new ReadOnlyCollection<Type>(new List<Type>());
			this.AccessPermissionVerifierTypes = new ReadOnlyCollection<Type>(accessPermissionVerifierTypes.Distinct().ToList());
			this.NodeServiceProviderTypes = new ReadOnlyCollection<Type>(nodeServiceProviderTypes.Distinct().ToList());
			this.NodeResolutionFilterTypes = new ReadOnlyCollection<Type>(nodeResolutionFilterTypes.Distinct().ToList());
			this.NodeOperationFilterTypes = new ReadOnlyCollection<Type>(nodeOperationFilterTypes.Distinct().ToList());
		}

		public FileSystemOptions AddVariables(NameValueCollection variables)
		{
			if (variables == null)
			{
				return this;
			}

			var newVariables = new NameValueCollection(this.Variables) { variables };

			return new FileSystemOptions(this.NodeCacheType, this.NodeServiceProviderTypes, this.NodeResolutionFilterTypes, this.NodeOperationFilterTypes, this.AccessPermissionVerifierTypes, newVariables, false);
		}

		private static Dictionary<Type, Action<object, NameValueCollection>> variableAdderFuncs = new Dictionary<Type, Action<object, NameValueCollection>>();

		public FileSystemOptions AddVariables<T>(T variables)
		{
			if (variables == null)
			{
				return this;
			}

			var newVariables = new NameValueCollection(this.Variables);

			Action<object, NameValueCollection> adderFunc;

			if (!variableAdderFuncs.TryGetValue(typeof(T), out adderFunc))
			{
				var param1 = Expression.Parameter(typeof(object));
				var param2 = Expression.Parameter(typeof(NameValueCollection));
				var adderCalls = new List<Expression>();

				var setMethod = typeof(NameValueCollection)
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(c => c.GetIndexParameters().Length == 1)
					.Where(c => c.GetIndexParameters()[0].ParameterType == typeof(string))
					.Where(c => c.Name == "Item")
					.Select(c => c.SetMethod)
					.Single();

				foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					adderCalls.Add(Expression.Call(param2, setMethod, Expression.Constant(property.Name), Expression.Property(Expression.Convert(param1, typeof(T)), property)));
				}

				adderFunc = Expression.Lambda<Action<object, NameValueCollection>>(Expression.Block(adderCalls), param1, param2).Compile();

				variableAdderFuncs = new Dictionary<Type, Action<object, NameValueCollection>>(variableAdderFuncs) { [typeof(T)] = adderFunc };
			}

			adderFunc(variables, newVariables);

			return new FileSystemOptions(this.NodeCacheType, this.NodeServiceProviderTypes, this.NodeResolutionFilterTypes, this.NodeOperationFilterTypes, this.AccessPermissionVerifierTypes, newVariables, false);
		}

		public FileSystemOptions ChangeNodeCacheType(Type nodeCacheType)
		{
			return new FileSystemOptions(nodeCacheType, this.NodeServiceProviderTypes, this.NodeResolutionFilterTypes, this.NodeOperationFilterTypes, this.AccessPermissionVerifierTypes, this.Variables, false);
		}
	}
}
