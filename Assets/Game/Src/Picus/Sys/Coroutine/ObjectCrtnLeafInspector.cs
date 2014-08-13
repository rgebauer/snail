using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Picus.Sys.Coroutine
{
#if DEBUG && UNITY_EDITOR
	[System.Serializable] // serizalization depth limit is finite. for leafs do not show stack
	public class ObjectCrtnLeafInspector
	{
		public string Name;
//		public int Id;
		public GameObject GameObject;
		public string EnvironmentStack;

		public ObjectCrtnLeafInspector(ObjectCrtn objCrtn)
		{
			Name = objCrtn.Name;
//			Id = objCrtn.Id;
			GameObject = objCrtn.GameObject;
			EnvironmentStack = objCrtn.Stack;
		}

		public static implicit operator ObjectCrtnLeafInspector(ObjectCrtn objCrtn)
		{
			return new ObjectCrtnLeafInspector(objCrtn);
		}
	}
#endif
}