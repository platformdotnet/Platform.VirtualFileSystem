using System;
using System.Text.RegularExpressions;
using Platform.Text.RegularExpressions;

namespace Platform.VirtualFileSystem
{
	public static class NodeFilters
	{
		public static Predicate<INode> Any
		{
			get
			{
				return isAnyPredicate;
			}
		}
		private static readonly Predicate<INode> isAnyPredicate = ByNodeType(NodeType.Any);

		public static Predicate<INode> File
		{
			get
			{
				return isFilePredicate;
			}
		}
		private static readonly Predicate<INode> isFilePredicate = ByNodeType(NodeType.File);

		public static Predicate<INode> Directory
		{
			get
			{
				return isDirectoryPredicate;
			}
		}
		private static readonly Predicate<INode> isDirectoryPredicate = ByNodeType(NodeType.Directory);

		public static Predicate<INode> ByNodeType(NodeType nodeType)
		{
			return ByNodeType(value => value.Equals(NodeType.Any) || nodeType.Equals(NodeType.Any) || nodeType.Equals(value));
		}

		public static Predicate<INode> ByNodeTypeExact(NodeType nodeType)
		{
			return ByNodeType(nodeType.Equals);
		}

		public static Predicate<INode> ByNodeType(Predicate<NodeType> acceptNodeType)
		{
			return node => acceptNodeType(node.NodeType);
		}

		public static Predicate<INode> ByNodeName(string name)
		{
			return ByNodeName(name, StringComparer.CurrentCultureIgnoreCase);
		}

		public static Predicate<INode> ByNodeName(string name, StringComparer comparer)
		{
			return ByNodeName(delegate(string value) { return comparer.Compare(value, name) == 0; });
		}

		public static Predicate<INode> ByNodeName(Predicate<string> acceptName)
		{
			return node => acceptName(node.Name);
		}

		public static Predicate<INode> BySimpleRegexNameFilter(string regex)
		{
			return new SimpleRegexNameFilter(regex).Assert;
		}

		public static bool IsSimpleRegexNameFilter(Predicate<INode> filter)
		{
			return filter.Target.GetType() == typeof(SimpleRegexNameFilter);
		}

		public static string GetSimpleRegexNameFilterRegexString(Predicate<INode> filter)
		{
			return ((SimpleRegexNameFilter)filter.Target).regexString;
		}

		private class SimpleRegexNameFilter
		{
			private Regex regex;
			internal readonly string regexString;
			
			public SimpleRegexNameFilter(string regex)
			{
				regexString = regex;
			}

			public bool Assert(INode node)
			{
				if (regex == null)
				{
					regex = RegexCache.Default.Create(regexString, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
				}

				return regex.IsMatch(node.Name);
			}
		}
	}
}
