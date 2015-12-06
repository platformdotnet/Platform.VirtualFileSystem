using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Platform.Reflection;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class CommandOptionsSerializer
	{
		private readonly IDictionary keyTypeMap;

		public event EventHandler<CommandOptionsSerializerEventArgs> Error;

		protected virtual void OnError(CommandOptionsSerializerEventArgs eventArgs)
		{
			this.Error?.Invoke(this, eventArgs);
		}

		public Type SupportedType { get; set; }

		private sealed class OptionInfo
		{
			public object Key { get; }
			public CommandOptionAttribute Attribute { get; }
			public CommandOptionChoicesAttribute ChoicesAttribute { get; }
			public string Name { get; }
			public Type Type { get; }
			private MemberInfo MemberInfo { get; }

			public OptionInfo(object key, string name, Type type, MemberInfo memberInfo, CommandOptionAttribute attribute, CommandOptionChoicesAttribute choicesAttribute)
			{
				this.Key = key;
				this.Name = name;
				this.Type = type;
				this.MemberInfo = memberInfo;
				this.Attribute = attribute;
				this.ChoicesAttribute = choicesAttribute;
			}

			public void SetValue(object target, object value)
			{
				if (this.MemberInfo is FieldInfo)
				{
					((FieldInfo)this.MemberInfo).SetValue(target, value);
				}
				else
				{
					(this.MemberInfo as PropertyInfo)?.SetValue(target, value, null);
				}
			}
		}

		public CommandOptionsSerializer(Type supportedType)
		{
			this.SupportedType = supportedType;
			this.keyTypeMap = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

			this.Scan();
		}

		protected virtual void Scan()
		{
			this.AddCommandLineOption(this.SupportedType.GetFields(BindingFlags.Public | BindingFlags.Instance));
			this.AddCommandLineOption(this.SupportedType.GetProperties(BindingFlags.Public | BindingFlags.Instance));
		}

		protected virtual void AddCommandLineOption(MemberInfo[] memberInfos)
		{
			foreach (var memberInfo in memberInfos)
			{
				var attributes = (CommandOptionAttribute[])memberInfo.GetCustomAttributes(typeof(CommandOptionAttribute), true);

				if (attributes.Length <= 0)
				{
					continue;
				}

				var attribute = attributes[0];

				var name = attribute.Name;

				if (name == "")
				{
					name = memberInfo.Name;
				}

				var type = attribute.Type;

				if (type == null)
				{
					type = memberInfo.GetMemberReturnType();
				}

				object key = name;

				if (attribute.UnnamedOptionIndex >= 0)
				{
					key = attribute.UnnamedOptionIndex;
				}

				CommandOptionChoicesAttribute choicesAttribute = null;

				var choicesAttributes = (CommandOptionChoicesAttribute[])memberInfo.GetCustomAttributes(typeof(CommandOptionChoicesAttribute), true);

				if (choicesAttributes.Length > 0)
				{
					choicesAttribute = choicesAttributes[0];
				}

				this.keyTypeMap[key] = new OptionInfo(key, name, type, memberInfo, attribute, choicesAttribute);
			}
		}

		public virtual object Parse(string commandLine)
		{
			return this.Parse(commandLine, null);
		}

		private static readonly Regex regex;

		static CommandOptionsSerializer()
		{
			regex = new Regex(@"
					([ ]*
						(?<keyvalue>(
							((\-\-? \"" (?<key>(((\\\"")|(\\\\)|[^""\=])+)) \"") | \-\-? (?<key>(((\\\"")|(\\\\)|[^\= ])+)))
							([\=]
							((\"" (?<value>((\\\""\\\\|[^""])*)) \"") | (?<value>((\\\""\\\\|[^ ])*)))
							)?
						))
					)
					|[ ]*(\""(?<other>[^\""]*)\"")
					|[ ]*(?<other>[^ ]*)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
		}

		public virtual object Parse(string commandLine, EventHandler<CommandOptionsSerializerEventArgs> errorEventHandler)
		{
			Group group;

			var unkeyedIndex = 0;
			var match = regex.Match(commandLine);
			var retval = Activator.CreateInstance(this.SupportedType);

			var optionsFound = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

			while (match.Success)
			{
				group = match.Groups["other"];

				var s = group.Value;

				if (group.Captures.Count > 0)
				{
					if (group.Length > 0)
					{
						optionsFound[unkeyedIndex] = "";

						this.SetValue(retval, unkeyedIndex, s, errorEventHandler);

						match = match.NextMatch();

						unkeyedIndex++;

						continue;
					}
					else
					{
						match = match.NextMatch();

						continue;
					}
				}

				var key = match.Groups["key"].Value;
				var value = match.Groups["value"].Value;

				OptionInfo optionInfo;

				if ((optionInfo = (OptionInfo)this.keyTypeMap[key]) != null)
				{
					if (optionInfo.Type == typeof(bool))
					{
						if (!match.Groups["value"].Success)
						{
							value = "true";
						}
					}
				}

				this.SetValue(retval, key, value, errorEventHandler);

				optionsFound[key] = value;

				match = match.NextMatch();
			}

			foreach (OptionInfo optionInfo in this.keyTypeMap.Values)
			{
				if (optionInfo.Attribute.Required)
				{
					if (!optionsFound.Contains(optionInfo.Key))
					{
						CommandOptionsSerializerEventArgs eventArgs;

						eventArgs = new CommandOptionsSerializerEventArgs(CommandLineError.MissingOption, optionInfo.Name);

						if (errorEventHandler != null)
						{
							errorEventHandler(this, eventArgs);
						}

						this.OnError(eventArgs);
					}
				}
			}

			return retval;
		}

		protected virtual void SetValue(object optionObject, object key, string value, EventHandler<CommandOptionsSerializerEventArgs> errorEventHandler)
		{
			object convertedValue;
			CommandOptionsSerializerEventArgs eventArgs;

			var optionInfo = (OptionInfo)this.keyTypeMap[key];

			if (optionInfo == null)
			{
				if (key is int)
				{
					eventArgs = new CommandOptionsSerializerEventArgs(CommandLineError.TooManyOptions, value);

					if (errorEventHandler != null)
					{
						errorEventHandler(this, eventArgs);
					}

					this.OnError(eventArgs);

					return;
				}
				else
				{
					eventArgs = new CommandOptionsSerializerEventArgs(CommandLineError.UnknownOption, key.ToString(), value);

					if (errorEventHandler != null)
					{
						errorEventHandler(this, eventArgs);
					}

					this.OnError(eventArgs);

					return;
				}
			}

			if (optionInfo.ChoicesAttribute != null)
			{
				var error = true;

				foreach (string s in optionInfo.ChoicesAttribute.Choices)
				{
					if (value.EqualsIgnoreCase(s))
					{
						error = false;
						break;
					}
				}

				if (error)
				{
					eventArgs = new CommandOptionsSerializerEventArgs(CommandLineError.InvalidValue, optionInfo.Name, value);

					if (errorEventHandler != null)
					{
						errorEventHandler(this, eventArgs);
					}

					this.OnError(eventArgs);
				}
			}

			try
			{
				convertedValue = Convert.ChangeType(value, optionInfo.Type);
			}
			catch (InvalidCastException)
			{
				eventArgs = new CommandOptionsSerializerEventArgs(CommandLineError.InvalidValue, optionInfo.Name, value);

				if (errorEventHandler != null)
				{
					errorEventHandler(this, eventArgs);
				}

				this.OnError(eventArgs);

				return;
			}

			optionInfo.SetValue(optionObject, convertedValue);
		}
	}
}

