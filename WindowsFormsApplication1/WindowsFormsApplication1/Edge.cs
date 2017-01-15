using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class Edge
    {
        public Vertex Src { get; private set; }

		public Vertex Dst { get; private set; }

		public int Cost { get; private set; }

		public float GetLength()
		{
			return CalcDistance(Src.Position, Dst.Position);
		}

		private float CalcDistance(Point p1, Point p2)
		{
			var a = (double)Math.Abs(p1.X - p2.X);
			var b = (double)Math.Abs(p1.Y - p2.Y);

			return (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
		}

		public Edge(Vertex src, Vertex dst, int cost)
        {
			Src = src;
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
