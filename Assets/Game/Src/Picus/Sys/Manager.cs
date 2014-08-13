using UnityEngine;
using System.Collections.Generic;

namespace Picus.Sys
{
	public abstract class Manager : Picus.MonoBehaviourExtend
	{
		private static List<Manager> _managers;

		/// <summary>
		/// Create new manager on its own new gameObject and link it to parentObj.
		/// </summary>
		/// <param name="parentObj">Owners gameObject.</param>
		/// <typeparam name="T">Manager type.</typeparam>
		public static T Create<T>(GameObject parentObj) where T : Manager
		{
			string childName = typeof(T).ToString() + "<manager>";
			Transform childTrans = parentObj.transform.Find(childName);

			Debug.Assert(childTrans == null, "Manager.Create " + childName + " already exist on " + parentObj, true);

			if (!childTrans)
			{
				GameObject child = new GameObject(childName);
				child.transform.parent = parentObj.transform;
				T comp = child.AddComponent<T>();
				DontDestroyOnLoad(child);
				return comp;
			}
			return childTrans.GetComponent<T>();
		}
		
		public void Delete()
		{
			DestroyImmediate(gameObject);
		}
		
		public abstract void OnSceneChanged();
	}
}