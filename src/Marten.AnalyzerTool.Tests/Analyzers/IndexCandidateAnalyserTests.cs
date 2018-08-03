using System.Linq;
using Marten.AnalyzerTool.Analyzers;
using Marten.AnalyzerTool.Tests.Infrastructure;
using Xunit;

namespace Marten.AnalyzerTool.Tests.Analyzers
{
	public sealed class IndexCandidateAnalyserTests
	{
		[Fact]
		public async void CanIdentifyIndexCandidates()
		{
			var analyzer = new IndexCandidateAnalyser();
			await TestHelper.GetDiagnosticsAsync(analyzer,
				@"using System;
using System.Linq;
using Marten;
using Marten.Schema;

class TestClass
{

	void TestMethod()
	{
		var store = DocumentStore.For(c =>
		{
			c.Schema.For<Item>().Index(x => x.AnotherMember);
			c.Schema.For<Item2>().GinIndexJsonData();
		});

		var item = new Item();

		using (var s = store.OpenSession())
		{
			s.Query<Item>().Where(x => x.SomeMember == """")
						   .Where(x => x.ThirdMember == """");
			//s.Query<Item>().Where(x => x.Id > 0).First();
			//s.Query<Item>().Where(x => x.SomeMember == """" && x.ThirdMember == item.ThirdMember).Where(x => x.SomeMember == ""abc"").First();
			//s.Query<Item>().Where(x => x.SomeMember == """").First();
			//s.Query<Item>().Where(x => x.SomeMember == """").First();
			//s.Query<Item>().Where(x => x.AnotherMember == """").First();
			//s.Query<Item>().Where(x => x.AnotherMember == """").First();
			//s.Query<Item>().Where(x => x.AnotherMember == """").First();
			//s.Query<Item2>().Where(x => x.SomeMember == """").First();
			//s.Query<Item2>().Where(x => x.SomeMember == """").First();
			//s.Query<Item2>().Where(x => x.SomeMember == """").First();
			//s.Query<Item2>().Where(x => x.Identity == 0).First();
		}
	}

	public sealed class Item
	{
		public int Id { get; set; }
		public string SomeMember { get; set; }
		public string AnotherMember { get; set; }
		public string ThirdMember { get; set; }
	}

	public sealed class Item2
	{
		[Identity]
		public int Identity { get; set; }
		public string SomeMember { get; set; }
	}
}");

			var indices = analyzer.GetIndices().Select(x => x.ToString()).ToArray();
			var candidates = analyzer.GetIndexCandidates().Select(x => x.Property.ToDisplayString()).ToArray();

			Assert.Contains("TestClass.Item.AnotherMember", indices);
			Assert.Contains("TestClass.Item.SomeMember", candidates);
			Assert.DoesNotContain("TestClass.Item.AnotherMember", candidates);
		}
	}
}