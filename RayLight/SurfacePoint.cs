using System;

namespace RayLight
{
	/*
	 * Surface point at a ray-object intersection.<br/><br/>
	 *
	 * All direction parameters are away from surface.<br/><br/>
	 *
	 * Constant.<br/><br/>
	  *
	 * @invariants
	 * * triangle points to a Triangle (is not null)
	*/
	class SurfacePoint
		{

		public Triangle Item { get; set; }
		public Vector Position { get; set; }

		public SurfacePoint(Triangle triangle, Vector position)
			{
			this.Item = triangle;
			this.Position = position;
			}

		public Vector GetEmission(Vector toPosition, Vector outDirection, bool isSolidAngle)
			{
			Vector ray = toPosition - Position;
			float distance2 = ray.Dot(ray);
			float cosArea = outDirection.Dot(Item.Normal) * Item.Area;

			// clamp-out infinity
			float solidAngle = 1;
			if (isSolidAngle)
				{
				if (distance2 < 1e-6f)
					distance2 = 1e-6f;
				solidAngle = cosArea / distance2;
				}

			// emit from front face of surface only
			if (cosArea > 0)
				return Item.Emitivity * solidAngle;
			return Vector.ZERO;
			}


		public Vector GetReflection(Vector inDirection, Vector inRadiance, Vector outDirection)
			{
			float inDot = inDirection.Dot(Item.Normal);
			float outDot = outDirection.Dot(Item.Normal);

			// directions must be on same side of surface
			if ((inDot < 0.0f) ^ (outDot < 0.0f))
				return Vector.ZERO;
			// ideal diffuse BRDF:
			// radiance scaled by cosine, 1/pi, and reflectivity
			return (inRadiance * Item.Reflectivity) * (float)(Math.Abs(inDot / Math.PI));
			}


		public bool GetNextDirection(Random random, Vector inDirection, out Vector outDirection, out Vector color)
			{

			float reflectivityMean = Item.Reflectivity.Dot(Vector.ONE) / 3.0f;

			// russian-roulette for reflectance magnitude
			if (random.NextDouble() < reflectivityMean)
				{
				color = Item.Reflectivity / reflectivityMean;

				// cosine-weighted importance sample hemisphere

				double _2pr1 = Math.PI * 2.0f * random.NextDouble();
				double sr2 = Math.Sqrt(random.NextDouble());

				// make coord frame coefficients (z in normal direction)
				float x = (float)(Math.Cos(_2pr1) * sr2);
				float y = (float)(Math.Sin(_2pr1) * sr2);
				float z = (float)(Math.Sqrt(1.0 - (sr2 * sr2)));

				// make coord frame
				Vector normal = Item.Normal;
				Vector tangent = Item.Tangent;
				if (normal.Dot(inDirection) < 0.0f)
					normal = -normal;

				// make vector from frame times coefficients
				outDirection = tangent * x + normal.Cross(tangent) * y + normal * z;
				}
			else
				{
				color = new Vector(0, 0, 0);
				outDirection = Vector.ZERO;
				}

			return !outDirection.IsZero();
			}

		}
	}
