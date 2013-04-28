using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform
{
	/// <summary>
	/// An <see cref="ITuple"/> that stores four values.
	/// </summary>
	/// <typeparam name="A">The type of the first element</typeparam>
	/// <typeparam name="B">The type of the second element</typeparam>
	/// <typeparam name="C">The type of the third element</typeparam>
	/// <typeparam name="D">The type of the fourth element</typeparam>
	public struct Quad<A, B, C, D>
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
		/// The fourth element in the tuple.
		/// </summary>
		public D Fourth
		{
			get
			{
				return fourth;
			}
			set
			{
				fourth = value;
			}
		}
		private D fourth;

		/// <summary>
		/// The fourth element in the tuple.
		/// </summary>
		public D Last
		{
			get
			{
				return this.fourth;
			}
			set
			{
				this.fourth = value;
			}
		}

		/// <summary>
		/// Gets an element at a given index (0, 1, 2 or 3).
		/// </summary>
		/// <typeparam name="T">The type of element to get</typeparam>
		/// <param name="index">The index of the element to get (0, 1, 2 or 3)</param>
		/// <returns>The element at <see cref="index"/></returns>
		public T GetAt<T>(int index)
		{
			switch (index)
			{
				case 0:
					return (T)(object)this.first;
				case 1:
					return (T)(object)this.second;
				case 2:
					return (T)(object)this.third;
				case 3:
					return (T)(object)this.fourth;
				default:
					throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Returns 4.
		/// </summary>
		public int Size
		{
			get
			{
				return 4;
			}
		}

		/// <summary>
		/// Constructs a new <see cref="Quad{A,B,C,D}"/>.
		/// </summary>
		/// <param name="first">The first element</param>
		/// <param name="second">The second element</param>
		/// <param name="third">The third element</param>
		/// <param name="fourth">The fourth element</param>
		public Quad(A first, B second, C third, D fourth)
		{
			this.first = first;
			this.second = second;
			this.third = third;
			this.fourth = fourth;
		}

		/// <summary>
		/// Gets a hashcode for the quad.  Made up of a combination of 
		/// all the quad's elements' hash codes.
		/// </summary>
		/// <returns>A hashcode</returns>
		public override int GetHashCode()
		{
			return GetHashCode(this.first) ^ GetHashCode(this.second) ^ GetHashCode(this.third) ^ GetHashCode(this.fourth);
		}

		private static int GetHashCode(object obj)
		{
			return obj == null ? 0 : obj.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Quad<A, B, C, D>? objTuple;

			objTuple = obj as Quad<A, B, C, D>?;

			if (objTuple != null)
			{
				return Object.Equals(first, objTuple.Value.first)
				&& Object.Equals(second, objTuple.Value.second)
				&& Object.Equals(third, objTuple.Value.third)
				&& Object.Equals(fourth, objTuple.Value.fourth);
			}

			return false;
		}

		/// <summary>
		/// Supports comparing two quads with the equality operator.
		/// </summary>
		public static bool operator ==(Quad<A, B, C, D> q1, Quad<A, B, C, D> q2)
		{
			return q1.Equals(q2);
		}

		/// <summary>
		/// Supports comparing two quads with the equality operator.
		/// </summary>
		public static bool operator !=(Quad<A, B, C, D> q1, Quad<A, B, C, D> q2)
		{
			return !q1.Equals(q2);
		}
	}
}
