using System;
using System.IO;

namespace RayLight
{
	/// <summary>
	/// Standard 3D vector and operations
	/// </summary>
	class Vector
		{
		/// <summary>
		/// X,Y,Z storage
		/// </summary>
		float x,y,z;

		/// <summary>
		/// Default constructor is (0,0,0)
		/// </summary>
		public Vector()
			{
			x = y = z = 0;
			}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public Vector(Vector a)
			{
			x = a.x;
			y = a.y;
			z = a.z;
			}

		/// <summary>
		/// Constructor for (x,y,z)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public Vector(float x, float y,float z)
			{
			this.x = x;
			this.y = y;
			this.z = z;
			}

		public float Dot(Vector v)
			{
			return (x * v.x) + (y * v.y) + (z * v.z);
			}

		static public Vector operator -(Vector v)
			{
			return new Vector(-v.x, -v.y, -v.z);
			}

		public Vector Unitize()
			{
			float length = (float)Math.Sqrt((x * x) + (y * y) + (z * z));
			if (length == 0)
				return Vector.ZERO;
			float inv = 1.0f / length;

			return new Vector(x * inv,y * inv,z * inv);
			}


		public Vector Cross(Vector v)
			{
			return new Vector((y * v.z) - (z * v.y),
							 (z * v.x) - (x * v.z),
							 (x * v.y) - (y * v.x));
			}


		public static Vector operator +(Vector a, Vector b)
			{
			return new Vector(a.x + b.x,a.y + b.y,a.z + b.z);
			}


		public static Vector operator-(Vector a, Vector b)
			{
			return new Vector(a.x - b.x,a.y - b.y,a.z - b.z);
			}


		public static Vector operator*(Vector a, Vector b)
			{
			return new Vector(a.x * b.x,a.y * b.y,a.z * b.z);
			}


		public static Vector operator*(Vector a, float f)
			{
			return new Vector(a.x * f,a.y * f,a.z * f);
			}


		public static Vector operator/(Vector a, float f)
			{
			float inv = 1.0f / f;

			return new Vector(a.x * inv, a.y * inv,a.z * inv);
			}


		public bool IsZero()
			{
			return (x == 0.0f) && (y == 0.0f) && (z == 0.0f);
			}



		public Vector Clamp(Vector min, Vector max)
			{
			float xn = Math.Min(Math.Max(x, min.x),max.x);
			float yn = Math.Min(Math.Max(y, min.y),max.y);
			float zn = Math.Min(Math.Max(z, min.z),max.z);
			return new Vector(xn,yn,zn);
			}

		public float this[int index]
			{
			set {}
			get
				{
				switch (index)
					{
					case 0: return x;
					case 1: return y;
					case 2: return z;
					}
				return 0;
				}
			}


		/// constants ------------------------------------------------------------------
		static public Vector ZERO = new Vector(0.0f, 0.0f, 0.0f);
		static public Vector ONE = new Vector(1.0f, 1.0f, 1.0f);
		static public Vector MIN = new Vector(-float.MaxValue, -float.MaxValue, -float.MaxValue);
		static public Vector MAX = new Vector(float.MaxValue, float.MaxValue, float.MaxValue);

		public Vector(StreamReader infile)
			{
			x = infile.ReadFloat();
			y = infile.ReadFloat();
			z = infile.ReadFloat();
			//infile.Pass(')'); /// todo?
			}

		/// <summary>
		/// Format a vector as (x y z)
		/// </summary>
		/// <returns>vector formatted as string</returns>
		public override string ToString()
			{
			return String.Format("({0} {1} {2})", x, y, z);
			}
		}
	}
