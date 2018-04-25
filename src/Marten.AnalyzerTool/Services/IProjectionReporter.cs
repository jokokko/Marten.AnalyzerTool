using System.Collections.Generic;
using Marten.AnalyzerTool.Model;

namespace Marten.AnalyzerTool.Services
{
	public interface IProjectionReporter
	{
		void Report(IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> syncProjections, IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> asyncProjections);
	}
}