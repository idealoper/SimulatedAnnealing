using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WindowsFormsApplication1
{
	[Serializable]
	public class GraphXmlObject
	{
		[XmlArray(ElementName = "Vertex")]
		public VertexXmlObject[] Vertexes { get; set; }

		[XmlArray(ElementName = "Edge")]
		public EdgeXmlObject[] Edges { get; set; }

		public void Save(string fileName, bool isOverrideExists)
		{
			using (var fs = new FileStream(fileName, isOverrideExists ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
			{
				new XmlSerializer(typeof(GraphXmlObject)).Serialize(fs, this);
			}
		}

		public static GraphXmlObject Load(string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				return (GraphXmlObject)new XmlSerializer(typeof(GraphXmlObject)).Deserialize(fs);
			}
		}

		public static explicit operator Graph(GraphXmlObject graphXmlObject)
		{
			var result = new Graph();

			result.AddRange(graphXmlObject.Vertexes.Select(x => new Vertex(x.Id) { Position = x.Position }));

			foreach(var edgeXmlObject in graphXmlObject.Edges)
			{
				var vertex1 = result.First(x => x.Id == edgeXmlObject.VertexId1);
				var vertex2 = result.First(x => x.Id == edgeXmlObject.VertexId2);
				vertex1.Edges.Add(new Edge(vertex2, edgeXmlObject.Cost));
				vertex2.Edges.Add(new Edge(vertex1, edgeXmlObject.Cost));
			}

			return result;
		}

		public static explicit operator GraphXmlObject(Graph graph)
		{
			var vertexes = new List<VertexXmlObject>();
			var edges = new List<EdgeXmlObject>();
			foreach (var vertex in graph)
			{
				vertexes.Add(new VertexXmlObject { Id = vertex.Id, Position = vertex.Position });
				foreach (var edge in vertex.Edges)
				{
					if (!edges.Any(
						x => x.Cost == edge.Cost
						&&
						((x.VertexId1 == vertex.Id && x.VertexId2 == edge.Dst.Id) || (x.VertexId2 == vertex.Id && x.VertexId1 == edge.Dst.Id))))
					{
						edges.Add(new EdgeXmlObject { VertexId1 = vertex.Id, VertexId2 = edge.Dst.Id, Cost = edge.Cost });
					}
				}
			}

			return new GraphXmlObject()
			{
				Vertexes = vertexes.ToArray(),
				Edges = edges.ToArray(),
			};
		}
	}

	[Serializable]
	public class VertexXmlObject
	{
		[XmlAttribute]
		public int Id { get; set; }

		[XmlIgnore]
		public Point Position { get; set; }

		[XmlAttribute]
		public int X
		{
			get
			{
				return Position.X;
			}
			set
			{
				if (Position.X != value)
				{
					Position = new Point(value, Position.Y);
				}
			}
		}

		[XmlAttribute]
		public int Y
		{
			get
			{
				return Position.Y;
			}
			set
			{
				if (Position.Y != value)
				{
					Position = new Point(Position.X, value);
				}
			}
		}
	}

	[Serializable]
	public class EdgeXmlObject
	{
		[XmlAttribute]
		public int VertexId1 { get; set; }

		[XmlAttribute]
		public int VertexId2 { get; set; }

		[XmlAttribute]
		public int Cost { get; set; }
	}
}
