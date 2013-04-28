using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Platform.Reflection;

namespace Platform.Utilities
{
	public enum CommandLineError
	{
		UnknownOption,
		InvalidValue,
		TooManyOptions,
		MissingOption
	}

	public class CommandLineOptionsSerializerEventArgs
		: EventArgs
	{
		/// <summary>
		///  OptionName
		/// </summary>
		public virtual string OptionName
		{
			get;
			set;
		}

		/// <summary>
		///  Optionvalue
		/// </summary>
		public virtual string OptionValue
		{
			get;
			set;
		}

		/// <summary>
		///  Error
		/// </summary>
		public virtual CommandLineError Error
		{
			get;
			set;
		}

		public CommandLineOptionsSerializerEventArgs(CommandLineError error, string optionName)
			: this(error, optionName, "")
		{
		}

		public CommandLineOptionsSerializerEventArgs(CommandLineError error, string optionName, string optionvalue)
		{
			this.Error = error;
			this.OptionName = optionName;
			this.OptionValue = optionvalue;
		}
	}

	public delegate void CommandLineOptionsSerializerEventHandler(object sender, CommandLineOptionsSerializerEventArgs eventArgs);

	/// <summary>
	/// Summary description for NameKeyCommandLineParser.
	/// </summary>
	public class CommandLineOptionsSerializer
	{
		public virtual event CommandLineOptionsSerializerEventHandler Error;

		protected virtual void OnError(CommandLineOptionsSerializerEventArgs eventArgs)
		{
			if (Error != null)
			{
				Error(this, eventArgs);
			}
		}

		private IDictionary keyTypeMap;

		public virtual Type SupportedType
		{
			get;
			set;
		}

		private sealed class OptionInfo
		{
			public object Key
			{
				get;
				private set;
			}

			public CommandLineOptionAttribute Attribute
			{
				get;
				private set;
			}

			public CommandLineOptionChoicesAttribute ChoicesAttribute
			{
				get;
				private set;
			}

			public string Name
			{
				get;
				private set;
			}

			public Type Type
			{
				get;
				private set;
			}

			/// <summary>
			///  MemberInfo
			/// </summary>
			private MemberInfo MemberInfo
			{
				get;
				set;
			}

			public OptionInfo(object key, string name, Type type, MemberInfo memberInfo, CommandLineOptionAttribute attribute, CommandLineOptionChoicesAttribute choicesAttribute)
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
				if (MemberInfo is FieldInfo)
				{
					((FieldInfo)MemberInfo).SetValue(target, value);
				}
				else if (MemberInfo is PropertyInfo)
				{
					((PropertyInfo)MemberInfo).SetValue(target, value, null);
				}
			}
		}

		public CommandLineOptionsSerializer(Type supportedType)
		{
			this.SupportedType = supportedType;
			keyTypeMap = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

			Scan();
		}

		protected virtual void Scan()
		{
			AddCommandLineOption(SupportedType.GetFields(BindingFlags.Public | BindingFlags.Instance));
			AddCommandLineOption(SupportedType.GetProperties(BindingFlags.Public | BindingFlags.Instance));
		}

		protected virtual void AddCommandLineOption(MemberInfo[] memberInfos)
		{
			foreach (MemberInfo memberInfo in memberInfos)
			{
				var attributes = (CommandLineOptionAttribute[])memberInfo.GetCustomAttributes(typeof(CommandLineOptionAttribute), true);

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

				CommandLineOptionChoicesAttribute choicesAttribute = null;

				var choicesAttributes = (CommandLineOptionChoicesAttribute[])memberInfo.GetCustomAttributes(typeof(CommandLineOptionChoicesAttribute), true);

				if (choicesAttributes.Length > 0)
				{
					choicesAttribute = choicesAttributes[0];
				}

				keyTypeMap[key] = new OptionInfo(key, name, type, memberInfo, attribute, choicesAttribute);
			}
		}

		public virtual object Parse(string commandLine)
		{
			return Parse(commandLine, null);
		}

		private static readonly Regex regex;

		static CommandLineOptionsSerializer()
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

		public virtual object Parse(string commandLine, CommandLineOptionsSerializerEventHandler errorEventHandler)
		{
			Group group;

			var unkeyedIndex = 0;
			var match = regex.Match(commandLine);
			var retval = Activator.CreateInstance(SupportedType);

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

						SetValue(retval, unkeyedIndex, s, errorEventHandler);

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

				if ((optionInfo = (OptionInfo)keyTypeMap[key]) != null)
				{
					if (optionInfo.Type == typeof(bool))
					{
						if (!match.Groups["value"].Success)
						{
							value = "true";
						}
					}
				}

				SetValue(retval, key, value, errorEventHandler);

				optionsFound[key] = value;

				match = match.NextMatch();
			}

			foreach (OptionInfo optionInfo in keyTypeMap.Values)
			{
				if (optionInfo.Attribute.Required)
				{
					if (!optionsFound.Contains(optionInfo.Key))
					{
						CommandLineOptionsSerializerEventArgs eventArgs;

						eventArgs = new CommandLineOptionsSerializerEventArgs(CommandLineError.MissingOption, optionInfo.Name);

						if (errorEventHandler != null)
						{
							errorEventHandler(this, eventArgs);
						}

						OnError(eventArgs);
					}
				}
			}

			return retval;	
		}

		protected virtual void SetValue(object optionObject, object key, string value, CommandLineOptionsSerializerEventHandler errorEventHandler)
		{
			object convertedValue;
			CommandLineOptionsSerializerEventArgs eventArgs;

			var optionInfo = (OptionInfo)keyTypeMap[key];

			if (optionInfo == null)
			{
				if (key is int)
				{
					eventArgs = new CommandLineOptionsSerializerEventArgs(CommandLineError.TooManyOptions, value);

					if (errorEventHandler != null)
					{
						errorEventHandler(this, eventArgs);
					}

					OnError(eventArgs);

					return;
				}
				else
				{
					eventArgs = new CommandLineOptionsSerializerEventArgs(CommandLineError.UnknownOption, key.ToString(), value);

					if (errorEventHandler != null)
					{
						errorEventHandler(this, eventArgs);
					}

					OnError(eventArgs);

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
					eventArgs = new CommandLineOptionsSerializerEventArgs(CommandLineError.InvalidValue, optionInfo.Name, value);

					if (errorEventHandler != null)
					{
						errorEventHandler(this, eventArgs);
					}

					OnError(eventArgs);
				}
			}

			try
			{
				convertedValue = Convert.ChangeType(value, optionInfo.Type);
			}
			catch (InvalidCastException)
			{
				eventArgs = new CommandLineOptionsSerializerEventArgs(CommandLineError.InvalidValue, optionInfo.Name, value);

				if (errorEventHandler != null)
				{
					errorEventHandler(this, eventArgs);
				}

				OnError(eventArgs);

				return;
			}

			optionInfo.SetValue(optionObject, convertedValue);
		}
	}
}
