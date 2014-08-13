using System;
using System.Collections;
using UnityEngine;

namespace Picus.Sys.Coroutine
{
	[System.Serializable]
	public class ObjectRetCrtn<T> : ObjectCrtn
	{
		public T Value { get; private set; }

		public ObjectRetCrtn(MethodOneParam<ObjectCrtn> onEnd, IEnumerator enumerator, MethodExceptionParam onException, GameObject obj, ObjectCrtn parent) : base(onEnd, enumerator, onException, obj, parent) { }

		protected override bool SolveYieldEnd(object yielded)
		{
			if (yielded != null && yielded is T)
			{
				Value = (T)yielded;
				return true;
			}
			return false;
		}
	}
}