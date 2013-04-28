using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Linq
{
	public class ExtendedLambdaExpressionCompiler
		: ExpressionVisitor
	{
		private readonly Type delegateType;
		private readonly ILGenerator generator;
		private readonly DynamicMethod dynamicMethod;
		private readonly LambdaExpression lambdaExpression;
		private readonly Dictionary<ParameterExpression, int> parameterIndexes;

		public ExtendedLambdaExpressionCompiler(LambdaExpression lambdaExpression)
		{
			var isAction = false;
			this.lambdaExpression = lambdaExpression;

			switch (lambdaExpression.Parameters.Count)
			{
				case 0:
					if (lambdaExpression.Body.Type == typeof(void))
					{
						isAction = true;
						delegateType = typeof(Action);
					}
					else
					{
						delegateType = typeof(Func<>);
					}
					break;
				case 1:
					if (lambdaExpression.Body.Type == typeof(void))
					{
						isAction = true;
						delegateType = typeof(Action<>);
					}
					else
					{
						delegateType = typeof(Func<,>);
					}
					break;
				case 2:
					if (lambdaExpression.Body.Type == typeof(void))
					{
						isAction = true;
						delegateType = typeof(Action<,>);
					}
					else
					{
						delegateType = typeof(Func<,,>);
					}
					break;
				case 3:
					if (lambdaExpression.Body.Type == typeof(void))
					{
						isAction = true;
						delegateType = typeof(Action<,,>);
					}
					else
					{
						delegateType = typeof(Func<,,,>);
					}
					break;
				case 4:
					if (lambdaExpression.Body.Type == typeof(void))
					{
						isAction = true;
						delegateType = typeof(Action<,,,>);
					}
					else
					{
						delegateType = typeof(Func<,,,,>);
					}
					break;
				default:
					throw new NotSupportedException("LambdaExpression has too many arguments");
			}

			if (isAction)
			{
				delegateType = delegateType.MakeGenericType(lambdaExpression.Parameters.Select(c => c.Type).ToArray());
			}
			else
			{
				delegateType = delegateType.MakeGenericType(lambdaExpression.Parameters.Select(c => c.Type).Append(lambdaExpression.Body.Type).ToArray());
			}

			dynamicMethod = new DynamicMethod("", lambdaExpression.Body.Type, lambdaExpression.Parameters.Select(c => c.Type).ToArray());
			generator = dynamicMethod.GetILGenerator();

			parameterIndexes = new Dictionary<ParameterExpression, int>();

			for (int i = 0; i < lambdaExpression.Parameters.Count; i++)
			{
				parameterIndexes[lambdaExpression.Parameters[i]] = i;
			}
		}

		public virtual Delegate Compile()
		{
			this.Visit(this.lambdaExpression.Body);

			//Console.WriteLine("Ret");
			generator.Emit(OpCodes.Ret);

			return dynamicMethod.CreateDelegate(delegateType);
		}

		protected override Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return expression;
			}

			switch (expression.NodeType)
			{
				case (ExpressionType)MemberPopulateExpression.MemberPopulateExpressionType:
					return VisitMemberPopulate((MemberPopulateExpression)expression);
			}

			return base.Visit(expression);
		}

		protected virtual Expression VisitMemberPopulate(MemberPopulateExpression memberPopulateExpression)
		{
			var local = generator.DeclareLocal(memberPopulateExpression.Source.Type);

			this.Visit(memberPopulateExpression.Source);

			generator.Emit(OpCodes.Stloc, local);

			foreach (var memberBinding in memberPopulateExpression.Bindings)
			{
				if (local.LocalType.IsValueType)
				{
					generator.Emit(OpCodes.Ldloca, local);
				}
				else
				{
					generator.Emit(OpCodes.Ldloc, local);
				}
                
				this.VisitBinding(memberBinding);
			}

			if (memberPopulateExpression.Type != typeof(void))
			{
				generator.Emit(OpCodes.Ldloc, local);
			}
		
			return memberPopulateExpression;
		}

		public static Delegate Compile(LambdaExpression lambdaExpression)
		{
			var compiler = new ExtendedLambdaExpressionCompiler(lambdaExpression);

			return compiler.Compile();
		}

		protected override NewExpression VisitNew(NewExpression expression)
		{
			this.VisitExpressionList(expression.Arguments);
			generator.Emit(OpCodes.Newobj, expression.Type.GetConstructor(expression.Arguments.Select(c => c.Type).ToArray()));
			
			return expression;
		}

		protected override Expression VisitUnary(UnaryExpression unaryExpression)
		{
			if (unaryExpression.NodeType == ExpressionType.Convert)
			{
				this.Visit(unaryExpression.Operand);

				if (unaryExpression.Operand.Type.IsValueType && unaryExpression.Type.IsClass)
				{
					//Console.WriteLine("box " + unaryExpression.Operand.Type);
					generator.Emit(OpCodes.Box, unaryExpression.Operand.Type);

					return unaryExpression;
				}

				if (unaryExpression.Type.IsValueType && unaryExpression.Operand.Type.IsClass)
				{
					//Console.WriteLine("unbox_any " + unaryExpression.Type);
					generator.Emit(OpCodes.Unbox_Any, unaryExpression.Type);

					return unaryExpression;
				}

				if (Nullable.GetUnderlyingType(unaryExpression.Type) != null && Nullable.GetUnderlyingType(unaryExpression.Operand.Type) == null)
				{
					generator.Emit(OpCodes.Newobj, unaryExpression.Type.GetConstructor(new[] {Nullable.GetUnderlyingType(unaryExpression.Type)}));

					return unaryExpression;
				}

				if (Nullable.GetUnderlyingType(unaryExpression.Type) == null && Nullable.GetUnderlyingType(unaryExpression.Operand.Type) != null)
				{
					var local = generator.DeclareLocal(unaryExpression.Operand.Type);
					generator.Emit(OpCodes.Stloc, local);
					generator.Emit(OpCodes.Ldloca, local);
					generator.Emit(OpCodes.Call, unaryExpression.Operand.Type.GetProperty("Value").GetGetMethod());

					return unaryExpression;
				}

				//Console.WriteLine("castclass " + unaryExpression.Type);
				generator.Emit(OpCodes.Castclass, unaryExpression.Type);
			}
			else
			{
				throw new NotSupportedException("UnaryExpression: " + unaryExpression.NodeType);
			}

			return unaryExpression;
		}

		protected override Expression VisitMemberInit(System.Linq.Expressions.MemberInitExpression expression)
		{
			var local = generator.DeclareLocal(expression.NewExpression.Type);

			this.VisitNew(expression.NewExpression);
			
			generator.Emit(OpCodes.Stloc, local);

			foreach (var memberBinding in expression.Bindings)
			{
				generator.Emit(OpCodes.Ldloc, local);

				this.VisitBinding(memberBinding);
			}

			generator.Emit(OpCodes.Ldloc, local);

			return expression;
		}

		protected override MemberBinding VisitBinding(MemberBinding binding)
		{
			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
					return VisitMemberAssignment((MemberAssignment)binding);
				default:
					throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		protected override Expression VisitParameter(ParameterExpression expression)
		{
			//Console.WriteLine("ldarg " + parameterIndexes[expression]);
			generator.Emit(OpCodes.Ldarg, parameterIndexes[expression]);

			return expression;
		}

		protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
		{
			if (!(methodCallExpression.Object == null
				|| (methodCallExpression.Object.NodeType == ExpressionType.Constant && ((ConstantExpression)methodCallExpression.Object).Value == null)))
			{
				this.Visit(methodCallExpression.Object);
			}

			foreach (Expression expression in methodCallExpression.Arguments)
			{
				this.Visit(expression);
			}

			if (methodCallExpression.Method.IsStatic)
			{
				generator.Emit(OpCodes.Call, methodCallExpression.Method);
			}
			else
			{
				generator.Emit(OpCodes.Callvirt, methodCallExpression.Method);
			}

			return methodCallExpression;
		}

		private static readonly MethodInfo TypeGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(RuntimeTypeHandle) }, null);

		protected override Expression VisitConstant(ConstantExpression constantExpression)
		{
			switch (Type.GetTypeCode(constantExpression.Type))
			{
				case TypeCode.Int16:
					generator.Emit(OpCodes.Ldc_I4, (Int16)constantExpression.Value);
					break;
				case TypeCode.Int32:
					generator.Emit(OpCodes.Ldc_I4, (Int32)constantExpression.Value);
					break;
				case TypeCode.Int64:
					generator.Emit(OpCodes.Ldc_I8, (Int64)constantExpression.Value);
					break;
				case TypeCode.String:
					if (constantExpression.Value == null)
					{
						generator.Emit(OpCodes.Ldnull);
					}
					else
					{
						generator.Emit(OpCodes.Ldstr, (string) constantExpression.Value);
					}
					break;
				case TypeCode.UInt16:
					generator.Emit(OpCodes.Ldc_I4, (UInt16)constantExpression.Value);
					break;
				case TypeCode.UInt32:
					generator.Emit(OpCodes.Ldc_I4, (UInt32)constantExpression.Value);
					break;
				case TypeCode.UInt64:
					generator.Emit(OpCodes.Ldc_I8, (UInt64)constantExpression.Value);
					break;
				case TypeCode.Boolean:
					generator.Emit(OpCodes.Ldc_I4, ((bool)constantExpression.Value) ? 1 : 0);
					break;
				case TypeCode.Byte:
					generator.Emit(OpCodes.Ldc_I4, (byte)constantExpression.Value);
					break;
				case TypeCode.Char:
					generator.Emit(OpCodes.Ldc_I4, (char)constantExpression.Value);
					break;
				case TypeCode.Double:
					generator.Emit(OpCodes.Ldc_R8, (double)constantExpression.Value);
					break;
				case TypeCode.Single:
					generator.Emit(OpCodes.Ldc_R4, (float)constantExpression.Value);
					break;
				default:
					if (typeof(Type).IsAssignableFrom(constantExpression.Type))
					{
						generator.Emit(OpCodes.Ldtoken, ((Type)constantExpression.Value));
						generator.Emit(OpCodes.Call, TypeGetTypeFromHandle);

						break;
					}

					if (constantExpression.Value == null)
					{
						if (constantExpression.Type.IsValueType)
						{
							var defaultValue = generator.DeclareLocal(constantExpression.Type);

							generator.Emit(OpCodes.Ldloc, defaultValue);
						}
						else
						{
							generator.Emit(OpCodes.Ldnull);
						}

						break;
					}

					if (constantExpression.Value.GetType().IsValueType)
					{
						var type = constantExpression.Value.GetType();

						var local = generator.DeclareLocal(type);
						generator.Emit(OpCodes.Initobj, local);
						generator.Emit(OpCodes.Ldloc, local);

						break;
					}

					throw new NotSupportedException("ConstantExpression Type: " + constantExpression.Type);
			}

			return constantExpression;
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpression)
		{
			if (binaryExpression.NodeType == ExpressionType.Equal)
			{
				this.Visit(binaryExpression.Left);
				this.Visit(binaryExpression.Right);
                
				var methodInfo = binaryExpression.Left.Type.GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public);

				if (methodInfo != null)
				{
					generator.Emit(OpCodes.Call, methodInfo);
				}
                else
				{
					generator.Emit(OpCodes.Ceq);
				}
			}
			else
			{
				throw new NotSupportedException("BinaryExpression: " + binaryExpression.NodeType);
			}

			return binaryExpression;
		}

		protected override Expression VisitConditional(ConditionalExpression expression)
		{
			var falseLabel = generator.DefineLabel();
			var endLabel = generator.DefineLabel();

			this.Visit(expression.Test);
			generator.Emit(OpCodes.Brfalse, falseLabel);
			this.Visit(expression.IfTrue);
			generator.Emit(OpCodes.Br, endLabel);
			generator.MarkLabel(falseLabel);
			this.Visit(expression.IfFalse);
			generator.MarkLabel(endLabel);

			return expression;
		}

		protected override Expression VisitMemberAccess(MemberExpression memberExpression)
		{
			this.Visit(memberExpression.Expression);

			switch (memberExpression.Member.MemberType)
			{
				case MemberTypes.Field:
					generator.Emit(OpCodes.Ldfld, (FieldInfo)memberExpression.Member);
					break;
				case MemberTypes.Property:
					var method = ((PropertyInfo)memberExpression.Member).GetGetMethod();
					generator.Emit(OpCodes.Callvirt, ((PropertyInfo)memberExpression.Member).GetGetMethod());
					break;
				default:
					throw new NotSupportedException("Unsupported member assignment type: " + memberExpression.Member.MemberType);
			}

			return memberExpression;
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			this.Visit(assignment.Expression);

			switch (assignment.Member.MemberType)
			{
				case MemberTypes.Field:
					generator.Emit(OpCodes.Stfld, (FieldInfo)assignment.Member);
					break;
				case MemberTypes.Property:
					var method = ((PropertyInfo)assignment.Member).GetSetMethod();
					
					if (assignment.Member.DeclaringType.IsValueType)
					{
						generator.Emit(OpCodes.Call, ((PropertyInfo)assignment.Member).GetSetMethod());
					}
					else
					{
						generator.Emit(OpCodes.Callvirt, ((PropertyInfo) assignment.Member).GetSetMethod());
					}
					break;
				default:
					throw new NotSupportedException("Unsupported member assignment type: " + assignment.Member.MemberType);
			}

			return assignment;
		}
	}
}
