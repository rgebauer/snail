using UnityEngine;
using System.Collections;

namespace Picus.Utils.GO
{
	public class Duration : Picus.MonoBehaviourExtend
	{		
		public float DestroyAfter = 10;
		public float PeekTime = 0;

		public void SetDuration(float duration)
		{
			if (duration == -1) // default
				return;
			else
				DestroyAfter = duration;
		}

		protected void Start()
		{
			if (DestroyAfter != 0)
				Destroy(gameObject, DestroyAfter);
		}
	}
}