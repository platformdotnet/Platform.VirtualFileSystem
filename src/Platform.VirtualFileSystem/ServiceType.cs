using System;
using System.Reflection;

namespace Platform.VirtualFileSystem
{
	[Serializable]
	public class ServiceType
		: INamed
	{
		public static ServiceType NativePathServiceType
		{
			get
			{
				return new ServiceType(typeof(INativePathService));
			}
		}

		public static ServiceType NodeDeletingServiceType
		{
			get
			{
				return new NodeDeletingServiceType();
			}
		}

		public static ServiceType RecursiveNodeDeletingServiceType
		{
			get
			{
				return new NodeDeletingServiceType(true);
			}
		}

		public virtual string Name
		{
			get
			{
				return this.RuntimeType.Name;
			}
		}

		public static ServiceType FromRuntimeType(Type runtimeType)
		{
			return new ServiceType(runtimeType);
		}

		public static implicit operator ServiceType(Type runtimeType)
		{
			return FromRuntimeType(runtimeType);
		}

		public virtual Type RuntimeType { get; private set; }

		protected ServiceType()
		{
			this.RuntimeType = GetType();
		}

		public ServiceType(Type runtimeType)
		{
			this.RuntimeType = runtimeType;
		}

		public virtual bool Is(Type type)
		{
			return this.RuntimeType.IsAssignableFrom(type);
		}

		public override bool Equals(object obj)
		{
			var ServiceType = obj as ServiceType;

			if (obj == null)
			{
				return false;
			}

			return this.RuntimeType == ServiceType.RuntimeType;
		}

		public override int GetHashCode()
		{
			return this.RuntimeType.GetHashCode();
		}

		private static FieldInfo[] publicFields;

		public override string ToString()
		{
			if (publicFields == null)
			{
				publicFields = typeof(ServiceType).GetFields(BindingFlags.Static | BindingFlags.Public);
			}

			foreach (var fieldInfo in publicFields)
			{
				var ServiceType = (ServiceType)fieldInfo.GetValue(null);

				if (this.Equals(ServiceType))
				{
					return fieldInfo.Name;
				}
			}

			return this.RuntimeType.Name;
		}
	}
}
