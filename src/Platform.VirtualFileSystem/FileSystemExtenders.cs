using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem
{
	public class FileSystemExtenders
	{
		/// <summary>
		/// A list of the <see cref="NodeResolutionFilters"/> used by the file system.
		/// </summary>
		public virtual IList NodeResolutionFilters { get; set; }

		/// <summary>
		/// A list of the <see cref="NodeTransformationFilters"/> used by the file system.
		/// </summary>
		public virtual IList NodeTransformationFilters { get; set; }

		/// <summary>
		/// A list of the <see cref="NodeServiceProviders"/> used by the file system.
		/// </summary>
		public virtual IList NodeServiceProviders { get; set; }

		/// <summary>
		/// NodeOperationFilters
		/// </summary>
		public virtual IList NodeOperationFilters { get; set; }

		public INodeOperationFilter CompositeNodeOperationFilter { get; private set; }

		private readonly IFileSystem fileSystem;

		private class CompositeNodeOperationFilterImpl
			: INodeOperationFilter
		{
			private readonly FileSystemExtenders systemExtenders;

			public CompositeNodeOperationFilterImpl(FileSystemExtenders extenders)
			{
				this.systemExtenders = extenders;
			}

			#region INodeOperationFilter Members

			public INode CopyTo(INode thisNode, INode target, bool overwrite, ref bool operationPerformed, Func<INode, bool, INode> defaultOperator)
			{
				INode retval = null;

				foreach (INodeOperationFilter filter in this.systemExtenders.NodeOperationFilters)
				{
					retval = filter.CopyTo(thisNode, target, overwrite, ref operationPerformed, defaultOperator);
				}

				if (!operationPerformed)
				{
					operationPerformed = true;

					return defaultOperator(target, overwrite);
				}
				else
				{
					return retval;
				}
			}

			public INode MoveTo(INode thisNode, INode target, bool overwrite, ref bool operationPerformed, Func<INode, bool, INode> defaultOperator)
			{
				INode retval = null;

				foreach (INodeOperationFilter filter in this.systemExtenders.NodeOperationFilters)
				{
					retval = filter.MoveTo(thisNode, target, overwrite, ref operationPerformed, defaultOperator);
				}

				if (!operationPerformed)
				{
					operationPerformed = true;

					return defaultOperator(target, overwrite);
				}
				else
				{
					return retval;
				}
			}

			public INode Create(INode thisNode, bool createParent, ref bool operationPerformed, Func<bool, INode> defaultOperator)
			{
				INode retval = null;

				foreach (INodeOperationFilter filter in this.systemExtenders.NodeOperationFilters)
				{
					retval = filter.Create(thisNode, createParent, ref operationPerformed, defaultOperator);
				}

				if (!operationPerformed)
				{
					operationPerformed = true;

					return defaultOperator(createParent);
				}
				else
				{
					return retval;
				}
			}

			public INode Delete(INode thisNode, ref bool operationPerformed, Func<INode> defaultOperator)
			{
				INode retval = null;

				foreach (INodeOperationFilter filter in this.systemExtenders.NodeOperationFilters)
				{
					retval = filter.Delete(thisNode, ref operationPerformed, defaultOperator);
				}
								
				if (!operationPerformed)
				{
					operationPerformed = true;

					return defaultOperator();
				}
				else
				{
					return retval;
				}
			}

			public INode RenameTo(INode thisNode, string name, bool overwrite, ref bool operationPerformed, Func<string, bool, INode> defaultOperator)
			{
				INode retval = null;

				foreach (INodeOperationFilter filter in this.systemExtenders.NodeOperationFilters)
				{
					retval = filter.RenameTo(thisNode, name, overwrite, ref operationPerformed, defaultOperator);
				}

				if (!operationPerformed)
				{
					operationPerformed = true;

					return defaultOperator(name, overwrite);
				}
				else
				{
					return retval;
				}
			}

			#endregion
		}

		public FileSystemExtenders(IFileSystem fileSystem)
		{
			this.NodeServiceProviders = new ArrayList();
			this.NodeTransformationFilters = new ArrayList();
			this.fileSystem = fileSystem;

			this.CreateExtenders();

			this.CompositeNodeOperationFilter = new CompositeNodeOperationFilterImpl(this);
		}

		public virtual object ConstructExtender(Type type)
		{
			try
			{
				return Activator.CreateInstance(type, new object[] { this.fileSystem });
			}
			catch (MissingMethodException)
			{
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}
			
			return Activator.CreateInstance(type);
		}

		protected virtual void CreateExtenders()
		{
			// Construct NodeServiceProviders

			var types = this.fileSystem.Options.NodeServiceProviderTypes;

			this.NodeServiceProviders = new INodeServiceProvider[types.Count];

			for (var i = 0; i < types.Count; i++)
			{
				this.NodeServiceProviders[i] = (INodeServiceProvider)ConstructExtender((Type)types[i]);
			}

			// Construct resolution filters

			types = this.fileSystem.Options.NodeResolutionFilterTypes;

			this.NodeResolutionFilters = new INodeResolutionFilter[types.Count];

			for (var i = 0; i < types.Count; i++)
			{
				try
				{
					this.NodeResolutionFilters[i] = (INodeResolutionFilter)ConstructExtender((Type)types[i]);
				}
				catch (Exception)
				{
				}
			}

			// Construct operation filters

			var typeList = this.fileSystem.Options.NodeOperationFilterTypes;

			this.NodeOperationFilters = new List<INodeOperationFilter>(typeList.Count);

			foreach (var type in typeList)
			{
				this.NodeOperationFilters.Add((INodeOperationFilter)this.ConstructExtender((Type)type));
			}
		}
	}
}
