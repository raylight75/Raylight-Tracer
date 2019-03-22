
using System;
namespace RayLight
{
	class RayTracer
		{

		/*
		 * Ray tracer for general light transport.<br/><br/>
		 *
		 * Traces a path with emitter sampling each step: A single chain of ray-steps
		 * advances from the eye into the scene with one sampling of emitters at each
		 * node.<br/><br/>
		 *
		 * Constant.
		 *
		 * @invariants
		 * * pScene_m points to a Scene (is not 0)
		 */

		Scene scene;

		public RayTracer(Scene scene)
			{
			this.scene = scene;
			}

		public Vector GetRadiance(Vector rayOrigin, Vector rayDirection, Random random, Triangle lastHit)
			{
			// intersect ray with scene
			Triangle pHitObject;
			Vector hitPosition;
			scene.GetIntersection(rayOrigin, rayDirection, lastHit, out pHitObject, out hitPosition);

			Vector radiance;
			if (null != pHitObject)
				{
				// make surface point of intersection
				SurfacePoint surfacePoint = new SurfacePoint(pHitObject, hitPosition);

				// local emission only for first-hit
				if (lastHit != null)
					radiance = Vector.ZERO;
				else
					radiance = surfacePoint.GetEmission(rayOrigin, -rayDirection, false);

				// add emitter sample
				radiance = radiance + SampleEmitters(rayDirection, surfacePoint, random);

				// add recursive reflection
				//
				// single hemisphere sample, ideal diffuse BRDF:
				// reflected = (inradiance * pi) * (cos(in) / pi * color) * reflectance
				// -- reflectance magnitude is 'scaled' by the russian roulette, cos is
				// importance sampled (both done by SurfacePoint), and the pi and 1/pi
				// cancel out
				Vector nextDirection;
				Vector color;
				// check surface bounces ray, recurse
				if (surfacePoint.GetNextDirection(random, -rayDirection, out nextDirection, out color))
					radiance = radiance + (color * GetRadiance(surfacePoint.Position, nextDirection, random, surfacePoint.Item));
				}
			else // no hit: default/background scene emission
				radiance = scene.GetDefaultEmission(-rayDirection);

			return radiance;
			}

		Vector SampleEmitters(Vector rayDirection, SurfacePoint surfacePoint, Random random)
			{
			Vector radiance;

			// single emitter sample, ideal diffuse BRDF:
			// reflected = (emitivity * solidangle) * (emitterscount) *
			// (cos(emitdirection) / pi * reflectivity)
			// -- SurfacePoint does the first and last parts (in separate methods)

			// get position on an emitter
			Vector emitterPosition;
			Triangle emitter;
			scene.GetEmitter(random, out emitterPosition, out emitter);

			// check an emitter was found
			if (null != emitter)
				{
				// make direction to emit point
				Vector emitDirection = (emitterPosition - surfacePoint.Position).Unitize();

				// send shadow ray
				Triangle hitObject;
				Vector hitPosition;
				scene.GetIntersection(surfacePoint.Position, emitDirection, surfacePoint.Item, out hitObject, out hitPosition);

				// if unshadowed, get inward emission value
				Vector emissionIn = null;
				if ((null == hitObject) || (emitter == hitObject))
					emissionIn = new SurfacePoint(emitter, emitterPosition).GetEmission(surfacePoint.Position, -emitDirection, true);
				else
					emissionIn = new Vector();

				// get amount reflected by surface
				radiance = surfacePoint.GetReflection(emitDirection, emissionIn * scene.GetEmittersCount(), -rayDirection);
				}
			else 
				radiance = new Vector();

			return radiance;
			}
		}
	}
