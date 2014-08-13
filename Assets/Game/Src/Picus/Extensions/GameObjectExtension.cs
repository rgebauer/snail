using UnityEngine;
using System.Collections;
using Picus.Utils;

namespace Picus.Extensions
{
	public static class GameObjectExtension
	{
		public static T GameObjectTopInfo<T>(this UnityEngine.GameObject obj) where T: Picus.Utils.GO.TopInfo
		{
			GameObject topObj = Utils.GO.TopInfo.GameObjectTop(obj);
			return topObj == null ? null : topObj.GetComponent<T>();
		}

		public static UnityEngine.GameObject GameObjectTop(this UnityEngine.GameObject obj)
		{
			return Utils.GO.TopInfo.GameObjectTop(obj);
		}

		public static void SetTransformTo(this UnityEngine.GameObject obj, Transform transform) // keep old parent. new scale will be applied
		{
			Utils.GO.Transformer.TransformTo(obj.transform, transform);
		}

		public static void SetTransformTo(this UnityEngine.GameObject obj, Transform transform, bool keepScale) // keep old parent
		{
			Utils.GO.Transformer.TransformTo(obj.transform, transform, keepScale);
		}

		/// <summary>
		/// Links to object to toObj.
		/// </summary>
		/// <param name="toObj">To object.</param>
		/// <param name="destroyWithParent">If parent is destroyed and set to <c>false</c> it will unlinks and stay on last position.</param>
		public static void LinkTo(this UnityEngine.GameObject obj, UnityEngine.GameObject toObj, bool destroyWithParent = false)
		{
			Utils.GO.Linker.LinkTo(obj, toObj, true, true, destroyWithParent);
		}

		public static void StartForkCoroutine(this UnityEngine.GameObject obj, IEnumerator coroutine)
		{
			obj.GetComponent<Picus.MonoBehaviourExtend>().StartForkCoroutine(coroutine);
		}
	}
}