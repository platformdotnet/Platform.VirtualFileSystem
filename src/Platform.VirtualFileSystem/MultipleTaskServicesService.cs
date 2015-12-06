using System;

//
namespace Platform.VirtualFileSystem
{
	public class MultipleTaskServicesServiceEventArgs
		: EventArgs
	{
		public virtual ITaskService TaskService { get; set; }

		public MultipleTaskServicesServiceEventArgs(ITaskService taskService)
		{
			this.TaskService = taskService;
		}
	}

	public delegate void MultipleTaskServicesServiceEventHandler(object sender, MultipleTaskServicesServiceEventArgs eventArgs);
}

