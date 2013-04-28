#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Platform
{
	/// <summary>
	/// Defines operations that can be performed on functions such as currying.
	/// </summary>
	public static class FunctionUtils
	{
		/// <summary>
		/// Takes a function (<paramref name="ff"/>) and returns a function that converts the return value of the 
		/// function using the function (<paramref name="ff"/>).
		/// <c>convert</c> function.
		/// </summary>
		/// <typeparam name="R">
		/// The return type of the function.
		/// </typeparam>
		/// <typeparam name="RT">
		/// The new return type of the function.
		/// </typeparam>
		/// <param name="ff">
		/// The functon whose return value needs to be returned.
		/// </param>
		/// <param name="convert">
		/// The function that will convert the return type from <typeparamref name="T"/> to <typeparamref name="RT"/>.
		/// </param>
		/// <returns>
		/// A function that converts the return type of the function <paramref name="ff"/>.
		/// </returns>
		public static Func<RT> ConvertResult<R, RT>(this Func<R> ff, Converter<R, RT> convert)
		{
			return () => convert(ff());
		}

		/// <summary>
		/// Takes a function (<paramref name="ff"/>) and returns a function that converts the return value of the 
		/// function using the function (<paramref name="ff"/>).
		/// <c>convert</c> function.
		/// </summary>
		/// <typeparam name="A">
		/// The type of the first argument of the function.
		/// </typeparam>
		/// <typeparam name="R">
		/// The return type of the function.
		/// </typeparam>
		/// <typeparam name="RT">
		/// The new return type of the function.
		/// </typeparam>
		/// <param name="ff">
		/// The functon whose return value needs to be returned.
		/// </param>
		/// <param name="convert">
		/// The function that will convert the return type from <typeparamref name="R"/> to <typeparamref name="RT"/>.
		/// </param>
		/// <returns>
		/// A function that converts the return type of the function <paramref name="ff"/>.
		/// </returns>
		public static Func<A, RT> ConvertResult<A, R, RT>(this Func<A, R> ff, Converter<R, RT> convert)
		{
			return a => convert(ff(a));
		}

		/// <summary>
		/// Returns a composite function made up of the given functions
		/// </summary>
		/// <returns>
		/// The function which the equivalent of the composite b(a())
		/// </returns>
		public static Func<A, R> Compose<A, B, R>(Func<B, R> b, Func<A, B> a)
		{
			return value => b(a(value));
		}

		/// <summary>
		/// Returns a composite function made up of the given functions
		/// </summary>
		/// <returns>
		/// The function which the equivalent of the composite c(b(a()))
		/// </returns>
		public static Func<A, R> Compose<A, B, C, R>(Func<C, R> c, Func<B, C> b, Func<A, B> a)
		{
			return value => c(b(a(value)));
		}

		/// <summary>
		/// Returns a composite function made up of the given functions
		/// </summary>
		/// <returns>
		/// The function which the equivalent of the composite d(c(b(a())))
		/// </returns>
		public static Func<A, R> Compose<A, B, C, D, R>(Func<D, R> d, Func<C, D> c, Func<B, C> b, Func<A, B> a)
		{
			return value => d(c(b(a(value))));
		}

		/// <summary>
		/// Returns a composite function made up of the given functions
		/// </summary>
		/// <returns>
		/// The function which the equivalent of the composite e(d(c(b(a()))))
		/// </returns>
		public static Func<A, R> Compose<A, B, C, D, E, R>(Func<E, R> e, Func<D, E> d, Func<C, D> c, Func<B, C> b, Func<A, B> a)
		{
			return value => e(d(c(b(a(value)))));
		}

		/// <summary>
		/// Returns a composite function made up of the given functions
		/// </summary>
		/// <returns>
		/// The function which the equivalent of the composite f(e(d(c(b(a())))))
		/// </returns>
		public static Func<A, R> Compose<A, B, C, D, E, F, R>(Func<F, R> f, Func<E, F> e, Func<D, E> d, Func<C, D> c, Func<B, C> b, Func<A, B> a)
		{
			return value => f(e(d(c(b(a(value))))));
		}

		/// <summary>
		/// Returns a composite function made up of the given functions
		/// </summary>
		/// <returns>
		/// The function which the equivalent of the composite g(f(e(d(c(b(a()))))))
		/// </returns>
		public static Func<A, R> Compose<A, B, C, D, E, F, G, R>(Func<G, R> g, Func<F, G> f, Func<E, F> e, Func<D, E> d, Func<C, D> c, Func<B, C> b, Func<A, B> a)
		{
			return value => g(f(e(d(c(b(a(value)))))));
		}

		/// <summary>
		/// Returns a composite function made up of the given functions
		/// </summary>
		/// <returns>
		/// The function which the equivalent of the composite h(g(e(d(c(b(a())))))))
		/// </returns>
		public static Func<A, R> Compose<A, B, C, D, E, F, G, H, R>(Func<H, R> h, Func<G, H> g, Func<F, G> f, Func<E, F> e, Func<D, E> d, Func<C, D> c, Func<B, C> b, Func<A, B> a)
		{
			return value => h(g(f(e(d(c(b(a(value))))))));
		}

		/* 2 */

		/// <summary>
		/// Curries a two parameter function.
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B -> R) -> (A -> (B -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<A, Func<B, R>> Curry<A, B, R>(Func<A, B, R> ff)
		{
			return a => b => ff(a, b);
		}

		/// <summary>
		/// UnCurries a two parameter function.
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B -> R) -> (A -> (B -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The uncurried function
		/// </returns>
		public static Func<A, B, R> UnCurry<A, B, R>(Func<A, Func<B, R>> ff)
		{
			return (a, b) => ff(a)(b);
		}

		/// <summary>
		/// Curries a two parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B -> R) -> A) -> (B -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<B, R> Curry<A, B, R>(Func<A, B, R> ff, A a)
		{
			return b => ff(a, b);
		}

		/* 3 */

		/// <summary>
		/// Curries a three parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C -> R) -> (A -> (B -> C -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<A, Func<B, C, R>> Curry<A, B, C, R>(Func<A, B, C, R> ff)
		{
			return a => (b, c) => ff(a, b, c);
		}

		/// <summary>
		/// UnCurries a three parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C -> R) -> (A -> (B -> C -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The uncurried function
		/// </returns>
		public static Func<A, B, C, R> Curry<A, B, C, R>(Func<A, Func<B, C, R>> ff)
		{
			return delegate(A a, B b, C c)
			{
				return ff(a)(b, c);
			};
		}

		/// <summary>
		/// Curries a three parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C -> R) -> A) -> (B -> C -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<B, C, R> Curry<A, B, C, R>(Func<A, B, C, R> ff, A a)
		{
			return (b, c) => ff(a, b, c);
		}

		/// <summary>
		/// Curries a three parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C -> R) -> A -> B) -> (C -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<C, R> Curry<A, B, C, R>(Func<A, B, C, R> ff, A a, B b)
		{
			return c => ff(a, b, c);
		}

		/* 4 */

		/// <summary>
		/// Curries a four parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D -> R)) -> (A -> (B -> C -> D - > R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<A, Func<B, C, D, R>> Curry<A, B, C, D, R>(Func<A, B, C, D, R> ff)
		{
			return a => ((b, c, d) => ff(a, b, c, d));
		}

		/// <summary>
		/// UnCurries a four parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D -> R)) -> (A -> (B -> C -> D - > R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The uncurried function
		/// </returns>
		public static Func<A, B, C, D, R> Curry<A, B, C, D, R>(Func<A, Func<B, C, D, R>> ff)
		{
			return (a, b, c, d) => ff(a)(b, c, d);
		}

		/// <summary>
		/// Curries a four parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D -> R) -> A) -> (B -> C -> D - > R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<B, C, D, R> Curry<A, B, C, D, R>(Func<A, B, C, D, R> ff, A a)
		{
			return delegate(B b, C c, D d)
			{
				return ff(a, b, c, d);
			};
		}

		/// <summary>
		/// Curries a four parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D -> R) -> A -> B) -> (C -> D - > R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<C, D, R> Curry<A, B, C, D, R>(Func<A, B, C, D, R> ff, A a, B b)
		{
			return (c, d) => ff(a, b, c, d);
		}

		/// <summary>
		/// Curries a four parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D -> R) -> A -> B -> C) -> (D - > R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<D, R> Curry<A, B, C, D, R>(Func<A, B, C, D, R> ff, A a, B b, C c)
		{
			return d => ff(a, b, c, d);
		}

		/* 5 */

		/// <summary>
		/// Curries a five parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> R) -> (A -> (B -> C -> D -> E -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<A, Func<B, C, D, E, R>> Curry<A, B, C, D, E, R>(Func<A, B, C, D, E, R> ff)
		{
			return a => (b, c, d, e) => ff(a, b, c, d, e);
		}

		/// <summary>
		/// UnCurries a five parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> R) -> (A -> (B -> C -> D -> E -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The uncurried function
		/// </returns>
		public static Func<A, B, C, D, E, R> Curry<A, B, C, D, E, R>(Func<A, Func<B, C, D, E, R>> ff)
		{
			return delegate(A a, B b, C c, D d, E e)
			{
				return ff(a)(b, c, d, e);
			};
		}

		/// <summary>
		/// Curries a five parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// (((A * B * C * D * E) -> A -> R) -> (B -> C -> D -> E -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<B, C, D, E, R> Curry<A, B, C, D, E, R>(Func<A, B, C, D, E, R> ff, A a)
		{
			return (b, c, d, e) => ff(a, b, c, d, e);
		}

		/// <summary>
		/// Curries a five parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E) -> A -> B -> R) -> (C -> D -> E -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<C, D, E, R> Curry<A, B, C, D, E, R>(Func<A, B, C, D, E, R> ff, A a, B b)
		{
			return (c, d, e) => ff(a, b, c, d, e);
		}


		public static Func<D, E, R> Curry<A, B, C, D, E, R>(Func<A, B, C, D, E, R> ff, A a, B b, C c)
		{
			return (d, e) => ff(a, b, c, d, e);
		}

		/// <summary>
		/// Curries a five parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E) -> A -> B -> C -> D -> R) -> (E -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<E, R> Curry<A, B, C, D, E, R>(Func<A, B, C, D, E, R> ff, A a, B b, C c, D d)
		{
			return e => ff(a, b, c, d, e);
		}

		/* 6 */

		/// <summary>
		/// Curries a six parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> F -> R) -> (A -> (B -> C -> D -> E -> F -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<A, Func<B, C, D, E, F, R>> Curry<A, B, C, D, E, F, R>(Func<A, B, C, D, E, F, R> ff)
		{
			return a => (b, c, d, e, f) => ff(a, b, c, d, e, f);
		}

		/// <summary>
		/// UnCurries a six parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> F -> R) -> (A -> (B -> C -> D -> E -> F -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The uncurried function
		/// </returns>
		public static Func<A, B, C, D, E, F, R> Curry<A, B, C, D, E, F, R>(Func<A, Func<B, C, D, E, F, R>> ff)
		{
			return (a, b, c, d, e, f) => ff(a)(b, c, d, e, f);
		}

		/// <summary>
		/// Curries a six parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E * F) -> A -> R) -> (B -> C -> D -> E -> F -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<B, C, D, E, F, R> Curry<A, B, C, D, E, F, R>(Func<A, B, C, D, E, F, R> ff, A a)
		{
			return (b, c, d, e, f) => ff(a, b, c, d, e, f);
		}

		/// <summary>
		/// Curries a six parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E * F) -> A -> B -> R) -> (C -> D -> E -> F -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<C, D, E, F, R> Curry<A, B, C, D, E, F, R>(Func<A, B, C, D, E, F, R> ff, A a, B b)
		{
			return (c, d, e, f) => ff(a, b, c, d, e, f);
		}

		/// <summary>
		/// Curries a six parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E * F) -> A -> B -> C -> R) -> (D -> E -> F -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<D, E, F, R> Curry<A, B, C, D, E, F, R>(Func<A, B, C, D, E, F, R> ff, A a, B b, C c)
		{
			return (d, e, f) => ff(a, b, c, d, e, f);
		}

		/// <summary>
		/// Curries a six parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E * F) -> A -> B -> C -> D -> R) -> (E -> F -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<E, F, R> Curry<A, B, C, D, E, F, R>(Func<A, B, C, D, E, F, R> ff, A a, B b, C c, D d)
		{
			return (e, f) => ff(a, b, c, d, e, f);
		}

		/// <summary>
		/// Curries a six parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E * F) -> A -> B -> C -> D -> E -> R) -> (F -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<F, R> Curry<A, B, C, D, E, F, R>(Func<A, B, C, D, E, F, R> ff, A a, B b, C c, D d, E e)
		{
			return delegate(F f)
			{
				return ff(a, b, c, d, e, f);
			};
		}

		/* 7 */

		/// <summary>
		/// Curries a seven parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> F -> G -> R) -> (A -> (B -> C -> D -> E -> F -> G -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<A, Func<B, C, D, E, F, G, R>> Curry<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> ff)
		{
			return a => ((b, c, d, e, f, g) => ff(a, b, c, d, e, f, g));
		}

		/// <summary>
		/// UnCurries a seven parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> F -> G -> R) -> (A -> (B -> C -> D -> E -> F -> G -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The uncurried function
		/// </returns>
		public static Func<A, B, C, D, E, F, G, R> Curry<A, B, C, D, E, F, G, R>(Func<A, Func<B, C, D, E, F, G, R>> ff)
		{
			return (a, b, c, d, e, f, g) => ff(a)(b, c, d, e, f, g);
		}

		/// <summary>
		/// Curries a seven parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G) -> A -> R) -> (B -> C -> D -> E -> F -> G -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<B, C, D, E, F, G, R> Curry<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> ff, A a)
		{
			return (b, c, d, e, f, g) => ff(a, b, c, d, e, f, g);
		}

		/// <summary>
		/// Curries a seven parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G) -> A -> B -> R) -> (C -> D -> E -> F -> G -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<C, D, E, F, G, R> Curry<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> ff, A a, B b)
		{
			return (c, d, e, f, g) => ff(a, b, c, d, e, f, g);
		}

		/// <summary>
		/// Curries a seven parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G) -> A -> B -> C -> R) -> (D -> E -> F -> G -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<D, E, F, G, R> Curry<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> ff, A a, B b, C c)
		{
			return (d, e, f, g) => ff(a, b, c, d, e, f, g);
		}

		/// <summary>
		/// Curries a seven parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G) -> A -> B -> C -> D -> R) -> (E -> F -> G -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<E, F, G, R> Curry<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> ff, A a, B b, C c, D d)
		{
			return (e, f, g) => ff(a, b, c, d, e, f, g);
		}

		/// <summary>
		/// Curries a seven parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G) -> A -> B -> C -> D -> E -> R) -> (F -> G -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<F, G, R> Curry<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> ff, A a, B b, C c, D d, E e)
		{
			return (f, g) => ff(a, b, c, d, e, f, g);
		}

		/// <summary>
		/// Curries a seven parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G) -> A -> B -> C -> D -> E -> F -> R) -> (G -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<G, R> Curry<A, B, C, D, E, F, G, R>(Func<A, B, C, D, E, F, G, R> ff, A a, B b, C c, D d, E e, F f)
		{
			return g => ff(a, b, c, d, e, f, g);
		}

		/* 8 */

		/// <summary>
		/// Curries an eight parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> F -> G -> H -> R) -> (A -> (B -> C -> D -> E -> F -> G -> H -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<A, Func<B, C, D, E, F, G, H, R>> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff)
		{
			return a => ((b, c, d, e, f, g, h) => ff(a, b, c, d, e, f, g, h));
		}

		/// <summary>
		/// UnCurries an eight parameter function
		/// </summary>
		/// <remarks>
		/// <code>
		/// (A * B * C * D * E -> F -> G -> H -> R) -> (A -> (B -> C -> D -> E -> F -> G -> H -> R))
		/// </code>
		/// </remarks>
		/// <returns>
		/// The uncurried function
		/// </returns>
		public static Func<A, B, C, D, E, F, G, H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, Func<B, C, D, E, F, G, H, R>> ff)
		{
			return (a, b, c, d, e, f, g, h) => ff(a)(b, c, d, e, f, g, h);
		}

		/// <summary>
		/// Curries an eight parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G -> H) -> A -> R) -> (B -> C -> D -> E -> F -> G -> H -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<B, C, D, E, F, G, H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff, A a)
		{
			return (b, c, d, e, f, g, h) => ff(a, b, c, d, e, f, g, h);
		}

		/// <summary>
		/// Curries an eight parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G -> H) -> A -> B -> R) -> (C -> D -> E -> F -> G -> H -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<C, D, E, F, G, H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff, A a, B b)
		{
			return (c, d, e, f, g, h) => ff(a, b, c, d, e, f, g, h);
		}

		/// <summary>
		/// Curries an eight parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G -> H) -> A -> B -> C -> R) -> (D -> E -> F -> G -> H -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<D, E, F, G, H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff, A a, B b, C c)
		{
			return (d, e, f, g, h) => ff(a, b, c, d, e, f, g, h);
		}

		/// <summary>
		/// Curries an eight parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G -> H) -> A -> B -> C -> D -> R) -> (E -> F -> G -> H -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<E, F, G, H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff, A a, B b, C c, D d)
		{
			return (e, f, g, h) => ff(a, b, c, d, e, f, g, h);
		}

		/// <summary>
		/// Curries an eight parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G -> H) -> A -> B -> C -> D -> E -> R) -> (F -> G -> H -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<F, G, H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff, A a, B b, C c, D d, E e)
		{
			return (f, g, h) => ff(a, b, c, d, e, f, g, h);
		}

		/// <summary>
		/// Curries an eight parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G -> H) -> A -> B -> C -> D -> E -> F -> R) -> (G -> H -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<G, H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff, A a, B b, C c, D d, E e, F f)
		{
			return (g, h) => ff(a, b, c, d, e, f, g, h);
		}

		/// <summary>
		/// Curries an eight parameter function and evaluates the curried function with the supplied argument(s)
		/// </summary>
		/// <remarks>
		/// <code>
		/// ((A * B * C * D * E -> F -> G -> H) -> A -> B -> C -> D -> E -> F -> G -> R) -> (H -> R)
		/// </code>
		/// </remarks>
		/// <returns>
		/// The curried function
		/// </returns>
		public static Func<H, R> Curry<A, B, C, D, E, F, G, H, R>(Func<A, B, C, D, E, F, G, H, R> ff, A a, B b, C c, D d, E e, F f, G g)
		{
			return h => ff(a, b, c, d, e, f, g, h);
		}

        /// <summary>
        /// Returns the result of a function evaluation, ensuring the value returned value
        /// is not optimised out-of-order by the JIT.  Useful for implementing thread-safe
        /// singleton initialisation.
        /// </summary>
        /// <typeparam name="T">The variable type</typeparam>
        /// <param name="function">The function that returns the value</param>
        /// <returns>The return value of the Func</returns>
		public static T VolatileAssign<T>(this Func<T> function)
		{
			T value;

			value = function();

			System.Threading.Thread.MemoryBarrier();

			return value;
		}
	}
}
