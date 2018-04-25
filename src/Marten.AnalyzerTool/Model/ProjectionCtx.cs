using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Marten.AnalyzerTool.Model
{
    public sealed class ProjectionCtx
    {
        public ProjectionCtx(InvocationExpressionSyntax invocation, IAssemblySymbol projectionAssembly, IAssemblySymbol wiringAssembly)
        {
            Invocation = invocation;
            ProjectionAssembly = projectionAssembly;
	        WiringAssembly = wiringAssembly;
        }

        public InvocationExpressionSyntax Invocation { get; }
        public IAssemblySymbol ProjectionAssembly { get; }
	    public IAssemblySymbol WiringAssembly { get; }
    }
}