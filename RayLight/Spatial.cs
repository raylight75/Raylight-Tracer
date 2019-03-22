using System.Collections.Generic;
using System.Linq;
using System;

namespace RayLight
{
	class Spatial
		{

		/*
		 * A minimal spatial index for ray tracing.<br/><br/>
		 *
		 * Suitable for a scale of 1 metre == 1 numerical unit, and has a resolution of
		 * 1 millimetre. (Implementation uses fixed tolerances)
		 *
		 * Constant.<br/><br/>
		 *
		 * @implementation
		 * A degenerate State pattern: typed by isBranch_m field to be either a branch
		 * or leaf cell.<br/><br/>
		 *
		 * Octree: axis-aligned, cubical. Subcells are numbered thusly:
		 * <pre>      110---111
		 *            /|    /|
		 *         010---011 |
		 *    y z   | 100-|-101
		 *    |/    |/    | /
		 *    .-x  000---001      </pre><br/><br/>
		 *
		 * Each cell stores its bound: fatter data, but simpler code.<br/><br/>
		 *
		 * Calculations for building and tracing are absolute rather than incremental --
		 * so quite numerically solid. Uses tolerances in: bounding triangles (in
		 * Triangle::getBound), and checking intersection is inside cell (both effective
		 * for axis-aligned items). Also, depth is constrained to an absolute subcell
		 * size (easy way to handle overlapping items).
		 *
		 * @invariants
		 * * bound_m[0-2] <= bound_m[3-5]
		 * * bound_m encompasses the cell's contents
		 * if isBranch_m
		 * * vector_m length is 8
		 * * vector_m elements are zero or Spatial pointers
		 * else
		 * * vector_m elements are non-zero Triangle pointers
		 */

		bool isBranch;
		float[] bound = new float[6];
		Triangle[] triangles = null; // isBranch = false
		Spatial[] spatial = null;	 // isBranch = true

		// accommodates scene including sun and earth, down to cm cells (use 47 for mm)
		const int MAX_LEVELS = 44;
		const int MAX_ITEMS = 8;


		public Spatial(Vector eyePosition, List<Triangle> items)
			{
			// set overall bound
			// accommodate eye position
			// (makes tracing algorithm simpler)
			for (int i = 6; i-- > 0; bound[i] = eyePosition[i % 3]) ;

			// accommodate all items
			foreach (Triangle item in items)
				{
				float [] itemBound = item.GetBound();

				// accommodate item
				for (int j = 0; j < 6; ++j)
					{
					if ((bound[j] > itemBound[j]) ^ (j > 2))
						bound[j] = itemBound[j];
					}
				}

			// make cubical
			float maxSize = 0.0f;
			for (int i = 0; i < 3; ++i)
				maxSize = Math.Max(maxSize, bound[3 + i] - bound[i]);
			for (int i = 0; i < 3; ++i)
				bound[3 + i] = Math.Max(bound[3 + i], bound[i] + maxSize);

			// make cell tree
			Construct(items, 0);
			}


		Spatial(float[] bound)
			{
			for (int i = 0; i < 6; ++ i)
				this.bound[i] = bound[i];
			}

