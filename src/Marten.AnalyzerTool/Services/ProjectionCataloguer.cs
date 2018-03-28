using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Marten.AnalyzerTool.Analyzers;
using Marten.AnalyzerTool.Model;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Console = Colorful.Console;

namespace Marten.AnalyzerTool.Services
{
    public sealed class ProjectionCataloger
    {
	    private class Theme
	    {
			public static readonly Theme Current = new Theme();

		    private Theme()
		    {
			    Projection = Color.Yellow;
			    Wired = Color.Green;
			    WiredConflict = Color.Red;
		    }

		    public Color Projection { get; }
		    public Color Wired { get; }
		    public Color WiredConflict { get; }
	    }

        public async Task BuildCatalog(IEnumerable<string> solutions, Dictionary<string, string> solutionProperties = null)
        {
            if (solutions == null)
            {
                throw new ArgumentNullException(nameof(solutions));
            }

            var solutionsToAnalyze = solutions as string[] ?? solutions.ToArray();

            if (!solutionsToAnalyze.Any())
            {
                return;
            }

            var collector = new ProjectionWiringAnalyser();

            await Task.WhenAll(solutionsToAnalyze.Select(x => AnalyzeSolution(x, collector, solutionProperties)).ToArray())
                .ConfigureAwait(false);

            void WriteLocations(IEnumerable<ProjectionCtx> items)
            {
                foreach (var c in items)
                {
                    Console.WriteLine($"\t\t{c.Invocation.GetLocation()}");
                }
            }

	        var theme = Theme.Current;

            void WriteProjections(IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> items, string header = null)
            {
                if (header != null)
                {
                    Console.WriteLine(header, theme.Wired);
                }

                foreach (var s in items.OrderBy(x => x.Key))
                {
                    Console.WriteLine($"\t{s.Key}", theme.Projection);
                    WriteLocations(s.Value);
                }
            }

            var syncProjections = collector.GetSyncProjections();
            var asyncProjections = collector.GetAsyncProjections();

            WriteProjections(syncProjections, "Synchronous projections:");
            WriteProjections(asyncProjections, "Asynchronous projections:");

            var wiredAsSyncAndAsync =
                from s in syncProjections
                orderby s.Key
                where asyncProjections.ContainsKey(s.Key)
                select new { sync = s, async = asyncProjections[s.Key] };

            Console.WriteLine("Wired as synchronous and asynchronous", theme.WiredConflict);
            foreach (var p in wiredAsSyncAndAsync)
            {
                var allLocations = p.sync.Value.Concat(p.async);
                Console.WriteLine($"\t{p.sync.Key}", theme.Projection);
                WriteLocations(allLocations);
            }
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