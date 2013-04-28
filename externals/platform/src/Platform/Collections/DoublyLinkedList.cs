using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A class that implements a doubly linked list.
	/// </summary>
	/// <typeparam name="T">The type of object stored in the list</typeparam>
	public class DoublyLinkedList<T>
		: AbstractSequentialAccessList<T>
	{
		private class Node
		{
			public Node Next = null;
			public Node Previous = null;
			public T Value = default(T);
		}

		private int count = 0;
		private readonly Node headSentinel;
		private readonly Node tailSentinel;

		public DoublyLinkedList()
		{
			headSentinel = new Node();
			tailSentinel = new Node();

			headSentinel.Next = tailSentinel;
			tailSentinel.Previous = headSentinel;
		}

		public override T this[int index]
		{
			get
			{
			    if (index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
								
				var node = headSentinel.Next;

				for (int i = 0; i < index; i++)
				{
					node = headSentinel.Next;
				}

				return node.Value;
			}
			set
			{
			    if (index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
								
				var node = headSentinel.Next;

				for (int i = 0; i < index; i++)
				{
					node = headSentinel.Next;
				}

				node.Value = value;
			}
		}

		public override bool RemoveLast(T item)
		{
		    if (tailSentinel.Previous == headSentinel)
			{
				return false;
			}

			var node = tailSentinel.Previous;

			while (node != headSentinel)
			{
				if (node.Value.Equals(item))
				{
					node.Previous.Next = node.Next;
					node.Next.Previous = node.Previous;
					count--;

					return true;
				}

				node = node.Previous;
			}

			return false;
		}

		public override bool RemoveFirst(T item)
		{
		    if (tailSentinel.Previous == headSentinel)
			{
				return false;
			}

			var node = headSentinel.Next;

			while (node != tailSentinel)
			{
				if (node.Value.Equals(item))
				{
					node.Previous.Next = node.Next;
					node.Next.Previous = node.Previous;
					count--;

					return true;
				}

				node = node.Next;
			}

			return false;
		}

		public override void RemoveAt(int index)
		{
			Node node;

			if (index >= count)
			{
				throw new ArgumentOutOfRangeException("index");
			}

			if (index <= count / 2)
			{
				node = headSentinel.Next;

				while (node != tailSentinel && index >= 0)
				{
					if (index == 0)
					{
						node.Previous.Next = node.Next;
						node.Next.Previous = node.Previous;
						count--;

						break;
					}

					index--;
					node = node.Next;
				}
			}
			else
			{
				node = tailSentinel.Previous;

				while (node != tailSentinel && index < count)
				{
					if (index == count - 1)
					{
						node.Previous.Next = node.Next;
						node.Next.Previous = node.Previous;
						count--;

						break;
					}

					index++;
					node = node.Previous;
				}
			}
		}

		public override void Clear()
		{
			count = 0;

			headSentinel.Next = tailSentinel;
			tailSentinel.Previous = headSentinel;
		}

		public override void RemoveRange(int startIndex, int endIndex)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override IEnumerator<T> GetReverseEnumerator()
		{
			return GetReverseEnumerable().GetEnumerator();	
		}

		public override IEnumerable<T> GetReverseEnumerable()
		{
			Node node;

			node = tailSentinel.Previous;

			while (node != headSentinel)
			{
				yield return node.Value;

				node = node.Previous;
			}
		}

		public override void Add(T item)
		{
			var node = new Node();
			
			node.Value = item;
			node.Previous = tailSentinel.Previous;
			node.Previous.Next = node;

			node.Next = tailSentinel;
			tailSentinel.Previous = node;

			count++;
		}

		public override int Count
		{
			get
			{
				return count;
			}
		}

		public override IEnumerator<T> GetEnumerator()
		{
		    var node = headSentinel.Next;

		    while (node != tailSentinel)
			{
				yield return node.Value;

				node = node.Next;
			}
		}

	    public override void Insert(int index, T item)
		{
	        if (index <= 0 || index > count)
			{
				throw new ArgumentOutOfRangeException("index", index, "");
			}

			var node = headSentinel.Next;

			for (int i = 0; i < index; i++)
			{
				node = node.Next;
			}

			var newNode = new Node();

			newNode.Value = item;
			newNode.Next = node;
			node.Previous = newNode;
			newNode.Previous = node.Previous;
			newNode.Previous.Next = newNode;
		}

		public override int SortedSearch(T item, int index, int length, Comparison<T> comparison)
		{
			return base.SortedSearch(item, index, length, comparison);
			/*
			int x = 0, y;

			if (length == 0)
			{
				return -1;
			}

			foreach (T value in this)
			{
				if (x >= index + length)
				{
					return -1;
				}

				if (x >= index)
				{
					y = comparison(item, value);

					if (y == 0)
					{
						return x;
					}
					else if (y < 0)
					{
					}
				}

				x++;
			}*/
		}
	}
}
