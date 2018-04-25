using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Marten.AnalyzerTool.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace Marten.AnalyzerTool.Services
{
	public sealed class ProjectionCataloger
	{
	  
		public async Task BuildCatalog(IEnumerable<string> solutions, IProjectionReporter reporter, Dictionary<string, string> solutionProperties = null)
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

			var collector = new ProjectionWiringAnalyser();

			await Task.WhenAll(solutionsToAnalyze.Select(x => AnalyzeSolution(x, collector, solutionProperties)).ToArray())
				.ConfigureAwait(false);
      
			var syncProjections = collector.GetSyncProjections();
			var asyncProjections = collector.GetAsyncProjections();

			reporter.Report(syncProjections, asyncProjections);
		}

		private static async Task AnalyzeSolution(string solutionPath, DiagnosticAnalyzer analyzer, Dictionary<string, string> workspaceProperties = null)
		{			
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