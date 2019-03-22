using System;
using System.IO;

namespace RayLight
{
	class Triangle
		{


		/*
		 * A simple, explicit/non-vertex-shared triangle.<br/><br/>
		 *
		 * Includes geometry and quality.<br/><br/>
		 *
		 * Constant.<br/><br/>
		 *
		 * @implementation
		 * Adapts ray intersection code from:
		 * <cite>'Fast, Minimum Storage Ray-Triangle Intersection'
		 * Moller, Trumbore;
		 * Journal of Graphics Tools, v2 n1 p21, 1997.
		 * http://www.acm.org/jgt/papers/MollerTrumbore97/</cite>
		 *
		 * @invariants
		 * * reflectivity_m >= 0 and <= 1
		 * * emitivity_m    >= 0
		 */


		/// <summary>
		/// Store geometry here
		/// </summary>
		Vector[] verts = new Vector[3];

		/// <summary>
		/// Material items
		/// </summary>
		public Vector Reflectivity { get; set; }
		public Vector Emitivity { get; set; }

		// one mm seems reasonable...
		public static float TOLERANCE = 1.0f / 1024.0f;

		public Vector Normal { get; set; }
		void MakeNormal()
			{
			if (Normal == null)
				Normal = Tangent.Cross(verts[2] - verts[1]).Unitize();
			}


		public Vector Tangent {get; set; }
		void MakeTangent()
			{
			if (Tangent == null)
				Tangent = (verts[1] - verts[0]).Unitize();
			}


		public float Area {get; set; }
		void MakeArea()
			{
			// half area of parallelogram
			Vector pa2 = edge1.Cross(verts[2] - verts[1]);
			Area = (float)Math.Sqrt(pa2.Dot(pa2)) * 0.5f;
			}

		float[] bound;
		/// <summary>
		/// Get bounding box for this triangle
		/// </summary>
		/// <param name="bound"></param>
		public float [] GetBound()
			{
			if (bound == null)
				{ // create and cache bounding box
				bound = new float[6];
				// initialize
				for (int i = 6; i-- > 0; bound[i] = verts[2][i % 3]) ;

				// expand
				for (int i = 0; i < 3; ++i)
					{
					for (int j = 0, d = 0, m = 0; j < 6; ++j, d = j / 3, m = j % 3)
						{
						// include some tolerance
						float v = verts[i][m] + ((d != 0 ? 1.0f : -1.0f) * (Math.Abs(verts[i][m]) + 1.0f) * TOLERANCE);
						if (d == 0)
							bound[j] = Math.Min(v,bound[j]);
						else
							bound[j] = Math.Max(v,bound[j]);
						}
					}
				}
			return bound;
			}


		Vector edge1, edge2; // edge 0->1, 0->2
		/*
		 * @implementation
		 * Adapted from:
		 * <cite>'Fast, Minimum Storage Ray-Triangle Intersection'
		 * Moller, Trumbore;
		 * Journal Of Graphics Tools, v2n1p21, 1997.
		 * http://www.acm.org/jgt/papers/MollerTrumbore97/</cite>
		 */
		public bool GetIntersection(Vector rayOrigin, Vector rayDirection, ref float hitDistance)
			{
			// begin calculating determinant - also used to calculate U parameter
			Vector pvec = rayDirection.Cross(edge2);

			// if determinant is near zero, ray lies in plane of triangle
			float det = edge1.Dot(pvec);

			const float EPSILON = 0.000001f;
			if ((det > -EPSILON) && (det < EPSILON))
				return false;

			float inv_det = 1.0f / det;

			// calculate distance from vertex 0 to ray origin
			Vector tvec = rayOrigin - verts[0];

			// calculate U parameter and test bounds
			float u = tvec.Dot(pvec) * inv_det;
			if ((u < 0.0f) || (u > 1.0f))
				return false;

			// prepare to test V parameter
			Vector qvec = tvec.Cross(edge1);

			// calculate V parameter and test bounds
			float v = rayDirection.Dot(qvec) * inv_det;
			if ((v < 0.0f) || (u + v > 1.0f))
				return false;

			// calculate t, ray intersects triangle
			hitDistance = edge2.Dot(qvec) * inv_det;

			// only allow intersections in the forward ray direction
			return hitDistance >= 0.0f;
			}


		public Vector GetSamplePoint(Random random)
			{
			// get two randoms
			float sqr1 = (float)Math.Sqrt(random.NextDouble());
			float r2 = (float)random.NextDouble();

			// make barycentric coords
			float a = 1.0f - sqr1;
			float b = (1.0f - r2) * sqr1;			

			// make position from barycentrics
			// calculate interpolation by using two edges as axes scaled by the
			// barycentrics
			return edge1 * a + edge2 * b + verts[0];
			}


		/// <summary>
		/// Construct a new triangle from a stream
		/// </summary>
		/// <param name="infile"></param>
		public Triangle(StreamReader infile)
			{
			// read three geometry points
			verts[0] = new Vector(infile);
			verts[1] = new Vector(infile);
			verts[2] = new Vector(infile);

			// read and condition quality
			Reflectivity = new Vector(infile).Clamp(Vector.ZERO, Vector.ONE);

			Emitivity = new Vector(infile).Clamp(Vector.ZERO, Vector.MAX);

			// some item caching
			edge1 = verts[1] - verts[0];
			edge2 = verts[2] - verts[0];

			// make area, Tangent, normal, deltas
			MakeTangent();
			MakeNormal();
			MakeArea();
			}


		}
	}
