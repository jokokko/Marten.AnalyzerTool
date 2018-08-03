using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Marten.AnalyzerTool.Model
{
	public sealed class IndexCandidate
	{
		public readonly ISymbol Property;
		public readonly InvocationExpressionSyntax Usage;

		public IndexCandidate(ISymbol property, InvocationExpressionSyntax usage)
		{
			Property = property;
			Usage = usage;
		}		
	}
}