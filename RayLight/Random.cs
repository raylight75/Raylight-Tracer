
namespace MinLight
	{
	class Randomizer // todo - replace?
		{
		/*
		 * Simple, fast, good random number generator (Multiply-with-carry).<br/><br/>
		 *
		 * Perhaps the fastest of any generator that passes the Diehard tests.<br/><br/>
		 *
		 * Constant (sort-of: internally/non-semantically modifying).
		 *
		 * @implementation
		 * Concatenation of following two 16-bit multiply-with-carry generators
		 * x(n)=a*x(n-1)+carry mod 2^16 and y(n)=b*y(n-1)+carry mod 2^16, number and
		 * carry packed within the same 32 bit integer. Algorithm recommended by
		 * Marsaglia. Copyright (c) 2005, Glenn Rhoads.<br/><br/>
		 *
		 * <cite>http://web.archive.org/web/20050213041650/http://
		 * paul.rutgers.edu/~rhoads/Code/code.html</cite>
		 */
		uint[] seeds = new uint[2];

		// default seeds
		static uint[] SEEDS = { 521288629, 362436069 };


		public Randomizer()
			{
			uint seed = 0;
			seeds[0] = (0 != seed) ? seed : SEEDS[0];
			seeds[1] = (0 != seed) ? seed : SEEDS[1];
			}

		public uint GetUInt32()
			{
			// Use any pair of non-equal numbers from this list for the two constants:
			// 18000 18030 18273 18513 18879 19074 19098 19164 19215 19584
			// 19599 19950 20088 20508 20544 20664 20814 20970 21153 21243
			// 21423 21723 21954 22125 22188 22293 22860 22938 22965 22974
			// 23109 23124 23163 23208 23508 23520 23553 23658 23865 24114
			// 24219 24660 24699 24864 24948 25023 25308 25443 26004 26088
			// 26154 26550 26679 26838 27183 27258 27753 27795 27810 27834
			// 27960 28320 28380 28689 28710 28794 28854 28959 28980 29013
			// 29379 29889 30135 30345 30459 30714 30903 30963 31059 31083

			seeds[0] = 18000u * (seeds[0] & 0xFFFFu) + (seeds[0] >> 16);
			seeds[1] = 30903u * (seeds[1] & 0xFFFFu) + (seeds[1] >> 16);

			return ((seeds[0] << 16) + (seeds[1] & 0xFFFFu));
			}


		public float GetFloat()
			{
			return (float)(GetUInt32()) / 4294967296.0f;
			}

		}
	}
