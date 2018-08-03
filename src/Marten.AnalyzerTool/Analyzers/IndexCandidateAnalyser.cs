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
	public sealed class IndexCandidateAnalyser : MartenAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.Marten1009IndexCandidate);

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext ctx, MartenContext martenCtx)
		{
			var analyzer = new Analyzer(this);
			ctx.RegisterSyntaxNodeAction(analyzer.Analyze, SyntaxKind.InvocationExpression);
		}

		public IEnumerable<IndexedProperty> GetIndices()
		{
			return indexedProperties;
		}

		public IEnumerable<IndexCandidate> GetIndexCandidates()
		{
			return indexCandidates;
		}

		private readonly ConcurrentBag<IndexedProperty> indexedProperties = new ConcurrentBag<IndexedProperty>();
		private readonly ConcurrentBag<IndexCandidate> indexCandidates = new ConcurrentBag<IndexCandidate>();

		private sealed class Analyzer : IOnMethodInvocation
		{
			private readonly IndexCandidateAnalyser host;

			public HashSet<string> OnMethods => new HashSet<string>(new[]
			{
				"DocumentMappingExpression.Index",
				"DocumentMappingExpression.GinIndexJsonData",
				"Queryable.Where",
				"Queryable.FirstOrDefault",
				"Queryable.First",
				"Queryable.Single",
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

			public Analyzer(IndexCandidateAnalyser host)
			{
				this.host = host;
			}

			private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax node, IMethodSymbol method)
			{
				if (method.Name.Equals("Index", StringComparison.Ordinal))
				{
					GetIndexedProperty(context, node);

					return;
				}

				if (method.Name.Equals("GinIndexJsonData", StringComparison.Ordinal) && method.ContainingSymbol is INamedTypeSymbol schema)
				{
					var registryType = schema.TypeArguments[0];
					var indexedProperty = new IndexedProperty(registryType);
					host.indexedProperties.Add(indexedProperty);

					return;
				}

				bool IsMartenQueryable(ExpressionSyntax n)
				{
					if (n == null)
					{
						return false;
					}

					var s = context.SemanticModel.GetSymbolInfo(n);

					return s.Symbol?.ContainingSymbol != null && s.Symbol.ContainingSymbol.ToDisplayString().Equals("Marten.IQuerySession", StringComparison.Ordinal);
				}

				if (!node.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Any(IsMartenQueryable))
				{
					return;
				}

				var descendInto = node.ArgumentList.DescendantNodes().OfType<LambdaExpressionSyntax>().ToArray();

				EvaluateDescendants(context, node, descendInto);
			}

			private void EvaluateDescendants(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax node,
				LambdaExpressionSyntax[] descendInto)
			{
				foreach (var syntax in descendInto)
				{
					var descendants = syntax.DescendantNodes().ToArray();

					var paramSyntax = descendants.OfType<ParameterSyntax>().FirstOrDefault();

					if (paramSyntax == null)
					{
						continue;
					}

					var arg = context.SemanticModel.GetDeclaredSymbol(paramSyntax);

					if (arg == null)
					{
						continue;
					}

					foreach (var m in descendants.OfType<MemberAccessExpressionSyntax>())
					{
						var accessSym = context.SemanticModel.GetSymbolInfo(m);
						var ownerSym = context.SemanticModel.GetSymbolInfo(m.Expression);

						if (accessSym.Symbol.GetAttributes().Any(x =>
							    x.AttributeClass.ToDisplayString().Equals("Marten.Schema.IdentityAttribute")) ||
						    accessSym.Symbol.Name.Equals("Id", StringComparison.Ordinal))
						{
							continue;
						}

						if (ownerSym.Symbol.Equals(arg))
						{
							host.indexCandidates.Add(new IndexCandidate(accessSym.Symbol, node));
						}
					}
				}
			}

			private void GetIndexedProperty(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax node)
			{
				var argNode = node.ArgumentList.Arguments.FirstOrDefault();
				var lambdaNode = argNode?.Expression as SimpleLambdaExpressionSyntax;

				if (!(lambdaNode?.Body is MemberAccessExpressionSyntax access))
				{
					return;
				}

				var symbol = context.SemanticModel.GetSymbolInfo(access.Expression);
				var member = context.SemanticModel.GetSymbolInfo(access.Name);
				var index = new IndexedProperty(symbol.Symbol, member.Symbol);
				host.indexedProperties.Add(index);
			}
		}
	}
}