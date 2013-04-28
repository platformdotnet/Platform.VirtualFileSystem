using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Platform.Linq
{
	public class MemberPopulateExpression
		: Expression
	{
		public const int MemberPopulateExpressionType = 8192 * 2;

		public Expression Source
		{
			get;
			private set;
		}

		public ReadOnlyCollection<MemberBinding> Bindings
		{
			get;
			private set;
		}

		public MemberPopulateExpression(Type type, Expression source, IEnumerable<MemberBinding> bindings)
			: base((ExpressionType)MemberPopulateExpressionType, type)
		{
			this.Source = source;

			if (bindings is ReadOnlyCollection<MemberBinding>)
			{
				this.Bindings = (ReadOnlyCollection<MemberBinding>)bindings;
			}
			else
			{
				this.Bindings = new ReadOnlyCollection<MemberBinding>(bindings.ToList());
			}
		}

		public MemberPopulateExpression(Type type, Expression source, ReadOnlyCollection<MemberBinding> bindings)
			: this(type, source, (IEnumerable<MemberBinding>)bindings)
		{
		}
	}
}
