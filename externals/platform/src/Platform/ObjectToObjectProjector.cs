using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Platform.Linq;
using Platform.Reflection;

namespace Platform
{
	public class ObjectToObjectProjector<T, U>
	{
		public static readonly ObjectToObjectProjector<T, U> Default = new ObjectToObjectProjector<T, U>();

		public virtual Func<ProjectionContext, T, U> BuildProjectIntoNew()
		{
			var projectionContextParameter = Expression.Parameter(typeof(ProjectionContext), "projectionContext");
			var sourceParameter = Expression.Parameter(typeof(T), "source");
			
			var body = ProjectTypeTo(projectionContextParameter, sourceParameter, null, typeof(U));

			var lambda = Expression.Lambda(body, projectionContextParameter, sourceParameter);

			var compiledLambda = lambda.Compile();

			return (Func<ProjectionContext, T, U>)compiledLambda;
		}

		public virtual Action<ProjectionContext, T, U> BuildProjectIntoExisting()
		{
			var projectionContextParameter = Expression.Parameter(typeof(ProjectionContext), "projectionContext");
			var sourceParameter = Expression.Parameter(typeof(T), "source");
			var destinationParameter = Expression.Parameter(typeof(U), "destination");

			var body = ProjectTypeTo(projectionContextParameter, sourceParameter, destinationParameter, typeof(U));

			var lambda = Expression.Lambda(body, projectionContextParameter, sourceParameter, destinationParameter);

			var compiledLambda = ExtendedLambdaExpressionCompiler.Compile(lambda);

			return (Action<ProjectionContext, T, U>)compiledLambda;
		}

		private static ConstructorInfo GetListConstructor(Type type)
		{
			foreach (var constructInfo in type.GetConstructors())
			{
				var parameters = constructInfo.GetParameters();

				if (parameters.Length != 1)
				{
					continue;
				}

				if (!typeof(IEnumerable).IsAssignableFrom(parameters[0].ParameterType))
				{
					continue;
				}

				return constructInfo;
			}

			return null;
		}

		protected virtual NewExpression GetNewExpression(Type type)
		{
			if (type.GetConstructor(Type.EmptyTypes) == null)
			{
				return null;
			}

			return Expression.New(type);
		}

		protected virtual Expression ProjectTypeTo(Expression translationContext, Expression sourceExpression, Expression destinationExpression, Type destinationType)
		{
			var bindings = new List<MemberBinding>();
			var newExpression = GetNewExpression(destinationType);

			Func<string, MemberInfo> sourceMemberInfoProvider = (s) =>
			{
				var retval = sourceExpression.Type.GetProperties().FirstOrDefault(c => c.Name == s);

				if (retval != null)
				{
					if ((retval).CanRead)
					{
						return retval;
					}
					else
					{
						return null;
					}
				}

				return sourceExpression.Type.GetField(s);
			};

			var destinationMembers =
				destinationType
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(c => c.CanWrite)
				.Select(c => (MemberInfo)c)
				.Concat(destinationType.GetFields(BindingFlags.Public | BindingFlags.Instance));

			foreach (var memberInfo in destinationMembers)
			{
				var sourceMemberInfo = sourceMemberInfoProvider(memberInfo.Name);
				var returnType = memberInfo.GetMemberReturnType();

				if (sourceMemberInfo == null)
				{
					continue;
				}

				var binding = CreateBinding(sourceExpression, memberInfo, sourceMemberInfo, returnType, translationContext);

				if (binding != null)
				{
					bindings.Add(binding);
				}
			}

			if (destinationExpression == null)
			{
				return Expression.MemberInit(newExpression, bindings);
			}
			else
			{
				return new MemberPopulateExpression(typeof(void), destinationExpression, bindings);
			}
		}

		protected virtual bool IsSimpleType(Type type)
		{
			return type.IsPrimitive 
				|| type == typeof(string) 
				|| type == typeof(Guid) 
				|| type == typeof(DateTime) 
				|| type == typeof(Decimal);
		}

