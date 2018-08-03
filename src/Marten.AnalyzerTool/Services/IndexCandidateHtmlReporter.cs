using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using Marten.AnalyzerTool.Infrastructure;
using Marten.AnalyzerTool.Model;
using Console = Colorful.Console;

namespace Marten.AnalyzerTool.Services
{
	public sealed class IndexCandidateHtmlReporter : IIndexCandidateReporter
	{
		public void Report(IEnumerable<IndexedProperty> indices, IEnumerable<IndexCandidate> indexCandidates)
		{
			var indexedProperties = indices as IndexedProperty[] ?? indices.ToArray();
			
			var notIndexed = from x in indexCandidates
				where !indexedProperties.Any(t =>
					t.Gin && x.Property.ContainingSymbol.Equals(t.IndexedType) ||
					!t.Gin && x.Property.Equals(t.Member))
				select x;

			var doc = new HtmlDocument();
			var body = HtmlNode.CreateNode("<html><head></head><body></body></html>");
			doc.DocumentNode.AppendChild(body);

			var tableIndices = doc.CreateElement("table");

			var tableHeaderIndices = HtmlNode.CreateNode(@"<thead><tr><th>Index</th><th>Gin</th></thead>");
			var tableBodyIndices = doc.CreateElement("tbody");

			tableBodyIndices = indexedProperties.Select(x => new { x, Presentation = x.ToString() }).DistinctBy(x => x.Presentation).OrderBy(x => x.Presentation).Aggregate(tableBodyIndices, (node, w) =>
			{
				var row = HtmlNode.CreateNode(
					$@"<tr><td><pre>{w.Presentation}</pre></td><td>{(w.x.Gin ? "X" : string.Empty)}</td></tr>");

				tableBodyIndices.AppendChild(row);

				return tableBodyIndices;
			});

			tableIndices.AppendChild(tableHeaderIndices);
			tableIndices.AppendChild(tableBodyIndices);

			doc.DocumentNode.SelectSingleNode("//body").AppendChild(tableIndices);

			var tableIndexCandidates = doc.CreateElement("table");

			var tableHeaderIndexCandidates = HtmlNode.CreateNode(@"<thead><tr><th>Property</th><th>Query Usages</th></thead>");
			var tableBodyIndexCandidates = doc.CreateElement("tbody");

			tableBodyIndexCandidates = notIndexed.GroupBy(x => x.Property).Select(x => new { x, Count = x.Count() }).OrderByDescending(x => x.Count).Aggregate(tableBodyIndexCandidates, (node, w) =>
			{				
				var row = HtmlNode.CreateNode(
					$@"<tr><td><pre>{w.x.Key.ToDisplayString()}</pre></td><td>{w.Count}</td></tr>");

				tableBodyIndexCandidates.AppendChild(row);				

				return tableBodyIndexCandidates;
			});

			tableIndexCandidates.AppendChild(tableHeaderIndexCandidates);
			tableIndexCandidates.AppendChild(tableBodyIndexCandidates);

			doc.DocumentNode.SelectSingleNode("//body").AppendChild(tableIndexCandidates);

			using (var sb = new StringWriter())
			{
				doc.Save(sb);
				Console.Write(sb);
			}
		}
	}
}