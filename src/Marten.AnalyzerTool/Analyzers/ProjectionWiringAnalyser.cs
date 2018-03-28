using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Marten.AnalyzerTool.Infrastructure;
using Marten.AnalyzerTool.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Marten.AnalyzerTool.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class ProjectionWiringAnalyser : MartenAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.Marten1008ProjectionAsSyncAndAsync);

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext ctx, MartenContext martenCtx)
		{
			var analyzer = new Analyzer(this);
			ctx.RegisterSyntaxNodeAction(analyzer.Analyze, SyntaxKind.InvocationExpression);
		}

		private readonly ConcurrentDictionary<string, ConcurrentBag<ProjectionCtx>> syncProjections = new ConcurrentDictionary<string, ConcurrentBag<ProjectionCtx>>();
		private readonly ConcurrentDictionary<string, ConcurrentBag<ProjectionCtx>> asyncProjections = new ConcurrentDictionary<string, ConcurrentBag<ProjectionCtx>>();
		public IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> GetSyncProjections()
		{
			return syncProjections.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
		}

		public IReadOnlyDictionary<string, IEnumerable<ProjectionCtx>> GetAsyncProjections()
		{
			return asyncProjections.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
		}

		private sealed class Analyzer : IOnMethodInvocation
		{
			private readonly ProjectionWiringAnalyser host;
			
			public HashSet<string> OnMethods => new HashSet<string>(new[]
			{
				"ProjectionCollection.AggregateStreamsWith",
				"ProjectionCollection.Add"
			});
			
			public void Analyze(SyntaxNodeAnalysisContext context)
			{                
				var node = (InvocationExpressionSyntax)context.Node;
				var symbol = context.SemanticModel.GetSymbolInfo(node);

				if (symbol.Symbol?.Kind != SymbolKind.Method)
				{
					return;
				}

				var method = (IMethodSymbol)symbol.Symbol;
				if (this.MatchInvocation(method))
				{
					AnalyzeInvocation(context, node, method);
				}
			}
						
			public Analyzer(ProjectionWiringAnalyser host)
			{
				this.host = host;								
			}

	
			private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax node, IMethodSymbol method)
			{
				var memberAccess = node.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();

				if (memberAccess == null)
				{
					return;
				}

				var member = context.SemanticModel.GetSymbolInfo(memberAccess.Expression);

				if (member.Symbol == null)
				{
					return;
				}

				var collection = member.Symbol.Name.Equals("AsyncProjections") ? host.asyncProjections : host.syncProjections;

			    ITypeSymbol t = null;

				if (method.Name.Equals("AggregateStreamsWith", StringComparison.Ordinal))
				{					
					t = method.TypeArguments.FirstOrDefault();					
				}
				else if (method.Name.Equals("Add", StringComparison.Ordinal))
				{					
					var p = node.ArgumentList.Arguments.FirstOrDefault();
				    t = context.SemanticModel.GetTypeInfo(p.Expression).Type;					
				}

				// ReSharper disable once InvertIf
				if (t != null)
				{
				    var ctx = new ProjectionCtx(node, t.ContainingAssembly.Name);
				    collection.GetOrAdd(t.ToDisplayString(Formats.Fqf), s => new ConcurrentBag<ProjectionCtx>()).Add(ctx);
				}
			}
		}
	}
}