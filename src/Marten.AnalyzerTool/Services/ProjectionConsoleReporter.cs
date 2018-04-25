using System.Collections.Generic;
using System.Linq;
using Colorful;
using Marten.AnalyzerTool.Model;

namespace Marten.AnalyzerTool.Services
{
	public sealed class ProjectionConsoleReporter : IProjectionReporter
	{
		private readonly Theme theme;

		public ProjectionConsoleReporter(Theme theme)
		{
			this.theme = theme;
		}

		private void WriteLocations(IEnumerable<ProjectionCtx> items)
		{
			foreach (var c in items)
			{
				Console.WriteLine($"\t\t{c.Invocation.GetLocation()}");
			}
		}

		public void Report(IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> syncProjections, IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> asyncProjections)
		{
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

		private void WriteProjections(IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> items, string header = null)
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
	}
}