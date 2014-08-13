using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Picus.Utils
{
	public class Containers
	{
		public static int Length<T>(T array) where T : IList
		{
			if (array == null)
				return 0;

			return array.Count;
		}

		public static bool IsNullOrEmpty<T>(T array) where T : IList
		{
			if (array == null)
				return true;
			
			return array.Count == 0;
		}

		public static TR First<TR, T>(T array) where T : IList
		{
			if (Length(array) == 0)
				return default(TR);

			return (TR) array[0];
		}

		public static TR Last<TR, T>(T array) where T : IList
		{
			int length = Length(array);

			if (length == 0)
				return default(TR);
			
			return (TR) array[length - 1];
		}

		public static TR AtOrLast<TR, T>(T array, int at) where T : IList
		{
			int length = Length(array);
			
			if (length == 0)
				return default(TR);

			if (at >= length)
				at = length - 1;
			return (TR) array[at];
		}

		public static TR Random<TR, T>(T array) where T : IList
		{
			if(array == null || array.Count == 0)
				return default(TR);
			
			return (TR) array[UnityEngine.Random.Range(0, array.Count)];
		}
	}
}

