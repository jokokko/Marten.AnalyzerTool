using System.Collections.Generic;

namespace Marten.AnalyzerTool.Infrastructure
{
	public interface IOnMethodInvocation
	{
		HashSet<string> OnMethods { get; }
	}
}