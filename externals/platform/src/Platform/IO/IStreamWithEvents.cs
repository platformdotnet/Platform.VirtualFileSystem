using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.IO
{
	public interface IStreamWithEvents
	{
		event EventHandler BeforeClose;
		event EventHandler AfterClose;
	}
}
