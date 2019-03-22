using System.Collections.Generic;
using System.IO;
using System;

namespace RayLight
{
	class Scene
		{

		/*
		 * A grouping of the objects in the environment.<br/><br/>
		 *
		 * Makes a sub-grouping of emitting objects.<br/><br/>
		 *
		 * Constant.
		 *
		 * @invariants
		 * * triangles_m.size() <= 2^20
		 * * emitters_m.size()  <= 2^16
		 * * pIndex_m is a non-zero pointer to a Spatial
		 * * skyEmission_m      >= 0
		 * * groundReflection_m >= 0
		 */
		List<Triangle> triangles;
		List<Triangle> emitters;
		Spatial octtree;

		Vector skyEmission;
		Vector groundReflection;

		// 2^20 ~= a million
		const int MAX_TRIANGLES = 0x100000;
		const int MAX_EMITTERS_P = 16;


		/// standard object services ---------------------------------------------------
		public Scene(StreamReader infile, Vector eyePosition)
			{
			// read and condition default sky and ground values
			skyEmission = new Vector(infile);
			groundReflection = new Vector(infile);

			skyEmission = skyEmission.Clamp(Vector.ZERO, skyEmission);
			groundReflection = skyEmission * groundReflection.Clamp(Vector.ZERO, Vector.ONE);
			triangles = new List<Triangle>();
			emitters = new List<Triangle>();

			// read objects
			while (infile.EndOfStream == false)
				{
				Triangle t = new Triangle(infile);
				if (t.Area != 0)
					triangles.Add(t);
				}

			// find emitting triangles
			foreach (Triangle t in triangles)
				{
				if (!t.Emitivity.IsZero() && (t.Area > 0.0f))
					{
					emitters.Add(t);
					if (emitters.Count >= (1 << MAX_EMITTERS_P))
						break;
					}
				}

			// make index
			octtree = new Spatial(eyePosition, triangles);
			}

		public void GetIntersection(
		   Vector rayOrigin, Vector rayDirection, Triangle lastHit,
		   out Triangle pHitObject, out Vector hitPosition)
			{
			octtree.GetIntersection(rayOrigin, rayDirection, lastHit, out pHitObject, out hitPosition, null);
			}


		public void GetEmitter(Random random, out Vector position, out Triangle triangle)
			{
			if (emitters.Count != 0)
				{
				// select emitter
				// not using lower bits, by treating the random as fixed-point i.f bits
				int index = (int)((((random.Next()) & ((1 << MAX_EMITTERS_P) - 1)) * emitters.Count) >> MAX_EMITTERS_P);

				// get position on triangle
				position = emitters[index].GetSamplePoint(random);
				triangle = emitters[index];
				}
			else
				{
				position = Vector.ZERO;
				triangle = null;
				}
			}


		public int GetEmittersCount()
			{
			return emitters.Count;
			}

		public Vector GetDefaultEmission(Vector backDirection)
			{
			// sky for downward ray, ground for upward ray
			if (backDirection[1] < 0)
				return skyEmission;
			return groundReflection;
			}

		}
	}
