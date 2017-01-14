using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class Vertex
    {
        public int Id { get; private set; }

		public Point Position { get; set; }

		public List<Edge> Edges { get; private set; }

        public Vertex(int id)
        {
			Id = id;
            Edges = new List<Edge>();
        }

		public bool IsEqual(Vertex secondVertex)
		{
			if (secondVertex == null)
				return false;
			if (secondVertex == this)
				return true;
			if (secondVertex.Id != this.Id)
				return false;
			if (Edges.Count != secondVertex.Edges.Count)
				return false;

			foreach (var edge in Edges)
			{
				if (!secondVertex.Edges.Any(x => x.IsEqual(edge)))
					return false;
			}

			return true;
		}
	}

	public class Graph: List<Vertex>
	{
		public bool IsEqual(Graph secondGraph)
		{
			if (secondGraph == null)
				return false;
			if (secondGraph == this)
				return true;
			if (Count != secondGraph.Count)
				return false;

			foreach (var vertex in this)
			{
				if (!secondGraph.Any(x => x.IsEqual(vertex)))
					return false;
			}

			return true;
		}
	}
}
