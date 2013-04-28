using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Platform
{
	/// <summary>
	/// Provides extension methods and static utility methods for <see cref="Action"/> objects.
	/// </summary>
	public static class ActionUtils<T>
	{
		public static readonly Action<T> Null = delegate { };
	}

	/// <summary>
	/// Provides extension methods and static utility methods for <see cref="Action"/> objects.
	/// </summary>
	public static class ActionUtils
	{
		public static Exception IgnoreExceptions(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				return e;
			}

			return null;
		}

		/// <summary>
		/// Executes an action and returns True if the action executes without throwing an exception.
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <returns>True if the action executed without throwing an exception</returns>
		public static bool IsSuccess<T>(Action<T> action)
		{
			return IsSuccess(action, default(T));
		}

		/// <summary>
		/// Executes an action and returns True if the action executes without throwing an exception.
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="state">The argument to be passed to the action</param>
		/// <returns>True if the action executed without throwing an exception</returns>
		public static bool IsSuccess<T>(Action<T> action, T state)
		{
			try
			{
				action(state);

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Creates a new action an action and retries it a set number of times
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="repeatCount">The number of times to try executing the action</param>
		/// <returns>An action that wraps he given <see cref="action"/></returns>
		public static Action<T> ToRetryAction<T>(Action<T> action, int repeatCount)
		{
			return ToRetryAction(action, repeatCount, TimeSpan.Zero);
		}

		/// <summary>
		/// Creates a new action an action and retries it a set number of times
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="maximumTime">The maximum time to spend trying to execute the action</param>
		/// <param name="retryOnException">
		/// A predicate that will validate if the action should continue retrying after the given has been thrown.
		/// If the predicate does not return True then the exception will be thrown and no further retries will be performed.
		/// </param>/// <returns>An action that wraps he given <see cref="action"/></returns>
		public static Action<T> ToRetryAction<T>(Action<T> action, TimeSpan maximumTime, Predicate<Exception> retryOnException)
		{
			return ToRetryAction(action, maximumTime, null, retryOnException);
		}

		/// <summary>
		/// Creates a new action an action and retries it a set number of times
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="maximumTime">The maximum time to spend trying to execute the action</param>
		/// <param name="pause">The amount of time to pause between retries</param>
		/// <returns>An action that wraps he given <see cref="action"/></returns>
		public static Action<T> ToRetryAction<T>(Action<T> action, TimeSpan maximumTime, TimeSpan pause)
		{
			return ToRetryAction(action, maximumTime, pause, PredicateUtils<Exception>.AlwaysTrue);
		}

		/// <summary>
		/// Creates a new action an action and retries it a set number of times
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="maximumTime">The maximum time to spend trying to execute the action</param>
		/// <param name="pause">The amount of time to pause between retries</param>
		/// <param name="retryOnException">A predicate that will validate if the action should continue retrying after the given has been thrown</param>
		/// <returns>An action that wraps he given <see cref="action"/></returns>
		public static Action<T> ToRetryAction<T>(Action<T> action, TimeSpan maximumTime, TimeSpan pause, Predicate<Exception> retryOnException)
		{
			return ToRetryAction(action, maximumTime, (TimeSpan?)pause, retryOnException);
		}

		/// <summary>
		/// Creates a new action an action and retries it a set number of times
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="maximumTime">The maximum time to spend trying to execute the action</param>
		/// <param name="pause">The amount of time to pause between retries</param>
		/// <param name="retryOnException">
		/// A predicate that will validate if the action should continue retrying after the given has been thrown.
		/// If the predicate does not return True then the exception will be thrown and no further retries will be performed.
		/// </param>
		/// <returns>An action that wraps he given <see cref="action"/></returns>
		public static Action<T> ToRetryAction<T>(Action<T> action, TimeSpan maximumTime, TimeSpan? pause, Predicate<Exception> retryOnException)
		{
			DateTime startTime;

			return delegate(T state)
			{
				startTime = DateTime.Now;

				while (true)
				{
					try
					{
						action(state);

						return;
					}
					catch (Exception e)
					{
						if (!retryOnException(e))
						{
							throw;
						}

						if (DateTime.Now - startTime > maximumTime)
						{
							throw;
						}
					}

					Thread.Sleep(pause ?? TimeSpan.FromSeconds(maximumTime.TotalSeconds / 10));					
				}
			};
		}

		/// <summary>
		/// Creates a new action an action and retries it a set number of times
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="repeatCount">The number of times to try executing the action</param>
		/// <param name="pause">The amount of time to pause between retries</param>
		/// <returns>An action that wraps he given <see cref="action"/></returns>
		public static Action<T> ToRetryAction<T>(Action<T> action, int repeatCount, TimeSpan pause)
		{
			return ToRetryAction(action, repeatCount, (TimeSpan?)pause);
		}

		/// <summary>
		/// Creates a new action an action and retries it a set number of times
		/// </summary>
		/// <typeparam name="T">The argument type for the action</typeparam>
		/// <param name="action">The action to execute</param>
		/// <param name="repeatCount">The number of times to try executing the action</param>
		/// <param name="pause">The amount of time to pause between retries</param>
		/// <returns>An action that wraps he given <see cref="action"/></returns>
		public static Action<T> ToRetryAction<T>(Action<T> action, int repeatCount, TimeSpan? pause)
		{
			return delegate(T state)
			{
				for (int i = 0; i < repeatCount; i++)
				{
					try
					{
						action(state);

						return;
					}
					catch (Exception)
					{
						if (i == repeatCount - 1)
						{
							throw;
						}
					}

					Thread.Sleep(pause ?? TimeSpan.FromSeconds(0));
				}
			};
		}
	}
}
