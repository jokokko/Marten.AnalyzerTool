using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marten.AnalyzerTool.Model;
using Marten.AnalyzerTool.Services;
using Oakton;

namespace Marten.AnalyzerTool.Commands
{
    [Description("Build projections catalog", Name = "projection-catalog")]
    public sealed class BuildProjectionCatalog : OaktonAsyncCommand<SolutionInput>
    {
        public override async Task<bool> Execute(SolutionInput input)
        {
            var service = new ProjectionCataloger();

	        var solutionProperties = new Dictionary<string, string>();

	        if (!string.IsNullOrEmpty(input.TargetFrameworkFlag))
	        {
		        solutionProperties["TargetFramework"] = input.TargetFrameworkFlag;
	        }

            await service.BuildCatalog(input.Solutions.Where(File.Exists), solutionProperties);

            return true;
        }
    }
}