		public void GetIntersection(
			Vector rayOrigin, Vector rayDirection, Triangle lastHit, 
			out Triangle pHitObject, out Vector hitPosition, Vector pStart)
			{
			hitPosition = null;
			pHitObject = null;
			// is branch: step through subcells and recurse
			if (isBranch)
				{
				if (pStart == null)
					pStart = rayOrigin;

				// find which subcell holds ray origin (ray origin is inside cell)
				int subCell = 0;
				for (int i = 3; i-- > 0; )
					{
					// compare dimension with center
					if (pStart[i] >= ((bound[i] + bound[i + 3]) * 0.5f))
						subCell |= 1 << i;
					}

				// step through intersected subcells
				Vector cellPosition = new Vector(pStart);
				for (; ; )
					{
					if (null != spatial[subCell])
						{
						// intersect subcell
						spatial[subCell].GetIntersection(rayOrigin, rayDirection, lastHit, out pHitObject, out hitPosition, cellPosition);
						// exit if item hit
						if (null != pHitObject)
							break;
						}

					// find next subcell ray moves to
					// (by finding which face of the corner ahead is crossed first)
					int axis = 2;
					float[] step = new float[3]; // todo - move this allocation?
					for (int i = 3; i-- > 0; axis = step[i] < step[axis] ? i : axis)
						{
						bool high = ((subCell >> i) & 1) != 0;
						float face = (rayDirection[i] < 0.0f) ^ high ? bound[i + ((high ? 1 : 0) * 3)] : (bound[i] + bound[i + 3]) * 0.5f;
						// distance to face
						// (div by zero produces infinity, which is later discarded)
						step[i] = (face - rayOrigin[i]) / rayDirection[i];
						}

					// leaving branch if: subcell is low and direction is negative,
					// or subcell is high and direction is positive
					if ((((subCell >> axis) & 1) != 0) ^ (rayDirection[axis] < 0.0f))
						break;

					// move to (outer face of) next subcell
					cellPosition = rayOrigin + (rayDirection * step[axis]);
					subCell = subCell ^ (1 << axis);
					}
				}
			else
				{ // is leaf: exhaustively intersect contained items
				float nearestDistance = float.MaxValue;

				// step through items
				foreach (Triangle item in triangles)
					{
					// avoid false intersection with surface just come from
					if (item != lastHit)
						{
						// intersect ray with item, and inspect if nearest so far
						float distance = float.MaxValue;
						if (item.GetIntersection(rayOrigin, rayDirection, ref distance) && (distance < nearestDistance))
							{
							// check intersection is inside cell bound (with tolerance)
							Vector hit = rayOrigin + (rayDirection * distance);
							float t = Triangle.TOLERANCE;
							if ((bound[0] - hit[0] <= t) && (hit[0] - bound[3] <= t) &&
								(bound[1] - hit[1] <= t) && (hit[1] - bound[4] <= t) &&
								(bound[2] - hit[2] <= t) && (hit[2] - bound[5] <= t))
								{
								pHitObject = item;
								nearestDistance = distance;
								hitPosition = hit;
								}
							}
						}
					}
				}
			}




		Spatial Construct(List<Triangle> items, int level)
			{
			// is branch if items overflow leaf and tree not too deep
			isBranch = (items.Count > MAX_ITEMS) && (level < (MAX_LEVELS - 1));

			// be branch: make sub-cells, and recurse construction
			if (isBranch)
				{
				// make subcells
				spatial = new Spatial[8];

				for (int s = spatial.Length, q = 0; s-- > 0; )
					{
					// make subcell bound
					// collect items that overlap subcell
					float[] subBound = new float[6];
					List<Triangle> subItems = new List<Triangle>();
					for (int i = items.Count; i-- > 0; )
						{
						float[] itemBound = items[i].GetBound(); 

						bool isOverlap = true;
						// step through two coord sets
						// todo - also break on isOverlap == false
						for (int j = 0, d = 0, m = 0; j < 6; ++j, d = j / 3, m = j % 3)
							{
							// make subcell bound
							subBound[j] = (((s >> m) & 1) ^ d) == 1 ?
							   (bound[m] + bound[m + 3]) * 0.5f : bound[j];
							// must overlap in all dimensions
							isOverlap &= (itemBound[(d ^ 1) * 3 + m] >= subBound[j]) ^ (d != 0);
							if (isOverlap == false)
								break;
							}

						if (isOverlap) 
							subItems.Add(items[i]);
						}

					// curtail degenerate subdivision by adjusting next level
					// (degenerate if two or more subcells copy entire contents of parent,
					// or if subdivision reaches below mm size)
					// (having a model including the sun requires one subcell copying
					// entire contents of parent to be allowed)
					q += subItems.Count == items.Count ? 1 : 0;
					int nextLevel = (q > 1) || ((subBound[3] - subBound[0]) <
					   (Triangle.TOLERANCE * 4.0f)) ? MAX_LEVELS : level + 1;

					// recurse
					spatial[s] = !(subItems.Count == 0) ? (new Spatial(subBound)).Construct(subItems, nextLevel) : null;
					}
				}

			else // be leaf: store items // (trim reserve capacity, since vector_m was default-empty)
				triangles = items.ToArray<Triangle>();

			return this;
			}
		}
	}
