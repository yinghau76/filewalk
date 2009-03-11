using System;

namespace FileWalk
{
	/// <summary>
	/// Summary description for ContainerFile.
	/// </summary>
	public interface ContainerFile
	{
		void Walk(ContainerVisitor visitor);
	}
}
