using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	/// <summary>
	/// Summary description for StandardCommandProcessorProvider.
	/// </summary>
	public class TextCommandProcessorProvider
		: CommandProcessorProvider
	{
		private readonly IDictionary commandProcessors;

		public virtual IEnumerable<string> GetCommandNames()
		{
			foreach (DictionaryEntry entry in commandProcessors)
			{
				var list = (IList)entry.Value;

				foreach (TextCommandProcessor processor in list)
				{
					if (processor.SupportsRunLevel(Connection.RunLevel))
					{
						yield return (string)entry.Key;
					}
				}
			}
		}

		public TextCommandProcessorProvider(Connection connection)
			: base(connection)
		{
			commandProcessors = CollectionsUtil.CreateCaseInsensitiveHashtable();
		}

		public TextCommandProcessorProvider(Connection connection, Type[] types)
			: this(connection)
		{
			this.AddCommandProcessors(types);
		}

		public TextCommandProcessorProvider(Connection connection, Assembly assembly)
			: this(connection)
		{
			this.AddCommandProcessors(assembly);
		}

		public virtual void AddCommandProcessors(Assembly assembly)
		{
			this.AddCommandProcessors(assembly.GetTypes());
		}

		public virtual void AddCommandProcessors(Type[] types)
		{
			foreach (Type type in types)
			{
				if (typeof(TextCommandProcessor).IsAssignableFrom(type))
				{
					if (!type.IsAbstract)
					{
						var commandProcessor = (TextCommandProcessor)Activator.CreateInstance(type, new object[] { Connection });

						AddCommandProcessor(commandProcessor);
					}
				}
			}
		}

		public virtual void AddCommandProcessor(TextCommandProcessor commandProcessor)
		{
			IList list;

			foreach (string s in commandProcessor.GetNames())
			{
				list = (IList)commandProcessors[s];

				if (list == null)
				{
					list = new ArrayList(4);

					commandProcessors[s] = list;
				}

				list.Add(commandProcessor);
			}
		}

		public override CommandProcessor GetCommandProcessor(Command command)
		{
			var list = (IList)commandProcessors[command.Name];

			if (list != null)
			{
				foreach (TextCommandProcessor processor in list)
				{
					if (processor.SupportsCommand(command))
					{
						return processor;
					}
				}
			}

			throw new CommandNotSupportedException(command);
		}
	}
}
