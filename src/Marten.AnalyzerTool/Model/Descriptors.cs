using Marten.AnalyzerTool.Infrastructure;
using Microsoft.CodeAnalysis;

namespace Marten.AnalyzerTool.Model
{
    internal static class Descriptors
    {
        private static DiagnosticDescriptor Rule(string id, string title, RuleCategory category, DiagnosticSeverity defaultSeverity, string messageFormat, string description = null)
        {            
            return new DiagnosticDescriptor(id, title, messageFormat, category.Name, defaultSeverity, true, description, $"https://jokokko.github.io/marten.analyzers/rules/{id}");
        }
        
	    internal static readonly DiagnosticDescriptor Marten1008ProjectionAsSyncAndAsync = Rule("Marten1009", "Projection wired as synchronous and asynchronous", RuleCategory.Usage, DiagnosticSeverity.Warning, "Projection '{0}' wired as synchronous and asynchronous.");
	}
}