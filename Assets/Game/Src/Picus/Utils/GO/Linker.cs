using System;
using UnityEngine;
using System.Collections.Generic;
using Picus.Extensions;

namespace Picus.Utils.GO
{
	public class Linker
	{
		public static void LinkTo(Transform obj, Transform toObj)
		{
			LinkTo(obj, toObj, true);
		}
		
		public static void LinkTo(Transform obj, Transform toObj, bool inheritRotation)
		{
			LinkTo(obj == null ? null : obj.gameObject, toObj == null ? null : toObj.gameObject, inheritRotation);
		}

		/// <summary>
		/// Links to toObj.
		/// </summary>
		/// <param name="destroyWithParent">If parent is destroyed and set to <c>false</c> it will unlinks and stay on last position.</param>
		public static void LinkTo(UnityEngine.GameObject obj, UnityEngine.GameObject toObj, bool inheritRotation = true, bool inheritSorting = true, bool destroyWithParent = false)
		{
			if (obj == null) 
				return;

			Finder.FindComponentAddIfNotExist<TopInfo>(obj);

			if (toObj == null)
			{
				TopInfo topInfo = TopInfo.GameObjectTop(obj, 2).GameObjectTopInfo<TopInfo>();

				obj.transform.parent = Sys.ResourceManager.Instance.RuntimeParent();
				if (topInfo) 
					topInfo.UnlinkOnDestroyRemove(obj);
			}
			else
			{
				if (!destroyWithParent)
				{
					TopInfo topInfo = toObj.GameObjectTopInfo<TopInfo>();

					if (topInfo) 
						topInfo.UnlinkOnDestroyAdd(obj);
				}

				obj.transform.position = toObj.transform.position;
				if (inheritRotation) 
					obj.transform.rotation = toObj.transform.rotation;
				obj.transform.parent = toObj.transform;
				if (inheritSorting)
					Finder.FindComponentAddIfNotExist<SortingLayer>(obj).SortAllDeepFromParent();
			}
		}

		public static void OnObjectWillBeDestroyed(UnityEngine.GameObject objToDestroy)
		{
			TopInfo parentTopInfo = TopInfo.GameObjectTop(objToDestroy, 2).GameObjectTopInfo<TopInfo>();

			if (parentTopInfo) parentTopInfo.UnlinkOnDestroyRemove(objToDestroy);

			TopInfo topInfo = objToDestroy.GameObjectTopInfo<TopInfo>();
			if (topInfo == null) return;

			List<UnityEngine.GameObject> list = topInfo.UnlinkOnDestroy;

			foreach(UnityEngine.GameObject linkedObj in list)
			{
#if DEBUG
				Debug.Assert(linkedObj != null && linkedObj.GetComponent<TopInfo>() != null, "OnObjectWillBeDestroyed linked object " + linkedObj + " without topinfo.");
#endif
				LinkTo(linkedObj, null);
			}
		}
	}
}

