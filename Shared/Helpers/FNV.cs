using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Helpers
{
    public class FNV
    {
		private const ulong fnvPrime = unchecked(1099511628211);
		private const ulong fnvOffsetBasis = unchecked(14695981039346656037);

		static public long Hash64(string key)
		{
			ulong h = fnvOffsetBasis;
			if (string.IsNullOrEmpty(key)) return unchecked((long)h);
			var arr = key.ToCharArray();
			for (var i = 0; i < arr.Length; i++)
			{
				unchecked
				{
					h ^= arr[i];
					h *= fnvPrime;
				}
			}

			return unchecked((long)h);
		}

	}
}
