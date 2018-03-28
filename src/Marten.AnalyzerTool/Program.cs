using System.Linq;
using System.Threading.Tasks;
using Oakton;

namespace Marten.AnalyzerTool
{
    internal static class Program
	{
	    private static async Task Main(string[] args)
		{
		    var executor = CommandExecutor.For(c =>
		    {
		        c.RegisterCommands(typeof(Program).Assembly);
		    });

		    if (!args.Any())
		    {
		        args = new[] {"help"};
		    }

		    await executor.ExecuteAsync(args).ConfigureAwait(false);
		}		
	}
}
