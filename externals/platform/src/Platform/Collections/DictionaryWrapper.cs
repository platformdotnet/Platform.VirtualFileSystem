using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Wraps a <see cref="ILDictionary{K,V}"/> and delegates to another dictionary.
	/// </summary>
	/// <typeparam name="K">The type of the key</typeparam>
	/// <typeparam name="V">The typ eof the value</typeparam>
	public class DictionaryWrapper<K, V>
		: CollectionWrapper<KeyValuePair<K, V>>, ILDictionary<K, V>
	{
		/// <summary>
		/// An event that is raised before an item in the dictionary is changed.
		/// </summary>
		public virtual event EventHandler<DictionaryEventArgs<K, V>> AfterItemChanged;

		/// <summary>
		/// An event that is raised after an item in the dictionary is changed.
		/// </summary>
		public virtual event EventHandler<DictionaryEventArgs<K, V>> BeforeItemChanged;

		public new ILDictionary<K, V> Wrappee
		{
			get
			{
				return (ILDictionary<K, V>)base.Wrappee;
			}
			protected set
			{
				base.Wrappee = value;

				if (this.Wrappee != null)
				{
					this.Wrappee.AfterItemChanged += delegate(object sender, DictionaryEventArgs<K, V> eventArgs)
					{
						if (this.AfterItemChanged != null)
						{
							this.AfterItemChanged(this, eventArgs);
						}
					};

					this.Wrappee.BeforeItemChanged += delegate(object sender, DictionaryEventArgs<K, V> eventArgs)
					{
						if (this.BeforeItemChanged != null)
						{
							this.BeforeItemChanged(this, eventArgs);
						}
					};
				}
			}
		}

		public DictionaryWrapper(ILDictionary<K, V> wrappee)
			: base(wrappee)
		{			
		}

		#region IDictionary<K,V> Members

		public virtual void Add(K key, V value)
		{
			this.Wrappee.Add(key, value);
		}

		public virtual bool ContainsKey(K key)
		{
			return this.Wrappee.ContainsKey(key);
		}

		ICollection<K> IDictionary<K, V>.Keys
		{
			get
			{
				return ((IDictionary<K, V>)this.Wrappee).Keys;
			}
		}

		public virtual bool Remove(K key)
		{
			return this.Wrappee.Remove(key);
		}

		public virtual bool TryGetValue(K key, out V value)
		{
			return this.Wrappee.TryGetValue(key, out value);
		}

		public virtual bool TryGetValueOrNot(K key, ref V value)
		{
			return this.Wrappee.TryGetValueOrNot(key, ref value);
		}

		ICollection<V> IDictionary<K, V>.Values
		{
			get
			{
				return ((IDictionary<K, V>)this.Wrappee).Values;
			}
		}

		public virtual V this[K key]
		{
			get
			{
				return this.Wrappee[key];
			}
			set
			{
				this.Wrappee[key] = value;
			}
		}

		#endregion

		#region ILDictionary<K,V> Members

		public virtual IEnumerable<K> Keys
		{
			get
			{
				return this.Wrappee.Keys;
			}
		}

		public virtual IEnumerable<V> Values
		{
			get
			{
				return this.Wrappee.Values;
			}
		}

		#endregion

		public new ILDictionary<K, V> ToReadOnly()
		{
			return this.Wrappee.ToReadOnly();
		}
	}
}
