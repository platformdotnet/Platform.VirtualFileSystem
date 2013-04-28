using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Platform
{
	/// <summary>
	/// A pair structure.
	/// Represents the following pair types: Left/Right, Key/Value, Name/Value, Source/Destination.
	/// </summary>
	/// <typeparam name="A">The type of the first or left element of the pair</typeparam>
	/// <typeparam name="B">The type of the last or right element of the pair</typeparam>
	public struct Pair<A, B>
		: ITuple, IKeyedValued<A, B>
	{
		/// <summary>
		/// Creates a new <see cref="Pair{A,B}"/>
		/// </summary>
		/// <param name="a">The left element of the pair</param>
		/// <param name="b">The right element of the oair</param>
		/// <returns>A new pair</returns>
		public static Pair<A, B> New(A a, B b)
		{
			return new Pair<A, B>(a, b);
		}

		/// <summary>
		/// Returns 2
		/// </summary>
		public int Size
		{
			get
			{
				return 2;
			}
		}

		/// <summary>
		/// Returns the left element of the pair.
		/// </summary>
		public A Source
		{
			get
			{
				return this.Left;
			}
			set
			{
				this.Left = value;
			}
		}

		/// <summary>
		/// Returns the right element of the pair.
		/// </summary>
		public B Destination
		{
			get
			{
				return this.Right;
			}
			set
			{
				this.Right = value;
			}
		}

		/// <summary>
		/// Returns the left element of the pair.
		/// </summary>
		public A Name
		{
			get
			{
				return this.Left;
			}
			set
			{
				this.Left = value;
			}
		}

		/// <summary>
		/// Returns the left element of the pair.
		/// </summary>
		public A Key
		{
			get
			{
				return this.Left;
			}
			set
			{
				this.Left = value;
			}
		}

		/// <summary>
		/// Returns the left element of the pair.
		/// </summary>
		object IKeyed.Key
		{
			get
			{
				return this.Key;
			}
		}

		/// <summary>
		/// Returns the right element of the pair.
		/// </summary>
		public B Value
		{
			get
			{
				return this.Right;
			}
			set
			{
				this.Right = value;
			}
		}

		/// <summary>
		/// Returns the right element of the pair.
		/// </summary>
		object IValued.Value
		{
			get
			{
				return this.Right;
			}
		}

		/// <summary>
		/// Returns the left element of the pair.
		/// </summary>
        public A Left
        {
            get;
            set;
        }

		/// <summary>
		/// Returns the right element of the pair.
		/// </summary>
        public B Right
        {
            get;
            set;
        }

		/// <summary>
		/// Gets a pair element at a given index (0 or 1).
		/// </summary>
		/// <typeparam name="T">The type of element to get</typeparam>
		/// <param name="index">The index of the element to get (0 or 1)</param>
		/// <returns>The element at <see cref="index"/></returns>
		public T GetAt<T>(int index)
		{
			switch (index)
			{
				case 0:
					return (T)(object)this.Left;
				case 1:
					return (T)(object)this.Right;
				default:
					throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Gets a hashcode for the pair.  Made up of a combination of 
		/// all the pair's elements' hash codes.
		/// </summary>
		/// <returns>A hashcode</returns>
		public override int GetHashCode()
		{
			return GetHashCode(this.Left) ^ GetHashCode(this.Right);
		}

		private static int GetHashCode(object obj)
		{
			if (obj == null)
			{
				return 0;
			}
			else
			{
				return obj.GetHashCode();
			}
		}

		/// <summary>
		/// Refer to <see cref="object.Equals(object)"/>.
		/// A Pair is equal to another Pair of all elements in the Pair
		/// are also equal (using the elements' <see cref="object.Equals(object)"/> method.
		/// </summary>
		public override bool Equals(object obj)
		{
			Pair<A, B>? pairObj;

			pairObj = obj as Pair<A, B>?;

			if (pairObj != null)
			{
				return Object.Equals(this.Left, pairObj.Value.Left)
						&& Object.Equals(this.Right, pairObj.Value.Right);
			}

			return false;
		}

		/// <summary>
		/// Constructs a new Pair.
		/// </summary>
		/// <param name="value1">The pair left element</param>
		/// <param name="value2">The pair right element</param>
		public Pair(A value1, B value2)
            : this()
		{
			this.Left = value1;
		    this.Right = value2;
		}

		/// <summary>
		/// Compares two pairs.
		/// </summary>
		public static bool operator ==(Pair<A, B> p1, Pair<A, B> p2)
		{
			return Object.Equals(p1.Left, p2.Left)
				&& Object.Equals(p1.Right, p2.Right);
		}

		/// <summary>
		/// Compares two pairs.
		/// </summary>
		public static bool operator !=(Pair<A, B> p1, Pair<A, B> p2)
		{
			return !(Object.Equals(p1.Left, p2.Left)
				&& Object.Equals(p1.Right, p2.Right));
		}

		/// <summary>
		/// Converts the current pair to a .NET <see cref="KeyValuePair{TKey,TValue}"/>.
		/// </summary>
		/// <returns>A new <see cref="KeyValuePair{TKey,TValue}"/></returns>
		public KeyValuePair<A, B> ToKeyValuePair()
		{
			return this;
		}

		/// <summary>
		/// Converts a <see cref="KeyValuePair{TKey,TValue}"/> into a <see cref="Pair{A,B}"/>.
		/// </summary>
		/// <param name="keyValuePair">The <see cref="KeyValuePair{TKey,TValue}"/></param>
		/// <returns>A new <see cref="Pair{A,B}"/></returns>
		public static Pair<A, B> FromKeyValuePair(KeyValuePair<A, B> keyValuePair)
		{
			return new Pair<A, B>(keyValuePair.Key, keyValuePair.Value);
		}

		/// <summary>
		/// Converts the given pair to a .NET <see cref="KeyValuePair{TKey,TValue}"/>.
		/// </summary>
		/// <returns>A new <see cref="KeyValuePair{TKey,TValue}"/></returns>
		public static KeyValuePair<A, B> ToKeyValuePair(Pair<A, B> pair)
		{
			return new KeyValuePair<A, B>(pair.Key, pair.Value);
		}

		/// <summary>
		/// Implicitly converts a <see cref="KeyValuePair{TKey,TValue}"/> into a <see cref="Pair{A,B}"/>.
		/// </summary>
		/// <returns>A new <see cref="Pair{A,B}"/></returns>
		public static implicit operator Pair<A, B>(KeyValuePair<A, B> keyValuePair)
		{
			return new Pair<A, B>(keyValuePair.Key, keyValuePair.Value);
		}

		/// <summary>
		/// Implicitly converts a <see cref="Pair{A,B}"/> into a <see cref="KeyValuePair{TKey,TValue}"/>.
		/// </summary>
		/// <returns>A new <see cref="KeyValuePair{A,B}"/></returns>
		public static implicit operator KeyValuePair<A, B>(Pair<A, B> pair)
		{
			return new KeyValuePair<A, B>(pair.Key, pair.Value);
		}

		/// <summary>
		/// Converts the given enumeration of <see cref="Pair{A,B}"/> objects into an enumeration
		/// of <see cref="KeyValuePair{TKey,TValue}"/> objects.
		/// </summary>
		/// <param name="enumerable">The enumeration of <see cref="Pair{A,B}"/> to convert</param>
		/// <returns>An enumeration of <see cref="KeyValuePair{TKey,TValue}"/></returns>
		public static IEnumerable<KeyValuePair<A, B>> ToKeyValuePairs(IEnumerable<Pair<A, B>> enumerable)
		{
			return enumerable.Convert<Pair<A, B>, KeyValuePair<A, B>>(ToKeyValuePair);
		}

		/// <summary>
		/// Converts the given enumeration of <see cref="KeyValuePair{TKey,TValue}"/> objects into an enumeration
		/// of <see cref="Pair{A,B}"/> objects.
		/// </summary>
		/// <param name="enumerable">The enumeration of <see cref="KeyValuePair{TKey,TValue}"/> to convert</param>
		/// <returns>An enumeration of <see cref="Pair{A,B}"/></returns>
		public static IEnumerable<Pair<A, B>> FromKeyValuePairs(IEnumerable<KeyValuePair<A, B>> enumerable)
		{
			return enumerable.Convert<KeyValuePair<A, B>, Pair<A, B>>(FromKeyValuePair);
		}

		public static IEnumerable<Pair<string, string>> FromNameValueCollection(NameValueCollection nameValueCollection)
		{
			foreach (string key in nameValueCollection)
			{
				yield return new Pair<string, string>(key, nameValueCollection[(string)key]);
			}
		}

		/// <summary>
		/// Converts this pair into a string.  The string will be of the format
		/// [Left, Right] where Left and Right are the string representations of
		/// <see cref="Left"/> and <see cref="Right"/>.
		/// </summary>
		/// <returns>A string representation of the pair</returns>
		public override string ToString()
		{
			return String.Format("[{0}, {1}]", Left, Right);
		}
	}
}
