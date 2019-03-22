using System;
using System.Text;
using System.IO;

namespace RayLight
{
	public static class Utility
		{
		/// <summary>
		/// Read a single float from str, ignoring other characters
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static float ReadFloat(this StreamReader str)
			{
			char ch;
			// skip noninteresting characters
			do
				{
				ch = (char)str.Read();
				} while ((!Char.IsDigit(ch)) && (ch != '-') && (ch != '.') && (ch != '+') && (str.EndOfStream != true));
			StringBuilder sb = new StringBuilder();
			sb.Append(ch);
			// read interesting characters
			bool done = false;
			do
				{
				ch = (char)str.Read();
				done = !((Char.IsDigit(ch)) || (ch == '.'));
				done |= str.EndOfStream;
				if (!done)
					sb.Append(ch);
				} while (!done);
			float t;
			float.TryParse(sb.ToString(),out t);
			return t;
			}
		}
	}
