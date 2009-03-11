using System;

namespace FileWalk
{
	/// <summary>
	/// Summary description for ContainerVisitor.
	/// </summary>
	public interface ContainerVisitor
	{
		void BeginVisitNode(string name, string desc);
		void EndVisitNode();
	}
}
