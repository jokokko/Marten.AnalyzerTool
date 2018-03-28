using System.Linq;
using Marten.AnalyzerTool.Analyzers;
using Marten.AnalyzerTool.Tests.Infrastructure;
using Xunit;

namespace Marten.AnalyzerTool.Tests.Analyzers
{
	public sealed class ProjectionWiringAnalyserTests
	{
		[Fact]
		public async void CanCatalogProjections()
		{
			var analyzer = new ProjectionWiringAnalyser();
			await TestHelper.GetDiagnosticsAsync(analyzer,
				@"using System;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Events.Projections.Async;
using Marten.Storage;

class T { }

class TestClass { 
	
	void TestMethod() 
	{
		DocumentStore.For(c =>
        {                
			c.Events.AsyncProjections.Add(new Projection());
			c.Events.AsyncProjections.AggregateStreamsWith<TestClass>();			
			c.Events.AsyncProjections.AggregateStreamsWith<T>();
		});
	}

	void TestMethod2() 
	{
		DocumentStore.For(c =>
        {                
			c.Events.InlineProjections.Add(new Projection());			
			c.Events.InlineProjections.AggregateStreamsWith<TestClass>();
			c.Events.AsyncProjections.AggregateStreamsWith<T>();
		});
	}

	class Projection : IProjection
	{
		public void Apply(IDocumentSession session, EventPage page)
		{
			throw new NotImplementedException();
		}

		public Task ApplyAsync(IDocumentSession session, EventPage page, CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public void EnsureStorageExists(ITenant tenant)
		{
			throw new NotImplementedException();
		}

		public Type[] Consumes { get; }
		public AsyncOptions AsyncOptions { get; }
	}
}");

			var asyncProjections = analyzer.GetAsyncProjections().Select(x => x.Key).ToArray();
			var syncProjections = analyzer.GetSyncProjections().Select(x => x.Key).ToArray();

			Assert.Contains("TestClass.Projection", syncProjections);
			Assert.Contains("TestClass.Projection", asyncProjections);
			Assert.Contains("TestClass", syncProjections);
			Assert.Contains("TestClass", asyncProjections);
		}
	}
}