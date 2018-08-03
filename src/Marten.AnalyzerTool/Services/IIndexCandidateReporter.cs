using System.Collections.Generic;
using Marten.AnalyzerTool.Model;

namespace Marten.AnalyzerTool.Services
{
	public interface IIndexCandidateReporter
	{
		void Report(IEnumerable<IndexedProperty> indices, IEnumerable<IndexCandidate> indexCandidates);
	}
}