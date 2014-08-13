using System;
using UnityEngine;
using System.Collections.Generic;

namespace Picus.Utils.GO
{
	public class Finder
	{
		// TODO: GS cache with gameobjecttopinfo
		public static bool HasColliderDeep(UnityEngine.GameObject obj, bool collider3d)
		{
			Component[] components = null;
			if (collider3d)
				components = obj.GetComponentsInChildren<Collider>(true);
			else
				components = obj.GetComponentsInChildren<Collider2D>(true);

			return components.Length != 0;
		}

		public static UnityEngine.GameObject FindChildDeep(UnityEngine.GameObject obj, string childName)
		{
			if (!obj) return null;

			Transform ret = FindChildDeep(obj.transform, childName);

			return ret == null ? null : ret.gameObject;
		}

		public static bool FindChildDeep(UnityEngine.GameObject obj, UnityEngine.GameObject lookingForObj)
		{
			if (obj == lookingForObj) return true;
			
			int count = obj.transform.childCount;
			for(int i = 0; i < count; i++)
			{
				UnityEngine.GameObject child = obj.transform.GetChild(i).gameObject;
				bool finded = FindChildDeep(child, lookingForObj);
				if (finded) return finded;
			}
			
			return false;
		}

		public static Transform FindChildDeep(Transform obj, string childName)
		{
			if (obj.gameObject.name == childName) return obj;

			int count = obj.transform.childCount;
			for(int i = 0; i < count; i++)
			{
				Transform child = obj.transform.GetChild(i);
				Transform finded = FindChildDeep(child, childName);
				if (finded != null) return finded;
			}

			return null;
		}

		public static void FindChildsDeep(Transform obj, string childName, ref List<Transform> findedChilds)
		{
			if (obj.gameObject.name == childName) 
				findedChilds.Add(obj.transform);
			
			int count = obj.transform.childCount;
			for(int i = 0; i < count; i++)
			{
				Transform child = obj.transform.GetChild(i);
				FindChildsDeep(child, childName, ref findedChilds);
			}
		}

		public static T FindComponentAddIfNotExist<T>(UnityEngine.GameObject obj) where T : UnityEngine.Component
		{
			System.Type type = (typeof(T));

			UnityEngine.Component oldComp = obj.GetComponent(type);
			if (oldComp == null)
				oldComp = obj.AddComponent(type);

			return (T)oldComp;
//			return (T) Convert.ChangeType(oldComp, type);
		}

		public static bool RemoveComponentIfExist<T>(UnityEngine.GameObject obj)
		{
			System.Type type = typeof(T);
			UnityEngine.Component oldComp = obj.GetComponent(type);
			if (oldComp != null) 
				UnityEngine.GameObject.Destroy(oldComp);
			return oldComp != null;
		}

		public static UnityEngine.Component FindNearestComponent(Transform tr, bool inParents, System.Type type)
		{
			if (tr == null)
				return null;
			
			UnityEngine.Component comp = tr.GetComponent(type);
			if (comp != null)
				return comp;
			
			if (inParents)
			{
				UnityEngine.Component ret = FindNearestComponent(tr.parent, inParents, type);
				if (ret != null)
					return ret;
			}
			else // in children
			{
				int count = tr.childCount;
				for(int i = 0 ; i < count; i++)
				{
					Transform child = tr.GetChild(i);
					UnityEngine.Component ret = FindNearestComponent(child, inParents, type);
					if (ret != null)
						return ret;
				}
			}
			
			return null;
		}

		public static T FindNearestComponent<T>(Transform tr, bool inParents) where T : UnityEngine.Component // in parents or in children
		{
			UnityEngine.Component comp = FindNearestComponent(tr, inParents, typeof(T));

			return (T)comp;

//			if (comp != null)
//				return (T) Convert.ChangeType(comp, typeof(T));

//			return null;
		}
/*
		public static void DisableAllChilds(UnityEngine.Transform obj)
		{
			int count = obj.childCount;
			for(int i = 0; i < count; i++)
			{
				obj.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
*/		
	}
}

