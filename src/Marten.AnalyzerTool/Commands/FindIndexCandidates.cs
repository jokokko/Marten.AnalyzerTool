using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marten.AnalyzerTool.Model;
using Marten.AnalyzerTool.Services;
using Oakton;

namespace Marten.AnalyzerTool.Commands
{
	[Description("Find index candidates", Name = "index-candidates")]
	public sealed class FindIndexCandidates : OaktonAsyncCommand<SolutionInput>
	{
		public override async Task<bool> Execute(SolutionInput input)
		{
			var service = new IndexCandidateFinder();

			var solutionProperties = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(input.TargetFrameworkFlag))
			{
				solutionProperties["TargetFramework"] = input.TargetFrameworkFlag;
			}

			var reporter = input.HtmlFlag
				? (IIndexCandidateReporter)new IndexCandidateHtmlReporter()
				: new IndexCandidateConsoleReporter();

			await service.BuildCatalog(input.Solutions.Where(File.Exists), reporter, solutionProperties);

			return true;
		}
	}
}