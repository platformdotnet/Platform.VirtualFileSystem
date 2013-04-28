using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.Collections
{
	public static class IListUtils
	{
		public static void MergeSort<T>(this IList<T> list)
		{
			MergeSort(list, Comparer<T>.Default);
		}

		public static void MergeSort<T>(this IList<T> list, Comparison<T> comparison)
		{
			MergeSort(list, new ComparisonComparer<T>(comparison));
		}

		public static void MergeSort<T>(this IList<T> list, IComparer<T> comparer)
		{
			var a1 = SubArray(list, 0, list.Count/ 2 - 1);
			var a2 = SubArray(list, list.Count / 2, list.Count - 1);

			if (a1.Count > 1)
			{
				MergeSort(a1, comparer);
			}

			if (a2.Count > 1)
			{
				MergeSort(a2, comparer);
			}
            
			Merge(a1, a2, list, comparer);
		}

		private static void Merge<T>(IList<T> a1, IList<T> a2, IList<T> a, IComparer<T> comparer)
		{
			int i = 0, j = 0, h = 0;

			while (i < a1.Count || j < a2.Count)
			{
				if (i == a1.Count) 
				{
					a[h++] = a2[j++];
				}
				else if (j == a2.Count)
				{
					a[h++] = a1[i++];
				}
				else if (comparer.Compare(a1[i], a2[j]) < 0)
				{
					a[h++] = a1[i++];
				}
				else if (comparer.Compare(a1[i], a2[j]) > 0)
				{
					a[h++] = a2[j++];
				}
				else
				{
                    a[h++] = a1[i++];
					a[h++] = a2[j++];
				}
			}
		}
        
		private static IList<T> SubArray<T>(IList<T> list, int start, int end)
		{
			var retval = new List<T>(end - start + 1);

			for (int i = start; i <= end && i < list.Count; i++)
			{
				retval.Add(list[i]);
			}

			return retval;
		}
	}
}
