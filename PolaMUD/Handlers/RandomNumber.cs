using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
	public static class RandomNumber
	{
		public static int Get(int min, int max)
		{
			Random random = new Random();
			return random.Next(min, max);
		}
	}
}
