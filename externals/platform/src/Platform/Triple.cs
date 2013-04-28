using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform
{
	/// <summary>
	/// An <see cref="ITuple"/> that stores three values.
	/// </summary>
	/// <typeparam name="A">The type of the first element</typeparam>
	/// <typeparam name="B">The type of the second element</typeparam>
	/// <typeparam name="C">The type of the third element</typeparam>
	public struct Triple<A, B, C>
		: ITuple
	{
		/// <summary>
		/// The first element in the tuple.
		/// </summary>
		public A First
		{
			get
			{
				return first;
			}
			set
			{
				first = value;
			}
		}
		private A first;

		/// <summary>
		/// The second element in the tuple.
		/// </summary>
		public B Second
		{
			get
			{
				return second;
			}
			set
			{
				second = value;
			}
		}
		private B second;

		/// <summary>
		/// The second element in the tuple.
		/// </summary>
		public B Middle
		{
			get
			{
				return this.second;
			}
			set
			{
				this.second = value;
			}
		}

		/// <summary>
		/// The third element in the tuple.
		/// </summary>
		public C Third
		{
			get
			{
				return third;
			}
			set
			{
				third = value;
			}
		}
		private C third;

		/// <summary>
		/// The third element in the tuple.
		/// </summary>
		public C Last
		{
			get
			{
				return this.third;
			}
			set
			{
				this.third = value;
			}
		}

		/// <summary>
		/// Gets an element at a given index (0, 1 or 2).
		/// </summary>
		/// <typeparam name="T">The type of element to get</typeparam>
		/// <param name="index">The index of the element to get (0, 1 or 2)</param>
		/// <returns>The element at <see cref="index"/></returns>
		public T GetAt<T>(int index)
		{
			switch (index)
			{
				case 0:
					return (T)(object)this.First;
				case 1:
					return (T)(object)this.Second;
				case 2:
					return (T)(object)this.Third;
				default:
					throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Returns 3.
		/// </summary>
		public int Size
		{
			get
			{
				return 3;
			}
		}

		/// <summary>
		/// Constructs a new <see cref="Triple{A,B,C}"/>.
		/// </summary>
		/// <param name="value1">The first element</param>
		/// <param name="value2">The second element</param>
		/// <param name="value3">The third element</param>
		public Triple(A value1, B value2, C value3)
		{
			this.first = value1;
			this.second = value2;
			this.third = value3;
		}

		/// <summary>
		/// Gets a hashcode for the triple.  Made up of a combination of 
		/// all the triple's elements' hash codes.
		/// </summary>
		/// <returns>A hashcode</returns>
		public override int GetHashCode()
		{
			return GetHashCode(this.first) ^ GetHashCode(this.second) ^ GetHashCode(this.third);
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
		/// A Triple is equal to another Triple of all elements in the Triple
		/// are also equal (using the elements' <see cref="object.Equals(object)"/> method.
		/// </summary>
		public override bool Equals(object obj)
		{
			Triple<A, B, C>? tripleObj;

			tripleObj = obj as Triple<A, B, C>?;

			if (tripleObj != null)
			{
				return Object.Equals(this.first, tripleObj.Value.first)
						&& Object.Equals(this.second, tripleObj.Value.second)
						&& Object.Equals(this.third, tripleObj.Value.third);
			}

			return false;
		}

		/// <summary>
		/// Supports comparing two triples with the equality operator.
		/// </summary>
		public static bool operator ==(Triple<A, B, C> t1, Triple<A, B, C> t2)
		{
			return t1.Equals(t2);
		}

		/// <summary>
		/// Supports comparing two triples with the equality operator.
		/// </summary>
		public static bool operator !=(Triple<A, B, C> t1, Triple<A, B, C> t2)
		{
			return !t1.Equals(t2);
		}
	}
}
