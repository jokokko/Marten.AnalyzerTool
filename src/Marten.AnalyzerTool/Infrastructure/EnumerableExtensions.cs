using System;
using System.Collections.Generic;
using System.Linq;

namespace Marten.AnalyzerTool.Infrastructure
{
	internal static class EnumerableExtensions
	{
		public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
		{
			return items.GroupBy(property).Select(x => x.First());
		}
	}
}