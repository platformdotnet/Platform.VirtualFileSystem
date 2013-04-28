using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// An interface for platform collections.
	/// </summary>
	/// <typeparam name="T">The tyoe if items stored in the collection</typeparam>
	public interface ILCollection<T>
		: ICollection<T>, ICloneable, ISyncLocked
	{
		event EventHandler<CollectionEventArgs<T>> AfterItemAdded;
		event EventHandler<CollectionEventArgs<T>> BeforeItemAdded;
		event EventHandler<CollectionEventArgs<T>> AfterItemRemoved;
		event EventHandler<CollectionEventArgs<T>> BeforeItemRemoved;
		event EventHandler<CollectionEventArgs<T>> AfterCleared;
		event EventHandler<CollectionEventArgs<T>> BeforeCleared;
				
		bool IsEmpty
		{
			get;
		}

		void AddAll(T[] items, int offset, int count);
		void AddAll(IEnumerable<T> items);		
		int RemoveAll(IEnumerable<T> items);

		bool ContainsAll(IEnumerable<T> items);
		bool ContainsAny(IEnumerable<T> items);

		void ForEach(Action<T> action);
		void ForEach(Action<T> action, Predicate<T> accept);
		
		void Print(Predicate<T> accept);
		void PrintAll();
		
		IEnumerable<T> FindAll(Predicate<T> accept);

		T[] ToArray();
		void CopyTo(T[] array, int arrayIndex, int count);

		IEnumerable<U> ConvertAll<U>(Converter<T, U> converter);

		ILCollection<T> ToReadOnly();
	}
}
