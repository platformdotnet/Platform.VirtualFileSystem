using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Platform.Xml.Serialization
{
	public interface IVariableSubstitutor
	{
		string Substitute(string value);
	}

	public class XmlEnvironmentVariableSubstitutor
		: IVariableSubstitutor
	{
		private static Regex c_Regex;

		static XmlEnvironmentVariableSubstitutor()
		{
			c_Regex = new Regex
			(
				@"
					\$\((?<name>([a-zA-Z]+[a-zA-Z_]*))\)
				",
				RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace
			);
		}

		protected static string EnvironmentVariableMatchEvaluator(Match value)
		{
			string name;

			name = value.Groups["name"].Value;

			return Environment.GetEnvironmentVariable(name);
		}

		public virtual string Substitute(string value)
		{
			return c_Regex.Replace(value, EnvironmentVariableMatchEvaluator);
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlVariableSubstitutionAttribute
		: XmlSerializationAttribute
	{
		public virtual Type SubstitutorType
		{
			get;
			set;
		}

		public XmlVariableSubstitutionAttribute()
			: this(typeof(XmlEnvironmentVariableSubstitutor))
		{
		}

		public XmlVariableSubstitutionAttribute(Type substitutorType)
		{
			this.SubstitutorType = substitutorType;
		}	
	}
}
