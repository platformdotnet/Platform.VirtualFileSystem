using System;
using System.Text.RegularExpressions;
using Platform.Text.RegularExpressions;

namespace Platform.VirtualFileSystem
{
	public class RegexBasedNodePredicateHelper
		: RegexBasedPredicateHelper
	{
		private readonly NodeType nodeType;

		public RegexBasedNodePredicateHelper(string regex)
			: base(regex)
		{
		}

		public RegexBasedNodePredicateHelper(Regex regex)
			: base(regex)
		{
		}

		internal RegexBasedNodePredicateHelper(Regex regex, NodeType nodeType)
			: base(regex)
		{
				this.nodeType = nodeType;
		}

		public static RegexBasedNodePredicateHelper New(string regex)
		{
			return new RegexBasedNodePredicateHelper(regex);
		}

		public virtual bool Accept(INode node)
		{
			if (nodeType != null)
			{
				if (!node.NodeType.Is(nodeType))
				{
					return false;
				}
			}

			return this.Regex.IsMatch(node.Name);
		}

		public static bool IsRegexBasedNodePredicate(Predicate<INode> value)
		{
			return value.Target is RegexBasedNodePredicateHelper;
		}

		public static Regex GetRegexFromRegexBasedNodePredicate(Predicate<INode> value)
		{
			return ((RegexBasedNodePredicateHelper)value.Target).Regex;
		}

		public static bool PredicateNameBasedOnly(Predicate<INode> value)
		{
			return ((RegexBasedNodePredicateHelper)value.Target).nodeType == null;
		}

		public static implicit operator Predicate<INode>(RegexBasedNodePredicateHelper predicateHelper)
		{
			return predicateHelper.Accept;
		}

		public static implicit operator Predicate<IFile>(RegexBasedNodePredicateHelper predicateHelper)
		{
			return new RegexBasedNodePredicateHelper(predicateHelper.Regex, NodeType.File).Accept;
		}

		public static implicit operator Predicate<IDirectory>(RegexBasedNodePredicateHelper predicateHelper)
		{
			return new RegexBasedNodePredicateHelper(predicateHelper.Regex, NodeType.Directory).Accept;
		}
	}
}
