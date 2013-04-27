using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Platform.Text;
using Platform.VirtualFileSystem.Providers.Imaginary;

namespace Platform.VirtualFileSystem.Providers.SystemInfo
{
	public class SystemInfoNodeProvider
		: ImaginaryNodeProvider
	{
		public SystemInfoNodeProvider(IFileSystemManager manager)
			: this(manager, new ConstructionOptions("systeminfo"))
		{
		}

		public SystemInfoNodeProvider(IFileSystemManager manager, string scheme)
			: base(manager, scheme)
		{			
		}

		public SystemInfoNodeProvider(IFileSystemManager manager, ConstructionOptions options)
			: this(manager, options.Scheme.IsNullOrEmpty() ? "systeminfo" : options.Scheme)
		{
			var root = (ImaginaryDirectory)this.ImaginaryFileSystem.RootDirectory;
						
			var systemClockDirectory = (ImaginaryDirectory)root.ResolveDirectory("SystemClock").Create();
			ImaginaryDirectory environmentVariablesDirectory = new EnvironmentVariablesDirectory(this.ImaginaryFileSystem, root.Address.ResolveAddress("EnvironmentVariables"));

			root.Add(systemClockDirectory);
			root.Add(environmentVariablesDirectory);
			
			var dateTimeFile = new ImaginaryMemoryFile
			(
				this.ImaginaryFileSystem,
				systemClockDirectory.Address.ResolveAddress("./CurrentDateTime"),
				delegate
				{
					return Encoding.ASCII.GetBytes(DateTime.Now.ToUniversalTime().ToString(DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString) + Environment.NewLine);
				},
				PredicateUtils<ImaginaryMemoryFile>.AlwaysTrue
			);

			dateTimeFile.Changed += delegate
			{
				string s;
				TimeSpan timeSpan;
				DateTime dateTime;

				s = Encoding.ASCII.GetString(dateTimeFile.RawNonDynamicValue);

				s = s.Trim(c => Char.IsWhiteSpace(c) || c == '\r' || c == '\n' || c == '\t');

				if (TimeSpan.TryParse(s, out timeSpan))
				{
					dateTime = DateTime.Now;

					dateTime += timeSpan;
				}
				else
				{
					try
					{
						dateTime = DateTime.ParseExact(s, DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString, CultureInfo.InvariantCulture);

						dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
						dateTime = dateTime.ToLocalTime();
					}
					catch (FormatException e)
					{
						Console.Error.WriteLine(e);

						return;
					}
				}

				EnvironmentUtils.SetSystemTime(dateTime);
			};

			systemClockDirectory.Add(dateTimeFile);

			var utcDateTimeFile = new ImaginaryMemoryFile
			(
				this.ImaginaryFileSystem,
				systemClockDirectory.Address.ResolveAddress("./CurrentUtcDateTime"),
				delegate
				{
					return Encoding.ASCII.GetBytes(DateTime.Now.ToUniversalTime().ToString(DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString) + "\n");
				},
				PredicateUtils<ImaginaryMemoryFile>.AlwaysTrue
			);

			utcDateTimeFile.Changed += delegate
			{
				TimeSpan timeSpan;
				DateTime dateTime;

				var s = Encoding.ASCII.GetString(utcDateTimeFile.RawNonDynamicValue);

				s = s.Trim(c => Char.IsWhiteSpace(c) || c == '\r' || c == '\n' || c == '\t');

				if (TimeSpan.TryParse(s, out timeSpan))
				{
					dateTime = DateTime.Now;

					dateTime += timeSpan;
				}
				else
				{
					try
					{
						dateTime = DateTime.ParseExact(s, DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString, CultureInfo.InvariantCulture);

						dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
					}
					catch (FormatException e)
					{
						Console.Error.WriteLine(e);

						return;
					}
				}

				EnvironmentUtils.SetSystemTime(dateTime);
			};

			systemClockDirectory.Add(utcDateTimeFile);
			
			var localDateTimeFile = new ImaginaryMemoryFile
			(
				this.ImaginaryFileSystem,
				systemClockDirectory.Address.ResolveAddress("./CurrentLocalDateTime"),
				delegate
				{
					return Encoding.ASCII.GetBytes(DateTime.Now.ToString(DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString) + Environment.NewLine);
				},
				PredicateUtils<ImaginaryMemoryFile>.AlwaysTrue
			);

			localDateTimeFile.Changed += delegate
			{
				TimeSpan timeSpan;
				DateTime dateTime;

				var s = Encoding.ASCII.GetString(localDateTimeFile.RawNonDynamicValue);

				s = s.Trim(c => Char.IsWhiteSpace(c) || c == '\r' || c == '\n' || c == '\t');

				if (TimeSpan.TryParse(s, out timeSpan))
				{
					dateTime = DateTime.Now;

					dateTime += timeSpan;
				}
				else
				{
					try
					{
						dateTime = DateTime.ParseExact(s, DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString, CultureInfo.InvariantCulture);

						dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
					}
					catch (FormatException e)
					{
						Console.Error.WriteLine(e);

						return;
					}
				}

				EnvironmentUtils.SetSystemTime(dateTime);
			};

			systemClockDirectory.Add(localDateTimeFile);
		}
	}
}
