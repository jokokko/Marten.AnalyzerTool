﻿using Microsoft.CodeAnalysis;

namespace Marten.AnalyzerTool.Infrastructure
{
	public static class OnInvocationMixins
	{
		public static bool MatchInvocation(this IOnMethodInvocation instance, IMethodSymbol methodSymbol)
		{
			return instance.OnMethods.Contains($"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}");
		}
	}
}