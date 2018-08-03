using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Marten.AnalyzerTool.Model;
using Console = Colorful.Console;

namespace Marten.AnalyzerTool.Services
{
	public sealed class IndexCandidateConsoleReporter : IIndexCandidateReporter
	{
		public void Report(IEnumerable<IndexedProperty> indices, IEnumerable<IndexCandidate> indexCandidates)
		{
			Console.WriteLine("Indices", Color.Yellow);

			var indexedProperties = indices as IndexedProperty[] ?? indices.ToArray();

			foreach (var i in indexedProperties.Select(x => x.ToString()).Distinct().OrderBy(x => x))
			{
				Console.WriteLine($"\t{i}");
			}

			var notIndexed = from x in indexCandidates
				where !indexedProperties.Any(t =>
					t.Gin && x.Property.ContainingSymbol.Equals(t.IndexedType) ||
					!t.Gin && x.Property.Equals(t.Member))
				select x;

			Console.WriteLine($"{Environment.NewLine}Index Candidates", Color.Yellow);

			foreach (var ic in notIndexed.GroupBy(x => x.Property).Select(x => new { x, Count = x.Count() }).OrderByDescending(x => x.Count))
			{
				Console.WriteLine($"\t{ic.x.Key.ToDisplayString()} ({ic.Count} usages in queries)");
			}
		}
	}
}