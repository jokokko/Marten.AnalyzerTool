using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colorful;
using HtmlAgilityPack;
using Marten.AnalyzerTool.Model;

namespace Marten.AnalyzerTool.Services
{
	public sealed class ProjectionHtmlReporter : IProjectionReporter
	{
		public void Report(IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> syncProjections, IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> asyncProjections)
		{			
			var allProjections = syncProjections.Select(x => new { x.Key, Proj = x.Value.Select(t => new { t, Sync = true })})
				.Concat(asyncProjections.Select(x => new { x.Key, Proj = x.Value.Select(t => new { t, Sync = false }) })).GroupBy(x => x.Key)
				.ToDictionary(x => x.Key, x => x.First().Proj);

			var doc = new HtmlDocument();
			var body = HtmlNode.CreateNode("<html><head></head><body></body></html>");
			doc.DocumentNode.AppendChild(body);

			var table = doc.CreateElement("table");

			var tableHeader = HtmlNode.CreateNode($@"<thead><tr><th>Projection</th><th>Wired As</th><th>Projection Assembly</th><th>Configuration Assembly</th><th>Source Location</th></thead>");
			var tableBody = doc.CreateElement("tbody");

			tableBody = allProjections.OrderBy(x => x.Key).Aggregate(tableBody, (node, w) =>
			{
				foreach (var r in w.Value.OrderBy(x => x.t.WiringAssembly.Name))
				{
					var row = HtmlNode.CreateNode(
						$@"<tr><td><pre>{w.Key.Enc()}</pre></td><td><pre>{(r.Sync ? "Synchronous" : "Asynchronous")}</pre></td><td><pre>{r.t.ProjectionAssembly.Name.Enc()}</pre></td><td><pre>{r.t.WiringAssembly?.Name.Enc()}</pre></td><td><pre>{r.t.Invocation.GetLocation().ToString().Enc()}</pre></td></tr>");

					tableBody.AppendChild(row);
				}

				return tableBody;
			});

			table.AppendChild(tableHeader);
			table.AppendChild(tableBody);

			doc.DocumentNode.SelectSingleNode("//body").AppendChild(table);

			using (var sb = new StringWriter())
			{
				doc.Save(sb);
				Console.Write(sb);
			}
		}		
	}
}