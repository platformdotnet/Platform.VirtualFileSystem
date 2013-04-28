using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Platform.References;

namespace Platform
{
	/// <summary>
	/// Class that facilitates creating weak event handlers that automatically deregister
	/// when the handler is collected.
	/// </summary>
	/// <typeparam name="E">The type that defines the event</typeparam>
	/// <typeparam name="D">The event handler delegate type</typeparam>
	public class WeakEventHandlerProxy<E, D>
		where E : class
		where D : class
	{
		/// <summary>
		/// Handler
		/// </summary>
		public virtual D RealHandler
		{
			get
			{
				return realHandlerRef.Target;
			}
		}
		/// <summary>
		/// <see cref="RealHandler"/>
		/// </summary>
		private readonly NotifyingWeakReference<D> realHandlerRef;
		
		/// <summary>
		/// ProxiedHandler
		/// </summary>
		public virtual D ProxiedHandler
		{
			get
			{
				return proxiedHandler;
			}
		}
		/// <summary>
		/// <see cref="ProxiedHandler"/>
		/// </summary>
		private D proxiedHandler;

		private readonly Action<E, D> deregister;
		private readonly Platform.References.WeakReference<E> eventSource;

		public WeakEventHandlerProxy(E e, D handler, Action<E, D> deregister)
		{
			if (!typeof(D).IsAssignableFrom(typeof(MulticastDelegate)))
			{
				throw new NotSupportedException(typeof(D).Name);
			}

			realHandlerRef = new NotifyingWeakReference<D>(handler);

			this.deregister = deregister;
			eventSource = new Platform.References.WeakReference<E>(e);

			realHandlerRef.ReferenceCollected += delegate
			{
				Deregister();
			};

			CreateProxyMethod();

			GC.KeepAlive(handler);
		}

		private void Deregister()
		{
			E e;
			Action<E, D> routine;

			e = eventSource.Target;

			routine = deregister;

			if (e != null)
			{
				routine(e, this.ProxiedHandler);
			}
		}

		private static IDictionary<Type, DynamicMethod> c_DynamicMethodCache
			= new Dictionary<Type, DynamicMethod>();

		private void CreateProxyMethod()
		{
		    ILGenerator generator;
			Delegate handlerDelegate;
			DynamicMethod dynamicMethod;
			ParameterInfo[] parameters;

			if (!c_DynamicMethodCache.TryGetValue(typeof(D), out dynamicMethod))
			{
				handlerDelegate = (Delegate)((object)realHandlerRef.Target);

				parameters = handlerDelegate.Method.GetParameters();

				dynamicMethod = new DynamicMethod
					(
					"Proxy",
					handlerDelegate.Method.ReturnType,
					EnumerableUtils.Chain
						(
						new[] { this.GetType() },
						parameters.Convert(value => value.ParameterType)
						).ToArray(),
						this.GetType()
					);

				generator = dynamicMethod.GetILGenerator();

				var elselabel = generator.DefineLabel();

				generator.DeclareLocal(typeof(D));

				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Callvirt, this.GetType().GetProperty("RealHandler").GetGetMethod());
				generator.Emit(OpCodes.Castclass, typeof(D));
				generator.Emit(OpCodes.Stloc_0);

				// if (local == null)

				generator.Emit(OpCodes.Ldloc_0);
				generator.Emit(OpCodes.Ldnull);
				generator.Emit(OpCodes.Ceq);
				generator.Emit(OpCodes.Brfalse, elselabel);

				// Call "deregister"

				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Callvirt,
				               this.GetType().GetMethod("Deregister", BindingFlags.Instance | BindingFlags.NonPublic));

				generator.Emit(OpCodes.Ret);

				// else
				generator.MarkLabel(elselabel);

				// load real delegate (the delegate's "this" pointer)

				generator.Emit(OpCodes.Ldloc_0);

				// load arguments

				for (int i = 0; i < parameters.Length; i++)
				{
					if (i == 0)
					{
						generator.Emit(OpCodes.Ldarg_1);
					}
					else if (i == 1)
					{
						generator.Emit(OpCodes.Ldarg_2);
					}
					else if (i == 2)
					{
						generator.Emit(OpCodes.Ldarg_3);
					}
					else if (i < 255)
					{
						generator.Emit(OpCodes.Ldarg_S, (byte)i + 1);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_S, i + 1);
					}
				}

				// Call invoke
				generator.Emit(OpCodes.Callvirt,
				               handlerDelegate.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public));

				// Return the result of the invoke
				generator.Emit(OpCodes.Ret);

				c_DynamicMethodCache[typeof(D)] = dynamicMethod;
			}
            
			proxiedHandler = (D)(object)dynamicMethod.CreateDelegate(typeof(D), this);
		}

		public static implicit operator D(WeakEventHandlerProxy<E, D> proxy)
		{
			return proxy.ProxiedHandler;
		}
	}
}
