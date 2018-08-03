using Microsoft.CodeAnalysis;

namespace Marten.AnalyzerTool.Model
{
	public sealed class IndexedProperty
	{
		public readonly ISymbol IndexedType;
		public readonly ISymbol Member;
		public readonly bool Gin;

		public IndexedProperty(ISymbol indexedType)
		{
			IndexedType = indexedType;
			Gin = true;
		}
		public IndexedProperty(ISymbol indexedType, ISymbol member)
		{
			IndexedType = indexedType;
			Member = member;
		}

		public override string ToString()
		{
			return Gin ? IndexedType.ToDisplayString() : Member.ToDisplayString();
		}
	}
}