		protected virtual MemberBinding CreateBinding(Expression sourceExpression, MemberInfo destinationMemberInfo, MemberInfo sourceMemberInfo, Type returnType, Expression translationContext)
		{
			if (IsSimpleType(returnType))
			{
				if (returnType != sourceMemberInfo.GetMemberReturnType())
				{
					Expression bindTarget;

					if (sourceMemberInfo.GetMemberReturnType() == returnType)
					{
						bindTarget = Expression.MakeMemberAccess
						(
							sourceExpression,
							sourceMemberInfo
						);
					}
					else
					{
						bindTarget = Expression.Call
						(
							translationContext,
							ProjectionContext.ConvertValueMethod1.MakeGenericMethod(sourceMemberInfo.GetMemberReturnType(), returnType),
							Expression.MakeMemberAccess
							(
								sourceExpression,
								sourceMemberInfo
							)
						);
					}

					return Expression.Bind(destinationMemberInfo, bindTarget);
				}
				else
				{
					return Expression.Bind(destinationMemberInfo, Expression.MakeMemberAccess(sourceExpression, sourceMemberInfo));
				}
			}
			else if ((returnType.IsArray || typeof(IList<>).IsAssignableFromIgnoreGenericParameters(returnType))
			         && typeof(IEnumerable<>).IsAssignableFromIgnoreGenericParameters(sourceMemberInfo.GetMemberReturnType()))
			{
				Expression newSourceExpression;
				var destinationElementType = returnType.GetSequenceElementType(); 
				var sourceElementType = sourceMemberInfo.GetMemberReturnType().GetSequenceElementType();

				if (returnType.IsArray)
				{
					newSourceExpression = Expression.Call
					(
						translationContext,
						ProjectionContext.ConvertToArrayMethod.MakeGenericMethod(sourceElementType, destinationElementType),
						Expression.MakeMemberAccess(sourceExpression, sourceMemberInfo)
					);
				}
				else if ((returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(List<>))
					|| returnType.IsInterface)
				{
					newSourceExpression = Expression.Call
					(
						translationContext,
						ProjectionContext.ConvertToListMethod.MakeGenericMethod(sourceElementType, destinationElementType),
						Expression.MakeMemberAccess(sourceExpression, sourceMemberInfo)
					);
				}
				else
				{
					var convertedEnumerable = Expression.Call
					(
						translationContext,
						ProjectionContext.ConvertToEnumerationMethod.MakeGenericMethod(sourceElementType, destinationElementType),
						Expression.MakeMemberAccess(sourceExpression, sourceMemberInfo)
					);

					newSourceExpression = Expression.New(GetListConstructor(returnType), convertedEnumerable);
				}

				newSourceExpression = Expression.Condition
				(
					Expression.Equal(Expression.MakeMemberAccess(sourceExpression, sourceMemberInfo), Expression.Convert(Expression.Constant(sourceMemberInfo.GetMemberReturnType().GetDefaultValue()), sourceMemberInfo.GetMemberReturnType())),
					Expression.Convert(Expression.Constant(null), newSourceExpression.Type),
					newSourceExpression
				);

				return Expression.Bind(destinationMemberInfo, newSourceExpression);
			}
			else if (GetNewExpression(returnType) != null)
			{
				var newSourceExpression = Expression.MakeMemberAccess(sourceExpression, sourceMemberInfo);

				var bindingValue = Expression.Condition
				(
					Expression.Equal(newSourceExpression, Expression.Constant(newSourceExpression.Type.GetDefaultValue(), newSourceExpression.Type)),
					Expression.Constant(returnType.GetDefaultValue(), returnType),
					ProjectTypeTo(translationContext, newSourceExpression, null, returnType)
				);

				return Expression.Bind(destinationMemberInfo, bindingValue);
			}

			return null;
		}
	}
}
