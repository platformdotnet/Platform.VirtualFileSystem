using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A queue that is based on a <see cref="DoublyLinkedList{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of the objects stored in the list</typeparam>
	public class LinkedListQueue<T>
		: ListBackedQueue<T>
	{
		public LinkedListQueue()
			: base(new DoublyLinkedList<T>())
		{
		}
	}
}
