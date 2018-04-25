using System.Drawing;

namespace Marten.AnalyzerTool.Model
{
	public sealed class Theme
	{
		public static readonly Theme Default = new Theme();

		private Theme()
		{
			Projection = Color.Yellow;
			Wired = Color.Green;
			WiredConflict = Color.Red;
		}

		public Color Projection { get; }
		public Color Wired { get; }
		public Color WiredConflict { get; }
	}
}