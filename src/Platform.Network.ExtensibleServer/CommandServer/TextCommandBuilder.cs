using System;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	public class TextCommandBuilder
		: CommandBuilder
	{
		public TextCommandBuilder(Connection connection)
			: base(connection)
		{
		}

		public override Command BuildNextCommand()
		{
			string s, name, parameters;
			
			for (;;)
			{
				s = ((ITextConnection)Connection).ReadTextBlock();

				if (s == null)
				{
					throw new ObjectDisposedException("Connection");
				}

				if (s.Trim().Length > 0)
				{
					break;
				}
			}

			var x = s.IndexOf(' ');

			if (x < 0)
			{
				name = s;
				parameters = "";
			}
			else
			{
				name = s.Substring(0, x);
				parameters = s.Substring(x + 1);
			}

			return new TextCommand(name, parameters);
		}
	}
}
