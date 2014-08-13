using System;
using UnityEngine;
using System.Collections.Generic;

namespace Picus.Utils.GO
{
	public class Transformer // TODO: move from GO
	{
		public static bool HaveParent(UnityEngine.GameObject child, UnityEngine.GameObject parent)
		{
			UnityEngine.GameObject obj = child;
			while (obj != null)
			{
				if (obj == parent) return true;
				obj = obj.transform.parent == null ? null : obj.transform.parent.gameObject;
			}
			return false;
		}

		public static bool IsDefaultLocalTransform(Transform transform)
		{
			if (transform.localPosition != Vector3.zero)
				return false;
			if (transform.localScale != new Vector3(1, 1, 1))
				return false;
			return transform.rotation == Quaternion.identity;
		}
		
		public static void SetDefaultLocalTransform(Transform transform)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = new Vector3(1, 1, 1);
		}
		
		public static void TransformTo(Transform obj, Transform toObj, bool keepScale)
		{
			Transform parent = obj.parent;
			
			if (!keepScale)
				obj.parent = null;
			
			obj.position = toObj.position;
			obj.rotation = toObj.rotation;
			
			if (!keepScale)
			{
				obj.localScale = toObj.lossyScale;
				obj.parent = toObj;
				obj.parent = parent;
			}
		}
		
		public static void TransformTo(Transform obj, Transform toObj)
		{
			TransformTo(obj, toObj, false);
		}
		
		public static void TransformTo(UnityEngine.GameObject obj, UnityEngine.GameObject toObj)
		{
			TransformTo(obj.transform, toObj.transform);
		}
	}
}

