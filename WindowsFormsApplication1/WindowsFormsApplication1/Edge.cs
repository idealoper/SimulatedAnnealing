using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class Edge
    {
        public Vertex Dst { get; private set; }

		public int Cost { get; private set; }

        public Edge(Vertex dst, int cost)
        {
            Dst = dst;
            Cost = cost;
        }

		public bool IsEqual(Edge secondEdge)
		{
			if (secondEdge == null)
				return false;
			if (secondEdge == this)
				return true;
			if (secondEdge.Cost != this.Cost)
				return false;

			return secondEdge.Dst.IsEqual(Dst);
		}
	}
}
