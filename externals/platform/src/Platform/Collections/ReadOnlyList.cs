using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A class for creating read-only lists.
	/// </summary>
	/// <typeparam name="T">The type of objects stored in the list</typeparam>
    internal class ReadOnlyList<T>
        : ListWrapper<T>
    {
        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

		public ReadOnlyList(ILList<T> wrappee)
			: base(wrappee)
		{
		}

        public override bool RemoveLast(T item)
        {
            throw new NotSupportedException();
        }

        public override bool RemoveFirst(T item)
        {
            throw new NotSupportedException();
        }

        public override void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public override IEnumerator<T> GetReverseEnumerator()
        {
            throw new NotSupportedException();
        }

        public override void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public override void Add(T item)
        {
            throw new NotSupportedException();
        }        
    }
}
