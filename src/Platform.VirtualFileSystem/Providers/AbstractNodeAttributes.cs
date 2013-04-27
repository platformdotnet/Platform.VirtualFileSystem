using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractNodeAttributes
		: MarshalByRefObject, INodeAttributes
	{
		protected abstract object GetValue(string name);
		protected abstract void SetValue(string name, object value);

		protected virtual object ProcessGetValue(string name)
		{
			var split = name.SplitAroundFirstCharFromRight('|');

			if (split.Right.Length == name.Length)
			{
				split.Left = split.Right;
				split.Right = "";				
			}

			var value = this.GetValue(split.Left);

			if (split.Right == "")
			{
				return value;
			}
						
			switch (split.Right.ToLower())
			{
				case "base64":
					if (value is byte[])
					{
						return Convert.ToBase64String((byte[])value);
					}
					else
					{
						return Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToString()));
					}
				case "string":
				case "utf8encoding":
					if (value is byte[])
					{
						return Encoding.UTF8.GetString((byte[])value);
					}
					else
					{
						return Convert.ToString(value);
					}
				case "utf16":
					if (value is byte[])
					{
						return Encoding.Unicode.GetString((byte[])value);
					}
					else
					{
						return Convert.ToString(value);
					}
				case "int16":
					return Convert.ToInt16(value);
				case "int32":
					return Convert.ToInt32(value);
				case "int64":
					return Convert.ToInt64(value);
				case "datetime":
					return Convert.ToDateTime(value);
				case "secondsresolution":
					DateTime dateTimeValue;

					dateTimeValue = Convert.ToDateTime(value);

					if (split.Right == "secondsresolution")
					{
						return new DateTime(dateTimeValue.Ticks / 10000000 * 10000000);
					}
					else
					{
						return DateTime.Parse(dateTimeValue.ToString(split.Right));
					}
				default:
					return value;
			}
		}

		public virtual INodeAttributesUpdateContext AquireUpdateContext()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public virtual bool? ReadOnly
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public virtual bool? IsHidden
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public virtual DateTime? CreationTime
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public virtual DateTime? LastAccessTime
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public virtual DateTime? LastWriteTime
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public virtual bool Exists
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public virtual object this[string name]
		{
			get
			{
				return ProcessGetValue(name);
			}
			set
			{
				SetValue(name, value);
			}
		}

		public virtual INodeAttributes Refresh()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public virtual IEnumerable<string> Names
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public virtual IEnumerable<object> Values
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public virtual bool Supports(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IRefreshable.Refresh()
		{
			Refresh();
		}

		public virtual object SyncLock
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public virtual IAutoLock GetAutoLock()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return GetAutoLock().Lock();
		}

		public virtual IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
