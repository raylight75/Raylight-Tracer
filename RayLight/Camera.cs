//#define PARALLEL // define to make a parallel renderer here and in Image

using System;
using System.IO;

namespace RayLight
{
	class Camera
		{

		// todo - comment all

		/*
		 * A View with rasterization capability.<br/><br/>
		 *
		 * getFrame() accumulates a frame to the image.<br/><br/>
		 *
		 * Constant.
		 *
		 * @invariants
		 * * viewAngle_m is >= 10 and <= 160 degrees in radians
		 * * viewDirection_m is unitized
		 * * right_m is unitized
		 * * up_m is unitized
		 * * above three form a coordinate frame
		 */


		public Vector ViewPosition { get; set; }
		float viewAngle;

		// view frame
		Vector viewDirection;
		Vector right;
		Vector up;


		/// standard object services ---------------------------------------------------
		public Camera(StreamReader infile)
			{
			// read and condition view definition

			ViewPosition = new Vector(infile);
			viewDirection = new Vector(infile);
			viewAngle = infile.ReadFloat();

			viewDirection = viewDirection.Unitize();
			if (viewDirection.IsZero())
				viewDirection = new Vector(0.0f, 0.0f, 1.0f);

			if (viewAngle < 10) viewAngle = 10;
			if (viewAngle > 160) viewAngle = 160;
			viewAngle *= (float)(Math.PI / 180);

			// make other directions of frame
			up = new Vector(0.0f, 1.0f, 0.0f);
			right = up.Cross(viewDirection).Unitize();

			if (!right.IsZero())
				up = viewDirection.Cross(right).Unitize();
			else
				{
				up = new Vector(0.0f, 0.0f, viewDirection[1] < 0.0f ? 1.0f : -1.0f);
				right = up.Cross(viewDirection).Unitize();
				}
			} // Camera


		public void GetFrame(Scene scene, Random randomIn, Image image)
			{
			RayTracer rayTracer = new RayTracer(scene);

			int width = image.Width;
			int height = image.Height;
			float halfAngle = (float)Math.Tan(viewAngle * 0.5f);

			// do image sampling pixel loop
#if PARALLEL
			Parallel.For(0, height, delegate(int y)
				{
					Random random = new Random(randomIn.Next());
#else
			for (int y = 0; y < height; ++y)
				  {
					Random random = randomIn;
#endif
					  for (int x = 0; x < width; ++x)
						  {
						  // make image plane displacement vector coefficients
						  float xF = (float)((x + random.NextDouble()) * 2.0f / width) - 1.0f;
						  float yF = (float)((y + random.NextDouble()) * 2.0f / height) - 1.0f;

						  // make image plane offset vector
						  Vector offset = (right * xF) + (up * yF * (height / width));

						  // make sample ray direction, stratified by pixels
						  Vector sampleDirection = (viewDirection + offset * halfAngle).Unitize();

						  // get radiance from RayTracer
						  Vector radiance = rayTracer.GetRadiance(ViewPosition, sampleDirection, random, null);

						  // add radiance to image
						  image.AddToPixel(x, y, radiance);
						  }
				  }
#if PARALLEL
);
#endif

			}
		}
	}
