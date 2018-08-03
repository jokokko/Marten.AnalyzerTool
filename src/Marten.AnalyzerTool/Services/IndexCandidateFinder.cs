using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Marten.AnalyzerTool.Analyzers;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace Marten.AnalyzerTool.Services
{
	public sealed class IndexCandidateFinder
	{	  
		public async Task BuildCatalog(IEnumerable<string> solutions, IIndexCandidateReporter reporter, Dictionary<string, string> solutionProperties = null)
		{
			if (solutions == null)
			{
				throw new ArgumentNullException(nameof(solutions));
			}

			if (reporter == null)
			{
				throw new ArgumentNullException(nameof(reporter));
			}

			var solutionsToAnalyze = solutions as string[] ?? solutions.ToArray();

			if (!solutionsToAnalyze.Any())
			{
				return;
			}

			var collector = new IndexCandidateAnalyser();

			await Task.WhenAll(solutionsToAnalyze.Select(x => AnalyzeSolution(x, collector, solutionProperties)).ToArray())
				.ConfigureAwait(false);
      
			var indices = collector.GetIndices();
			var indexCandidates = collector.GetIndexCandidates();
			
			reporter.Report(indices, indexCandidates);
		}

		private static async Task AnalyzeSolution(string solutionPath, DiagnosticAnalyzer analyzer, Dictionary<string, string> workspaceProperties = null)
		{
			MSBuildLocator.RegisterDefaults();
			var solution = await MSBuildWorkspace.Create(workspaceProperties ?? new Dictionary<string, string>()).OpenSolutionAsync(solutionPath).ConfigureAwait(false);
			var analyzers = ImmutableArray.Create(analyzer);

			foreach (var s in solution.Projects)
			{
				var compilation = await s.GetCompilationAsync().ConfigureAwait(false);
				await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
			}
		}
	}
}