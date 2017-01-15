using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
	public partial class Form1 : Form
	{
		public const int D = 24;
		private Graph graph;
		private List<GraphicVertex> graphicVertexes;
		private List<GraphicEdge> graphicEdges;
		private List<float> lengthes;

		public Form1()
		{
			InitializeComponent();
			panel1.Paint += OnPaint;
			listBox1.DrawItem += ListBox1_DrawItem;
		}

		private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
		{
			if(e.Index < lengthes.Count)
			{
				var val = lengthes[e.Index];
				var avg = lengthes.Average();
				if(((avg / val) - (int)(avg / val)) < 0.2)
				{
					e.Graphics.FillRectangle(Brushes.Gainsboro, e.Bounds);
				}else
				{
					e.DrawBackground();
				}
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			SimulatedAnnealing.Run(graph, () => Invoke(new Action(UpdateGraphics)), panel1.Size);
		}

		private void LoadFromFile(string fileName)
		{
			var loadedGraph = (Graph)GraphXmlObject.Load(fileName);
			UpdateGraph(loadedGraph);
			saveFilePath = fileName;
		}

		private void SaveToFile(string fileName)
		{
			((GraphXmlObject)graph).Save(fileName, isOverrideExists: true);
		}

		private void Regenerate()
		{
			saveFilePath = null;
			UpdateGraph(GenerateGraph(10));
		}

		private void UpdateGraph(Graph graph)
		{
			this.graph = graph;

			UpdateGraphics();
		}


		private void UpdateGraphics()
		{
			listBox1.Items.Clear();
			UpdateGraphGraphics();
			UpdateLengthesListBox();
			Refresh();
		}

		private void UpdateLengthesListBox()
		{
			lengthes = new List<float>();
			lengthes.AddRange(graphicEdges.Select(x => x.CalcLength()).OrderBy(x => x));
			var avg = lengthes.Average();
			foreach (var length in lengthes)
			{
				if ((avg / length) < 1.1 && (avg / length) > 0.9)
				{
					listBox1.Items.Add("----" + length.ToString("F"));
				}
				else
				{
					listBox1.Items.Add(length.ToString("F"));
				}

			}

			listBox1.Items.Add("---");
			listBox1.Items.Add($"Min:{lengthes.Min().ToString("F")}");
			listBox1.Items.Add($"Max:{lengthes.Max().ToString("F")}");
			listBox1.Items.Add($"Avg:{lengthes.Average().ToString("F")}");
		}

		private void UpdateGraphGraphics()
		{
			graphicVertexes = new List<GraphicVertex>();
			graphicEdges = new List<GraphicEdge>();

			foreach (var vertex in graph)
			{
				graphicVertexes.Add(new GraphicVertex(vertex.Id, vertex.Position));
				foreach (var edge in vertex.Edges)
				{
					var id1 = vertex.Id;
					var id2 = edge.Dst.Id;
					var cost = edge.Cost;

					if (!IsExistsGraphicEdge(id1, id2, cost))
					{
						graphicEdges.Add(new GraphicEdge(graphicVertexes, id1, id2, cost));
					}
				}
			}

			Refresh();
		}

		private bool IsExistsGraphicEdge(int id1, int id2, int cost)
		{
			return graphicEdges.Any(x => x.Cost == cost && (x.Id1 == id1 && x.Id2 == id2) || (x.Id1 == id2 && x.Id2 == id1));
		}

		private Point GetNewPosition(IEnumerable<Point> points, Random rand)
		{
			var x = rand.Next(D, panel1.Width - 2 * D);
			var y = rand.Next(D, panel1.Height - 2 * D);

			var rect = new Rectangle(x, y, 0, 0);
			rect.Inflate(D, D);

			if (points.Any(z => rect.Contains(z)))
			{
				return GetNewPosition(points, rand);
			}

			return new Point(x, y);
		}


		private void OnPaint(object sender, PaintEventArgs e)
		{
			if (graphicEdges != null && graphicVertexes != null)
			{
				graphicEdges.ForEach(x => x.Draw(e.Graphics));
				graphicVertexes.ForEach(x => x.Draw(e.Graphics));
			}

			temperatureTextBox.Visible = SimulatedAnnealing.IsRun;
			var tempText = SimulatedAnnealing.Temperature.ToString("F2") + "º";
			if (temperatureTextBox.Text != tempText)
			{
				temperatureTextBox.Text = tempText;
			}
		}

		private Graph GenerateGraph(int vertexCount)
		{
			var rand = new Random(DateTime.Now.Millisecond);

			var res = new Graph();
			for (int i = 0; i < vertexCount; i++)
			{
				res.Add(new Vertex(i + 1) { Position = GetNewPosition(res.Select(x => x.Position), rand) });
			}

			foreach (var vertex in res)
			{
				var linkCount = rand.Next(1, vertexCount - 1);
				if (vertex.Edges.Count < linkCount)
				{
					var count = linkCount - vertex.Edges.Count;
					for (int i = 0; i < count; i++)
					{
						int index = -1;
						while (index == -1)
						{
							index = rand.Next(0, vertexCount);
							if (res[index] == vertex || vertex.Edges.Any(x => x.Dst == res[index]))
							{
								index = -1;
							}
						}
						var cost = rand.Next(1, 10);
						vertex.Edges.Add(new Edge(vertex, res[index], cost));
						res[index].Edges.Add(new Edge(res[index], vertex, cost));
					}
				}
			}

			return res;
		}

		#region Menu "File"
		const string GRAPH_FILE_EXT = ".wg";
		private string saveFilePath;

		private void newMenuItem_Click(object sender, EventArgs e)
		{
			Regenerate();
		}

		private void openMenuItem_Click(object sender, EventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.Title = "Select graph file";
			dialog.AddExtension = true;
			dialog.CheckFileExists = true;
			dialog.CheckPathExists = true;
#if DEBUG
			var executionAppDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
			dialog.CustomPlaces.Add(new FileDialogCustomPlace(executionAppDir));
			dialog.InitialDirectory = executionAppDir;
#endif
			dialog.DefaultExt = GRAPH_FILE_EXT;
			dialog.Filter = $"Graph files (*{GRAPH_FILE_EXT})|*{GRAPH_FILE_EXT}";
			dialog.Multiselect = false;
			dialog.RestoreDirectory = true;
			if(dialog.ShowDialog() == DialogResult.OK)
			{
				LoadFromFile(dialog.FileName);
			}
		}

		private void saveMenuItem_Click(object sender, EventArgs e)
		{
			SaveGraph();
		}

		private void saveAsMenuItem_Click(object sender, EventArgs e)
		{
			saveFilePath = null;

			SaveGraph();
		}

		private void SaveGraph()
		{
			if (string.IsNullOrWhiteSpace(saveFilePath))
			{
				var dialog = new SaveFileDialog();
				dialog.Title = "Save graph file";
				dialog.AddExtension = true;
				dialog.CheckPathExists = true;
#if DEBUG
				var executionAppDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
				dialog.CustomPlaces.Add(new FileDialogCustomPlace(executionAppDir));
				dialog.InitialDirectory = executionAppDir;
#endif
				dialog.DefaultExt = GRAPH_FILE_EXT;
				dialog.Filter = $"Graph files (*{GRAPH_FILE_EXT})|*{GRAPH_FILE_EXT}";
				dialog.RestoreDirectory = true;
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					saveFilePath = dialog.FileName;
				}
			}

			if (!string.IsNullOrWhiteSpace(saveFilePath))
			{
				SaveToFile(saveFilePath);
			}
		}

		private void exitMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}
		#endregion

		private void showWeightMenuItem_Click(object sender, EventArgs e)
		{
			GLOBAL_PARAMETERS.WhetherShowWeight = showWeightMenuItem.Checked;
			showWeightMenuItem.Text = showWeightMenuItem.Checked ? "Hide weights" : "Show weights";
			Refresh();
		}
	}

	

	interface IGraphicObject
	{
		void Draw(Graphics graphics);
	}

	class GraphicVertex : IGraphicObject
	{
		public int Id { get; private set; }

		public Point Position { get; private set; }

		public GraphicVertex(int id, Point position)
		{
			Id = id;
			Position = position;
		}

		public void Draw(Graphics graphics)
		{
			var rect = new Rectangle(Position, new Size(0, 0));
			rect.Inflate((Form1.D / 2), (Form1.D / 2));

			graphics.FillEllipse(Brushes.Gainsboro, rect);
			graphics.DrawEllipse(Pens.Black, rect);
			var font = new Font("Arial", 9, FontStyle.Regular);
			var size = graphics.MeasureString(Id.ToString(), font);
			var textRect = new RectangleF(Position, new Size(0, 0));
			textRect.Inflate(size.Width / 2f, size.Height / 2f);
			graphics.DrawString(Id.ToString(), font, Brushes.Black, textRect);
		}
	}

	class GraphicEdge : IGraphicObject
	{
		private IEnumerable<GraphicVertex> vertexes;

		public int Id1 { get; private set; }

		public int Id2 { get; private set; }

		public int Cost { get; private set; }

		public GraphicEdge(IEnumerable<GraphicVertex> vertexes, int id1, int id2, int cost)
		{
			this.vertexes = vertexes;
			Id1 = id1;
			Id2 = id2;
			Cost = cost;
		}

		public void Draw(Graphics graphics)
		{
			var p1 = vertexes.First(x => x.Id == Id1).Position;
			var p2 = vertexes.First(x => x.Id == Id2).Position;

			graphics.DrawLine(Pens.Red, p1, p2);

			if (GLOBAL_PARAMETERS.WhetherShowWeight)
			{
				var pL = new Point(p1.X - (int)((((float)p1.X) - p2.X) / 2f), p1.Y - (int)((((float)p1.Y) - p2.Y) / 2f));

				var rect = new Rectangle(pL, new Size(0, 0));
				rect.Inflate(Form1.D / 2, Form1.D / 2);

				graphics.FillEllipse(Brushes.Blue, rect);
				graphics.DrawEllipse(Pens.Red, rect);
				var font = new Font("Arial", 9, FontStyle.Regular);
				var size = graphics.MeasureString(GetLabel(), font);
				var textRect = new RectangleF(pL, new Size(0, 0));
				textRect.Inflate(size.Width / 2f, size.Height / 2f);
				graphics.DrawString(GetLabel(), font, Brushes.White, textRect);
			}
		}

		private string GetLabel()
		{
			return string.Format("{0}", Cost);
		}

		public float CalcLength()
		{
			var p1 = vertexes.First(x => x.Id == Id1).Position;
			var p2 = vertexes.First(x => x.Id == Id2).Position;

			return CalcDistance(p1, p2);
		}

		private float CalcDistance(Point p1, Point p2)
		{
			var a = (double)Math.Abs(p1.X - p2.X);
			var b = (double)Math.Abs(p1.Y - p2.Y);

			return (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
		}
	}
}
