using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	public interface ILList<T>
		: IList<T>, ILCollection<T>, IReverseEnumerable<T>
	{
		event EventHandler<ListEventArgs<T>> AfterItemChanged;
		event EventHandler<ListEventArgs<T>> BeforeItemChanged;

		T First
		{
			get;
		}

		T Last
		{
			get;
		}

		T RemoveLast();
		T RemoveFirst();

		void Sort(int index, int length, Comparison<T> comparison);
		int SortedSearch(T item, int index, int length, Comparison<T> comparison);

		bool RemoveLast(T item);
		bool RemoveFirst(T item);

		/// <summary>
		/// Finds the first item that matches the given predicate.
		/// </summary>
		/// <param name="accept">A predicate that accepts an item to be returned</param>
		/// <returns>The first item in the collection that is accepted by the predicate</returns>
		/// <exception cref="ItemNotFoundException">A matching item was not found</exception>
		T FindFirst(Predicate<T> accept);

		/// <summary>
		/// Finds the last item that matches the given predicate.
		/// </summary>
		/// <param name="accept">A predicate that accepts an item to be returned</param>
		/// <returns>The first item in the collection that is accepted by the predicate</returns>
		/// <exception cref="ItemNotFoundException">A matching item was not found</exception>
		T FindLast(Predicate<T> accept);

		bool TryFindFirst(Predicate<T> accept, out T retval);
		bool TryFindLast(Predicate<T> accept, out T retval);
		

		void RemoveRange(int startIndex, int count);

		IEnumerable<Pair<int, T>> GetIndexValuePairs();

		void CopyTo(T[] array, int arrayIndex, int collectionIndex, int count);

		new ILList<T> ToReadOnly();
	}
}
