//#define PARALLEL // define to make a parallel renderer here and in Camera
using System;
using System.IO;
using System.Drawing;

namespace RayLight
	{
	class Image
		{

		/*
		 * Pixel sheet with simple tone-mapping and file formatting.<br/><br/>
		 *
		 * Uses PPM image format:
		 * <cite>http://netpbm.sourceforge.net/doc/ppm.html</cite><br/><br/>
		 *
		 * Uses Ward simple tonemapper:
		 * <cite>'A Contrast Based Scalefactor For Luminance Display'
		 * Ward;
		 * Graphics Gems 4, AP 1994.</cite><br/><br/>
		 *
		 * Uses RGBE image format:
		 * <cite>http://radsite.lbl.gov/radiance/refer/filefmts.pdf</cite>
		 * <cite>'Real Pixels' Ward; Graphics Gems 2, AP 1991;</cite><br/><br/>
		 *
		 * @invariants
		 * * width_m  >= 1 and <= 10000
		 * * height_m >= 1 and <= 10000
		 * * pixels_m.size() == (width_m * height_m)
		 */

		public int Width { get; set; }
		public int Height { get; set; }

		Vector[,] pixels;

		// format items
		const string PPM_ID = "P6";
		const string MINILIGHT_URI = "http://www.hxa7241.org/minilight/";
		const string LOMONT_URI = "http://www.lomont.org";

		// guess of average screen maximum brightness
		static float DISPLAY_LUMINANCE_MAX = 200.0f;

		// ITU-R BT.709 standard RGB luminance weighting
		static Vector RGB_LUMINANCE = new Vector(0.2126f, 0.7152f, 0.0722f);

		// ITU-R BT.709 standard gamma
		static float GAMMA_ENCODE = 0.45f;
		
#if PARALLEL
		object locker = new object();
#endif

		/// <summary>
		/// Create image based on size from stream
		/// </summary>
		/// <param name="infile"></param>
		public Image(StreamReader infile)
			{
			// read width and height
			Width = (int)infile.ReadFloat();
			Height = (int)infile.ReadFloat();

			// clamp width and height
			Width = Width < 1 ? 1 : (Width > 10000 ? 10000 : Width);
			Height = Height < 1 ? 1 : (Height > 10000 ? 10000 : Height);

			pixels = new Vector[Width, Height];
			for (int i = 0; i < Width; ++i)
				for (int j = 0; j < Height; ++j)
					pixels[i, j] = new Vector();
			}

		public void AddToPixel(int x, int y, Vector radiance)
			{
#if PARALLEL
			lock (locker)
#endif
			pixels[x, Height - 1 - y] += radiance;
			}

		/// <summary>
		/// Get RGB byte array of image data
		/// </summary>
		/// <param name="iteration"></param>
		/// <returns></returns>
		public byte[, ,] GetImageBytes(int iteration)
			{
			byte[, ,] data = new byte[Width, Height, 3]; // RGB buffer of bytes

			// make pixel value accumulation divider
			float divider = 1.0f / (float)((iteration > 0 ? iteration : 0) + 1);

			float tonemapScaling = CalculateToneMapping(pixels, divider);

			// write pixels
			for (int j = 0; j < Height; ++j)
				for (int i = 0; i < Width; ++i)
					{
					for (int c = 0; c < 3; ++c)
						{
						// tonemap
						float mapped = pixels[i, j][c] * divider * tonemapScaling;

						// gamma encode
						mapped = (float)Math.Pow((mapped > 0.0f ? mapped : 0.0f), GAMMA_ENCODE);

						// quantize
						mapped = (float)Math.Floor((mapped * 255.0f) + 0.5f);

						// store as byte
						data[i, j, c] = (byte)(mapped <= 255.0f ? mapped : 255.0f);
						}
					}
			return data;
			}

		public void SavePPM(string filename, int frame)
			{
			byte[, ,] data = GetImageBytes(frame);

			// get file
			using (StreamWriter outfile = File.CreateText(filename))
				{
				// write header
				// write ID and comment
				outfile.WriteLine(PPM_ID + "\n# " + LOMONT_URI + "\n");
				outfile.WriteLine(PPM_ID + "\n# " + MINILIGHT_URI + "\n");

				// write width, height, maxval
				outfile.WriteLine(Width + " " + Height + "\n255\n");

				// write pixels
				for (int j = 0; j < Height; ++j)
					for (int i = 0; i < Width; ++i)
						{
						for (int c = 0; c < 3; ++c)
							outfile.Write(data[i, j, c] + " ");
						outfile.WriteLine();
						}
				}
			} // SavePPM

		public void SavePNG(string filename, int iteration)
			{
			byte[,,] data = GetImageBytes(iteration);

			Bitmap bmp = new Bitmap(Width, Height);

			// write pixels
			for (int j = 0; j < Height; ++j)
				for (int i = 0; i < Width; ++i)
					bmp.SetPixel(i, j, Color.FromArgb(data[i, j, 0], data[i, j, 1], data[i, j, 2]));
			bmp.Save(filename);
			}


		public void SaveImage(string filename, int frame, bool showPNG)
			{
			if (showPNG)
				SavePNG(filename, frame);
			else
				SavePPM(filename, frame);
			}

		float CalculateToneMapping(Vector[,] pixels, float divider)
			{
			// calculate log mean luminance
			float logMeanLuminance = 1e-4f;

			float sumOfLogs = 0.0f;
			foreach (Vector p in pixels)
				{
				float Y = p.Dot(RGB_LUMINANCE) * divider;
				sumOfLogs += (float)Math.Log10(Y > 1e-4f ? Y : 1e-4f);
				}

			logMeanLuminance = (float)Math.Pow(10.0f, sumOfLogs / pixels.Length);

			// (what do these mean again? (must check the tech paper...))
			float a = 1.219f + (float)Math.Pow(DISPLAY_LUMINANCE_MAX * 0.25f, 0.4f);
			float b = 1.219f + (float)Math.Pow(logMeanLuminance, 0.4f);

			return (float)Math.Pow(a / b, 2.5f) / DISPLAY_LUMINANCE_MAX;
			}
		}
	}
