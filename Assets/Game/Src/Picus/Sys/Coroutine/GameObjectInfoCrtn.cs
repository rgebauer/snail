using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Picus.Sys.Coroutine
{
	public class GameObjectInfoCrtn : Picus.MonoBehaviourExtend
	{
		[SerializeField]
		private List<ObjectCrtn> _coroutines = new List<ObjectCrtn>();

		public void Remove(ObjectCrtn objectCoroutine)
		{
			Debug.Log("Removing coroutineObject " + objectCoroutine + " from " + gameObject);
			bool removed = _coroutines.Remove(objectCoroutine);

			Debug.Assert(removed, "CoroutineGameObject " + gameObject + " trying to remove not existing coroutine " + objectCoroutine, true);

			if (objectCoroutine.ToString().Contains("CallAfterTime")) 
				Picus.Sys.Debug.Empty();

			if (removed)
				return;

			for (int i = 0, cnt = _coroutines.Count; i < cnt; ++i)
			{
				if (_coroutines[i].Equals(objectCoroutine))
				{
					_coroutines.RemoveAt(i);
					break;
				}
			}
		}

		public void Add(ObjectCrtn objectCoroutine)
		{
			Debug.Log("Adding coroutineObject " + objectCoroutine + " to " + gameObject);

			_coroutines.Add(objectCoroutine);
		}
	}
}