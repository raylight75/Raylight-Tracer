// Raylight C# port
// TihomirBlajev
// www.tihoblajev.com
using System;
using System.IO;

namespace RayLight
{
	class Program
		{

		static string MODEL_FORMAT_ID = "#RayLight";
		static double SAVE_PERIOD = 180.0; // seconds

		/// <summary>
		/// Program start. Takes one command line parameter to get a filename
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		static int Main(string[] args)
			{
			int returnValue = -1;

			// catch everything
			try
				{
				// check for help request
				if ((args.Length == 0) || (args[0] == "-?") || (args[0] == "--help"))
					{
					Console.WriteLine(BANNER_MESSAGE);					
					}

				else
					{ // execute
					int starttime, lastSaveTime;
					starttime = lastSaveTime = Environment.TickCount;

					bool showPNG = false; // default PPM
					if ((args.Length == 2) && (args[1].ToUpper() == "G"))
						showPNG = true;
					Console.WriteLine(BANNER_MESSAGE);

					// get file names
					string modelFilePathname = args[0];
					string imageFilePathname = Path.GetFileNameWithoutExtension(modelFilePathname);
					if (showPNG == true)
						imageFilePathname += ".png";
					else
						imageFilePathname += ".ppm";

					// open model file
					StreamReader modelFile = File.OpenText(modelFilePathname);

					// check model file format identifier at start of first line
					string formatId = modelFile.ReadLine();
					if (MODEL_FORMAT_ID != formatId)
						throw new Exception("Invalid model file");

					// read frame iterations
					int iterations = (int)modelFile.ReadFloat();

					// create top-level rendering objects with model file
					Image image = new Image(modelFile);
					Camera camera = new Camera(modelFile);
					Scene scene = new Scene(modelFile, camera.ViewPosition);

					modelFile.Close();
					Console.WriteLine("Rendering scene file " + modelFilePathname);
					Console.WriteLine("Output file will be " + imageFilePathname);						

					Random rand = new Random(); // todo - option to set seed?

					// do progressive refinement render loop
					for (int frameNo = 1; frameNo <= iterations; ++frameNo)
						{
						// render a frame
						camera.GetFrame(scene, rand, image);

						// display latest frame number
						Console.CursorLeft = 0;
						Console.Write("Iteration: {0} of {1}. Elapsed seconds {2}", frameNo, iterations, (Environment.TickCount-lastSaveTime)/1000);

						// save image every three minutes, and at end
						if ((frameNo == iterations) || (Environment.TickCount - lastSaveTime > SAVE_PERIOD * 1000))
							{
							lastSaveTime = Environment.TickCount;
							image.SaveImage(imageFilePathname, frameNo, showPNG);
							if (frameNo == iterations)
								Console.WriteLine("\nImage file {0} saved", imageFilePathname);
							}

						}

					Console.WriteLine("\nfinished in {0} secs",(Environment.TickCount - starttime)/1000.0);
					}

				returnValue = 1;
				}
			// print exception message
			catch (Exception e)
				{
				Console.WriteLine("\n*** execution failed:  " + e.Message);
				}
			return returnValue;
			}

		/// user messages --------------------------------------------------------------
		static string BANNER_MESSAGE =
		"\n----------------------------------------------------------------------\n" +
		"  RayLight v0.9 C# 3.5\n\n" +
		"  Copyright (c) 2018, Tihomir Blajev\n" +
		"  http://www.tihoblajev.com\n\n" +
		"  2018-12-12\n" +
		"----------------------------------------------------------------------";		
		}
	}
