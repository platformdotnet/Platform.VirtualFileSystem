using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Platform.Linq
{
	public abstract class ExpressionVisitor
	{
		protected virtual Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return null;
			}

			switch (expression.NodeType)
			{
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.ArrayLength:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
					return VisitUnary((UnaryExpression)expression);
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.Coalesce:
				case ExpressionType.ArrayIndex:
				case ExpressionType.RightShift:
				case ExpressionType.LeftShift:
				case ExpressionType.ExclusiveOr:
					return VisitBinary((BinaryExpression)expression);
				case ExpressionType.TypeIs:
					return VisitTypeIs((TypeBinaryExpression)expression);
				case ExpressionType.Conditional:
					return VisitConditional((ConditionalExpression)expression);
				case ExpressionType.Constant:
					return VisitConstant((ConstantExpression)expression);
				case ExpressionType.Parameter:
					return VisitParameter((ParameterExpression)expression);
				case ExpressionType.MemberAccess:
					return VisitMemberAccess((MemberExpression)expression);
				case ExpressionType.Call:
					return VisitMethodCall((MethodCallExpression)expression);
				case ExpressionType.Lambda:
					return VisitLambda((LambdaExpression)expression);
				case ExpressionType.New:
					return VisitNew((NewExpression)expression);
				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
					return VisitNewArray((NewArrayExpression)expression);
				case ExpressionType.Invoke:
					return VisitInvocation((InvocationExpression)expression);
				case ExpressionType.MemberInit:
					return VisitMemberInit((MemberInitExpression)expression);
				case ExpressionType.ListInit:
					return VisitListInit((ListInitExpression)expression);
				default:
					throw new Exception(String.Format("Unhandled expression type: '{0}'", expression.NodeType));
			}
		}

		protected virtual MemberBinding VisitBinding(MemberBinding binding)
		{
			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
					return VisitMemberAssignment((MemberAssignment)binding);
				case MemberBindingType.MemberBinding:
					return VisitMemberMemberBinding((MemberMemberBinding)binding);
				case MemberBindingType.ListBinding:
					return VisitMemberListBinding((MemberListBinding)binding);
				default:
					throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
		{
			ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);

			if (arguments != initializer.Arguments)
			{
				return Expression.ElementInit(initializer.AddMethod, arguments);
			}

			return initializer;
		}

		protected virtual Expression VisitUnary(UnaryExpression unaryExpression)
		{
			Expression operand = Visit(unaryExpression.Operand);

			if (operand != unaryExpression.Operand)
			{
				return Expression.MakeUnary(unaryExpression.NodeType, operand, unaryExpression.Type, unaryExpression.Method);
			}

			return unaryExpression;
		}

		protected virtual Expression VisitBinary(BinaryExpression binaryExpression)
		{
			var left = Visit(binaryExpression.Left);
			var right = Visit(binaryExpression.Right);
			var conversion = Visit(binaryExpression.Conversion);

			if (left != binaryExpression.Left || right != binaryExpression.Right || conversion != binaryExpression.Conversion)
			{
				if (binaryExpression.NodeType == ExpressionType.Coalesce && binaryExpression.Conversion != null)
				{
					return Expression.Coalesce(left, right, conversion as LambdaExpression);
				}
				else
				{
					return Expression.MakeBinary(binaryExpression.NodeType, left, right, binaryExpression.IsLiftedToNull, binaryExpression.Method);
				}
			}

			return binaryExpression;
		}

		protected virtual Expression VisitTypeIs(TypeBinaryExpression expression)
		{
			Expression expr = Visit(expression.Expression);

			if (expr != expression.Expression)
			{
				return Expression.TypeIs(expr, expression.TypeOperand);
			}

			return expression;
		}

		protected virtual Expression VisitConstant(ConstantExpression constantExpression)
		{
			return constantExpression;
		}

		protected virtual Expression VisitConditional(ConditionalExpression expression)
		{
			var test = Visit(expression.Test);
			var ifTrue = Visit(expression.IfTrue);
			var ifFalse = Visit(expression.IfFalse);

			if (test != expression.Test || ifTrue != expression.IfTrue || ifFalse != expression.IfFalse)
			{
				return Expression.Condition(test, ifTrue, ifFalse);
			}

			return expression;
		}

		protected virtual Expression VisitParameter(ParameterExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			var exp = Visit(memberExpression.Expression);

			if (exp != memberExpression.Expression)
			{
				return Expression.MakeMemberAccess(exp, memberExpression.Member);
			}

			return memberExpression;
		}

		protected virtual Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			var obj = Visit(methodCallExpression.Object);

			IEnumerable<Expression> args = VisitExpressionList(methodCallExpression.Arguments);

			if (obj != methodCallExpression.Object || args != methodCallExpression.Arguments)
			{
				return Expression.Call(obj, methodCallExpression.Method, args);
			}

			return methodCallExpression;
		}

		protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			List<Expression> list = null;

			if (original == null)
			{
				return original;
			}

			for (int i = 0, n = original.Count; i < n; i++)
			{
				Expression p = Visit(original[i]);

				if (list != null)
				{
					list.Add(p);
				}

				else if (p != original[i])
				{
					list = new List<Expression>(n);

					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}

					list.Add(p);
				}
			}

			if (list != null)
			{
				return list.AsReadOnly();
			}

			return original;
		}

		protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			var e = Visit(assignment.Expression);

			if (e != assignment.Expression)
			{
				return Expression.Bind(assignment.Member, e);
			}

			return assignment;
		}

		protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			var bindings = VisitBindingList(binding.Bindings);

			if (bindings != binding.Bindings)
			{
				return Expression.MemberBind(binding.Member, bindings);
			}

			return binding;
		}

		protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			var initializers = VisitElementInitializerList(binding.Initializers);

			if (initializers != binding.Initializers)
			{
				return Expression.ListBind(binding.Member, initializers);
			}

			return binding;
		}

		protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
		{
			List<MemberBinding> list = null;

			for (int i = 0, n = original.Count; i < n; i++)
			{
				var b = VisitBinding(original[i]);

				if (list != null)
				{
					list.Add(b);
				}
				else if (b != original[i])
				{
					list = new List<MemberBinding>(n);

					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}

					list.Add(b);
				}
			}

			if (list != null)
			{
				return list;
			}

			return original;
		}

		protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
		{
			List<ElementInit> list = null;

			for (int i = 0, n = original.Count; i < n; i++)
			{
				var init = VisitElementInitializer(original[i]);

				if (list != null)
				{
					list.Add(init);
				}

				else if (init != original[i])
				{
					list = new List<ElementInit>(n);

					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}

					list.Add(init);
				}
			}

			if (list != null)
			{
				return list;
			}

			return original;
		}

		protected virtual Expression VisitLambda(LambdaExpression expression)
		{
			var body = Visit(expression.Body);

			if (body != expression.Body)
			{
				return Expression.Lambda(expression.Type, body, expression.Parameters);
			}

			return expression;
		}

		protected virtual NewExpression VisitNew(NewExpression expression)
		{
			var args = VisitExpressionList(expression.Arguments);

			if (args != expression.Arguments)
			{
				if (expression.Members != null)
				{
					return Expression.New(expression.Constructor, args, expression.Members);
				}
				else
				{
					return Expression.New(expression.Constructor, args);
				}
			}

			return expression;
		}

		protected virtual Expression VisitMemberInit(MemberInitExpression expression)
		{
			var n = VisitNew(expression.NewExpression);

			var bindings = VisitBindingList(expression.Bindings);

			if (n != expression.NewExpression || bindings != expression.Bindings)
			{
				return Expression.MemberInit(n, bindings);
			}

			return expression;
		}

		protected virtual Expression VisitListInit(ListInitExpression expression)
		{
			var n = VisitNew(expression.NewExpression);
			var initializers = VisitElementInitializerList(expression.Initializers);

			if (n != expression.NewExpression || initializers != expression.Initializers)
			{
				return Expression.ListInit(n, initializers);
			}

			return expression;
		}

		protected virtual Expression VisitNewArray(NewArrayExpression expression)
		{
			var exprs = VisitExpressionList(expression.Expressions);

			if (exprs != expression.Expressions)
			{
				if (expression.NodeType == ExpressionType.NewArrayInit)
				{
					return Expression.NewArrayInit(expression.Type.GetElementType(), exprs);
				}

				else
				{
					return Expression.NewArrayBounds(expression.Type.GetElementType(), exprs);
				}
			}

			return expression;
		}

		protected virtual Expression VisitInvocation(InvocationExpression expression)
		{
			var args = VisitExpressionList(expression.Arguments);

			var expr = Visit(expression.Expression);

			if (args != expression.Arguments || expr != expression.Expression)
			{
				return Expression.Invoke(expr, args);
			}

			return expression;
		}
	}
}