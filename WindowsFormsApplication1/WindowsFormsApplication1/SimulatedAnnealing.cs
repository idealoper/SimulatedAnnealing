using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
	class SimulatedAnnealing
	{
		const double INITIAL_TEMPERATURE = 1000.0;
		const int COOLING_STEP = 200;
		const double COOLING_FRACTION = 0.97;
		const int STEPS_PER_TEMP = 10;
		const double K = 0.000001;

		public static bool IsRun { get; private set; }
		public static double Temperature { get; private set; }

		public static void Run(Graph graph, Action refresh, Size size)
		{
			if (!IsRun)
			{
				IsRun = true;

				ThreadPool.QueueUserWorkItem(state =>
				{
					var avgEdgeLength = graph.SelectMany(x => x.Edges.Select(e => e.GetLength())).Average();
					Func<double> GetSolutionCost = () => CalcSolutionCost(graph, avgEdgeLength);

					var rand = new Random(DateTime.Now.Millisecond);
					Temperature = INITIAL_TEMPERATURE;
					double solutionCost = GetSolutionCost();
					for (int i = 0; i < COOLING_STEP; i++)
					{
						Temperature *= COOLING_FRACTION;

						var startValue = solutionCost;
						for (int j = 0; j < STEPS_PER_TEMP; j++)
						{
							var vertexIndex = rand.Next(0, graph.Count);
							var vertex = graph[vertexIndex];
							var moveDirection = (Direction)rand.Next(0, 7);
							var moveDistance = (float)rand.NextDouble() * avgEdgeLength;
							var position = vertex.Position;
							Move(vertex, moveDirection, moveDistance, size);
							refresh();
							var flip = rand.NextDouble() * 1.1;
							var currentSolutionCost = GetSolutionCost();
							var delta = solutionCost - currentSolutionCost;
							var exponent = (-delta / solutionCost) / (K * Temperature);
							var merit = Math.Pow(Math.E, exponent);
							if (delta < 0) //Принять удачный вариант
							{
								solutionCost = currentSolutionCost;
								refresh();
							}
							else
							{
								if (merit > flip) //Принять неудачный вариант
								{
									solutionCost = currentSolutionCost;
									refresh();
								}
								else //Отклонить
								{
									vertex.Position = position;
								}
							}

							
						}

						if(solutionCost - startValue < 0)
						{
							Temperature = Temperature / COOLING_FRACTION;
						}

					}
					IsRun = false;
				});
			}
		}

		private static void Move(Vertex vertex, Direction moveDirection, float moveDistance, Size size)
		{
			switch (moveDirection)
			{
				case Direction.Left:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X - (int)moveDistance >= 0 ? vertex.Position.X - (int)moveDistance: vertex.Position.X, vertex.Position.Y);
					}
					break;
				case Direction.Right:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X + (int)moveDistance < size.Width ? vertex.Position.X + (int)moveDistance: vertex.Position.X, vertex.Position.Y);
					}
					break;
				case Direction.Up:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X, vertex.Position.Y - (int)moveDistance > 0 ? vertex.Position.Y - (int)moveDistance : vertex.Position.Y);
					}
					break;
				case Direction.Down:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X, vertex.Position.Y + (int)moveDistance < size.Height ? vertex.Position.Y + (int)moveDistance : vertex.Position.Y);
					}
					break;
				case Direction.LeftUp:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X - (int)(moveDistance / 2) > 0 ? vertex.Position.X - (int)(moveDistance / 2) : vertex.Position.X , vertex.Position.Y - (int)(moveDistance / 2) > 0 ? vertex.Position.Y - (int)(moveDistance / 2) : vertex.Position.Y);
					}
					break;
				case Direction.RightUp:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X + (int)(moveDistance / 2) < size.Width ? vertex.Position.X + (int)(moveDistance / 2): vertex.Position.X, vertex.Position.Y - (int)(moveDistance / 2) > 0 ? vertex.Position.Y - (int)(moveDistance / 2) : vertex.Position.Y);
					}
					break;
				case Direction.LeftDown:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X - (int)(moveDistance / 2) > 0 ? vertex.Position.X - (int)(moveDistance / 2) : vertex.Position.X, vertex.Position.Y + (int)(moveDistance / 2) < size.Height ? vertex.Position.Y + (int)(moveDistance / 2): vertex.Position.Y);
					}
					break;
				case Direction.RightDown:
					{
						vertex.Position = new System.Drawing.Point(vertex.Position.X + (int)(moveDistance / 2) < size.Width ? vertex.Position.X + (int)(moveDistance / 2) : vertex.Position.X, vertex.Position.Y + (int)(moveDistance / 2) < size.Height ? vertex.Position.Y + (int)(moveDistance / 2) : vertex.Position.Y);
					}
					break;
				default:
					break;
			}
		}

		enum Direction
		{
			Left,
			Right,
			Up,
			Down,
			LeftUp,
			RightUp,
			LeftDown,
			RightDown,
		}

		private static double CalcSolutionCost(Graph graph, float edgeLength)
		{
			var c = graph.SelectMany(x => x.Edges.Select(e => e.GetLength())).Where(x => Math.Abs(x / edgeLength) > 0.9 && Math.Abs(x / edgeLength) < 1.1).Count() / 2;
			return (((double)c * 2)/ graph.SelectMany(x => x.Edges).Count()) *100.0;
		}
	}
}
