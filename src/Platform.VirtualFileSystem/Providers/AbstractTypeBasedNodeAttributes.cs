using System;
using System.Reflection;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>INodeAttributes</c>interface to minimize the effort 
	/// required to implement the interface.
	/// <seealso cref="INodeAttributes"/>
	/// </summary>
	public abstract class AbstractTypeBasedNodeAttributes
		: AbstractNodeAttributes, INodeAttributes
	{
		private readonly INode node;
		private readonly IList<PropertyInfo> properties;

		protected INode Node
		{
			get
			{
				return this.node;
			}
		}

		protected AbstractTypeBasedNodeAttributes(INode node)
		{
			this.node = node;

			var type = this.GetType();

			var propInfos = type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);

			this.properties = new List<PropertyInfo>(propInfos.Length);

			foreach (var propInfo in propInfos)
			{
				if (HasCustomAttribute(propInfo, typeof(NodeAttributeAttribute)))
				{
					this.properties.Add(propInfo);
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			foreach (var name in this.Names)
			{
				yield return new KeyValuePair<string, object>(name, this[name]);
			}
		}

		[NodeAttribute]
		public override bool? ReadOnly
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		[NodeAttribute]
		public override bool? IsHidden
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		[NodeAttribute]
		public override DateTime? CreationTime
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		[NodeAttribute]
		public override DateTime? LastAccessTime
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		[NodeAttribute]
		public override DateTime? LastWriteTime
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		[NodeAttribute]
		public override bool Exists
		{
			get
			{
				return false;
			}
		}

		void IRefreshable.Refresh()
		{
			Refresh();
		}

		public override INodeAttributes Refresh()
		{
			return this;
		}

		public override IEnumerable<string> Names
		{
			get
			{
				foreach (var propInfo in this.properties)
				{
					yield return propInfo.Name;
				}
			}
		}

		public virtual int Count
		{
			get
			{
				return this.properties.Count;
			}
		}

		public override IEnumerable<object> Values
		{
			get
			{
				foreach (var propInfo in this.properties)
				{
					yield return propInfo.GetValue(this, null);
				}
			}
		}

		public override bool Supports(string name)
		{
			return this[name] != null;
		}

		protected static bool HasCustomAttribute(PropertyInfo propertyInfo, Type attributeType)
		{
			if (propertyInfo.GetCustomAttributes(attributeType, false).Length > 0)
			{
				return true;
			}
			else
			{
				if (propertyInfo.DeclaringType.BaseType == null)
				{
					return false;
				}

				propertyInfo = propertyInfo.DeclaringType.BaseType.GetProperty(propertyInfo.Name);

				if (propertyInfo == null)
				{
					return false;
				}

				return HasCustomAttribute(propertyInfo, attributeType);
			}
		}

		protected override object GetValue(string name)
		{
			switch (name)
			{
				case "CreationTime":
				case "creationtime":
					return this.CreationTime;
				case "LastAccessTime":
				case "lastaccesstime":
					return this.LastAccessTime;
				case "LastWriteTime":
				case "lastwritetime":
					return this.LastWriteTime;
				case "Exists":
				case "exists":
					return this.Exists;
				case "ReadOnly":
				case "readonly":
					return this.ReadOnly;
				case "Hidden":
				case "hidden":
					return this.IsHidden;
			}

			foreach (var propertyInfo in this.properties)
			{
				if (propertyInfo.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
				{
					try
					{
						var value = propertyInfo.GetValue(this, null);

						if (value == null)
						{
							return null;
						}

						if (value.GetType() != propertyInfo.PropertyType
							&& Nullable.GetUnderlyingType(value.GetType()) != propertyInfo.PropertyType
							&& value.GetType() != Nullable.GetUnderlyingType(propertyInfo.PropertyType))
						{
							value = Convert.ChangeType(value, propertyInfo.PropertyType);
						}

						return value;
					}
					catch (TargetInvocationException e)
					{
						throw e.InnerException;
					}
				}
			}

			return null;
		}

		protected override void SetValue(string name, object value)
		{
			switch (name.ToLower())
			{
				case "CreationTime":
				case "creationtime":
					this.CreationTime = (DateTime?)value;
					return;
				case "LastAccessTime":
				case "lastaccesstime":
					this.LastAccessTime = (DateTime?)value;
					return;
				case "LastWriteTime":
				case "lastwritetime":
					this.LastWriteTime = (DateTime?)value;
					return;
				case "ReadOnly":
				case "readonly":
					this.ReadOnly = (bool)value;
					return;
				case "Hidden":
				case "hidden":
					this.IsHidden = (bool)value;
					return;
			}

			foreach (var propertyInfo in this.properties)
			{
				if (propertyInfo.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
				{
					try
					{
						if (value.GetType() != propertyInfo.PropertyType
							&& Nullable.GetUnderlyingType(value.GetType()) != propertyInfo.PropertyType
							&& value.GetType() != Nullable.GetUnderlyingType(propertyInfo.PropertyType))
						{
							value = Convert.ChangeType(value, propertyInfo.PropertyType);
						}

						propertyInfo.SetValue(this, value, null);

						return;
					}
					catch (TargetInvocationException e)
					{
						throw e.InnerException;
					}
				}
			}
		}

		public override object SyncLock
		{
			get
			{
				return this;
			}
		}

		public override IAutoLock GetAutoLock()
		{
			return new AutoLock(this);
		}

		public override INodeAttributesUpdateContext AquireUpdateContext()
		{
			return new StandardNodeAttributesUpdateContext(this);
		}
	}
}
