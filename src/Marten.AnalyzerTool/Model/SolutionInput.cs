using System.Collections.Generic;

namespace Marten.AnalyzerTool.Model
{
    public sealed class SolutionInput
    {
		[Oakton.Description("Solutions to analyze")]
	    public IEnumerable<string> Solutions;
	    [Oakton.Description("Target framework")]
		public string TargetFrameworkFlag;
    }
}