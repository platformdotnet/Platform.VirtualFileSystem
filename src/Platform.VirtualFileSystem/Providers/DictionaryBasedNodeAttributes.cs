using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Providers
{
	public class DictionaryBasedNodeAttributesEventArgs
		: EventArgs
	{
		public virtual string AttributeName { get; set; }

		public virtual object NewValue { get; set; }

		public virtual object OldValue { get; set; }

		public virtual bool UserSet { get; set; }


		public DictionaryBasedNodeAttributesEventArgs(string name, object oldValue, object newValue, bool userSet)
		{
			this.AttributeName = name;
			this.OldValue = oldValue;
			this.NewValue = newValue;
			this.UserSet = userSet;
		}	
	}

	public class DictionaryBasedNodeAttributes
		: AbstractNodeAttributes, IFileAttributes
	{
		private readonly Predicate<string> supportsAttribute;
		private readonly IDictionary<string, object> attributes;

		public virtual event EventHandler<DictionaryBasedNodeAttributesEventArgs> AttributeValueChanged;

		public virtual void OnAttributeValueChanged(DictionaryBasedNodeAttributesEventArgs eventArgs)
		{
			if (AttributeValueChanged != null)
			{
				AttributeValueChanged(this, eventArgs);
			}
		}	

		public virtual Func<string, object, object> AttributeValueGetFilter
		{
			get
			{
				return this.attributeValueGetFilter;
			}
			set
			{
				this.attributeValueGetFilter = value;
			}
		}
		private Func<string, object, object> attributeValueGetFilter;

		public object DefaultAttributeValueGetFilter(string name, object value)
		{
			return value;
		}

		protected virtual Predicate<string> SupportsAttribute
		{
			get
			{
				return this.supportsAttribute;
			}
		}

		public static bool DefaultSupportsAttribute(string attributeName)
		{
			if (attributeName == "CreationTime"
				|| attributeName == "LastAccessTime"
				|| attributeName == "LastWriteTime"
				|| attributeName == "ReadOnly"
				|| attributeName == "Hidden"
			    || attributeName == "Exists")
			{
					return true;
			}

			return typeof(INodeAttributes).GetProperty(attributeName) != null;
		}

		public DictionaryBasedNodeAttributes()
			: this((string attributeName) => true)
		{
		}

		public DictionaryBasedNodeAttributes(Predicate<string> supportsAttribute)
			: this(supportsAttribute, new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase))
		{
		}

		public DictionaryBasedNodeAttributes(Predicate<string> supportsAttribute, IDictionary<string, object> dictionary)
		{
			this.attributes = dictionary;
			this.supportsAttribute = supportsAttribute;
			this.attributeValueGetFilter = DefaultAttributeValueGetFilter;
		}

		#region INodeAttributes Members

	
		public override INodeAttributesUpdateContext AquireUpdateContext()
		{
			return new StandardNodeAttributesUpdateContext(this);
		}

		public virtual void Clear()
		{
			lock (this.SyncLock)
			{
				this.attributes.Clear();
			}
		}

		public override bool? ReadOnly
		{
			get
			{
				return GetValue<bool?>("ReadOnly");
			}
			set
			{
				SetValue<bool?>("ReadOnly", value, true);
			}
		}

		public override bool? IsHidden
		{
			get
			{
				return GetValue<bool?>("Hidden");
			}
			set
			{
				SetValue<bool?>("Hidden", value, true);
			}
		}

		public override DateTime? CreationTime
		{
			get
			{
				return GetValue<DateTime?>("CreationTime");
			}
			set
			{
				SetValue<DateTime?>("CreationTime", value, true);
			}
		}

		public override DateTime? LastAccessTime
		{
			get
			{
				return GetValue<DateTime?>("LastWriteTime");
			}
			set
			{
				SetValue<DateTime?>("LastWriteTime", value, true);
			}
		}

		public override DateTime? LastWriteTime
		{
			get
			{
				return GetValue<DateTime?>("LastWriteTime");
			}
			set
			{
				SetValue<DateTime?>("LastWriteTime", value, true);
			}
		}

		public virtual T GetValue<T>(string attributeName)
		{
			object retval;

			if (this.attributes.TryGetValue(attributeName, out retval))
			{
				if (retval == null)
				{
					return (T)this.AttributeValueGetFilter(attributeName, default(T));
				}
				else
				{
					if (retval == null)
					{
						return (T)this.AttributeValueGetFilter(attributeName, default(T));
					}

					if (typeof(T) == typeof(object))
					{
						return (T)this.AttributeValueGetFilter(attributeName, (T)retval);
					}

					return (T)this.AttributeValueGetFilter(attributeName, (T)retval);
				}
			}

			return (T)this.AttributeValueGetFilter(attributeName, default(T));
		}

		public virtual void SetValue<T>(string attributeName, T value)
		{
			SetValue<T>(attributeName, value, false);
		}

		public virtual void SetValue<T>(string attributeName, T value, bool user)
		{
			object oldValue, newValue;

			if (!SupportsAttribute(attributeName.SplitAroundFirstCharFromLeft(':').Left))
			{
				return;
			}

			if (!this.attributes.TryGetValue(attributeName, out oldValue))
			{
				oldValue = null;
			}

			if (value == null)
			{
				newValue = null;

				this.attributes.Remove(attributeName);
				
				OnAttributeValueChanged(new DictionaryBasedNodeAttributesEventArgs(attributeName, oldValue, newValue, user));
			}
			else if (value.Equals(default(T)))
			{
				newValue = default(T);

				this.attributes[attributeName] = default(T);

				OnAttributeValueChanged(new DictionaryBasedNodeAttributesEventArgs(attributeName, oldValue, newValue, user));
			}
			else
			{				
				if (typeof(T) == typeof(object))
				{
					this.attributes[attributeName] = newValue = value;
				}
				else if (value.GetType() == typeof(T))
				{
					this.attributes[attributeName] = newValue = value;
				}
				else
				{	
					newValue = Convert.ChangeType
					(
						value,
						Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)
					);

					this.attributes[attributeName] = newValue;
				}

				OnAttributeValueChanged(new DictionaryBasedNodeAttributesEventArgs(attributeName, oldValue, newValue, user));
			}
		}

		public override bool Exists
		{
			get
			{
				return GetValue<bool>("Exists");
			}
		}

		protected override object GetValue(string name)
		{
			return GetValue<object>(name);
		}

		protected override void SetValue(string name, object value)
		{
			if (!SupportsAttribute(name))
			{
				return;
			}

			SetValue<object>(name, value, true);
		}

		public override INodeAttributes Refresh()
		{
			return this;
		}

		public override IEnumerable<string> Names
		{
			get
			{
				foreach (KeyValuePair<string, object> pair in this.attributes)
				{
					if (pair.Value != null)
					{
						yield return pair.Key;
					}
				}
			}
		}

		public override IEnumerable<object> Values
		{
			get
			{
				foreach (KeyValuePair<string, object> pair in this.attributes)
				{
					if (pair.Value != null)
					{
						yield return pair.Value;
					}
				}
			}
		}

		public override bool Supports(string name)
		{
			object retval;

			if (!this.attributes.TryGetValue(name, out retval))
			{
				return false;
			}

			return retval != null;
		}

		#endregion

		#region IRefreshable Members

		void IRefreshable.Refresh()
		{
			this.Refresh();
		}

		#endregion

		#region ISyncLocked Members

		public override object SyncLock
		{
			get
			{
				return this;
			}
		}

		public override IAutoLock GetAutoLock()
		{
			if (this.autoLock == null)
			{
				lock (this.SyncLock)
				{
					this.autoLock = FunctionUtils.VolatileAssign(() => new AutoLock(this));
				}
			}

			return this.autoLock;
		}

		private IAutoLock autoLock;

		#endregion

		#region IEnumerable<KeyValuePair<string,object>> Members

		public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			foreach (KeyValuePair<string, object> keyValuePair in this.attributes)
			{
				if (keyValuePair.Value != null)
				{
					yield return keyValuePair;
				}
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IFileAttributes Members

		public virtual long? Length
		{
			get
			{
				return GetValue<long?>("length");
			}
		}

		IFileAttributes IFileAttributes.Refresh()
		{
			Refresh();

			return this;
		}

		#endregion
	}
}
