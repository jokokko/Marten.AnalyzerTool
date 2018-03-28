using Microsoft.CodeAnalysis;

namespace Marten.AnalyzerTool.Analyzers
{
    internal static class Formats
    {
        public static SymbolDisplayFormat Fqf { get; } =
            new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,                                
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.ExpandNullable);
    }
}