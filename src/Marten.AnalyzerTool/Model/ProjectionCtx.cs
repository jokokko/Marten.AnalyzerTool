using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Marten.AnalyzerTool.Model
{
    public sealed class ProjectionCtx
    {
        public ProjectionCtx(InvocationExpressionSyntax invocation, string assembly)
        {
            Invocation = invocation;
            Assembly = assembly;
        }

        public InvocationExpressionSyntax Invocation { get; }
        public string Assembly { get; }
    }
}