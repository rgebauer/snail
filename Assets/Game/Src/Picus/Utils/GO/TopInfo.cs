using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Picus.Utils.GO
{
	/// <summary>
	/// Object used for identification prefab top gameObject and for caching access to prefab components.
	/// If You use Utils.LinkTo and later destroy parent prefab, this script is used to identify linked child prefab and prevent it from destroying.
	/// </summary>
	public class TopInfo : Picus.MonoBehaviourExtend 
	{
		private List<UnityEngine.GameObject> _unlinkOnDestroy = new List<UnityEngine.GameObject>();

		public List<UnityEngine.GameObject> UnlinkOnDestroy { get { return new List<UnityEngine.GameObject>(_unlinkOnDestroy); } }

		public UnityEngine.Transform Transform { get; private set; }	

		public static UnityEngine.GameObject GameObjectTop(UnityEngine.GameObject obj, int level)
		{
			if (obj == null) return null;
			
			Transform tr = obj.transform;
			
			int finded = 0;
			while (tr != null)
			{
				TopInfo info = tr.GetComponent<TopInfo>();
				
				if (info != null)
				{
					finded++;
					if (finded == level) return tr.gameObject;
				}
				tr = tr.parent;
			}
			
			//			Picus.Sys.Debug.Throw("GameObjectTop(" + level + ") asked on " + obj + " but found only on " + finded, true);
			return null;
		}
		
		public static UnityEngine.GameObject GameObjectTop(UnityEngine.GameObject obj)
		{
			return GameObjectTop(obj, 1);
		}
		
		public static TopInfo GameObjectTopInfo(UnityEngine.GameObject obj)
		{
			return GameObjectTop(obj).GetComponent<TopInfo>();
		}

		public static void RemoveTopInfo(UnityEngine.GameObject obj)
		{
			UnityEngine.Component oldComp = obj.GetComponent<TopInfo>();
			if (oldComp != null) 
			{
				Linker.LinkTo(obj, null);
				UnityEngine.GameObject.Destroy(oldComp);
			}
		}

		public void UnlinkOnDestroyAdd(UnityEngine.GameObject obj)
		{
			#if DEBUG
			string[] containing = _unlinkOnDestroy.Select(x => x.ToString()).ToArray();
			Debug.Log("GameObjectTopInfo.UnlinkOnDestroyAdd " + obj + " id " + obj.GetInstanceID()+ " to " + gameObject + " id " + gameObject.GetInstanceID() + " containing: " + string.Join(",", containing));
			#endif
			if (_unlinkOnDestroy.Contains(obj))
			{
				Picus.Sys.Debug.Throw("GameObjectTopInfo.UnlinkToDestroyAdd " + obj + " already added", true);
				return;
			}
			_unlinkOnDestroy.Add(obj);
		}
		
		public void UnlinkOnDestroyRemove(UnityEngine.GameObject obj)
		{
			#if DEBUG
			string[] containing = _unlinkOnDestroy.Select(x => x.ToString()).ToArray();
			Debug.Log("GameObjectTopInfo.UnlinkOnDestroyRemove " + obj + " id " + obj.GetInstanceID() + " from " + gameObject + " id " + gameObject.GetInstanceID() + " containing: " + string.Join(",", containing));
			#endif
			if (!_unlinkOnDestroy.Contains(obj))
			{
				//				Picus.Sys.Debug.Throw("GameObjectTopInfo.UnlinkToDestroyRemove " + obj + " not in", true);
				return;
			}
			_unlinkOnDestroy.Remove(obj);
		}

		public virtual void Invalidate()
		{
		}

		/* protected */
		protected virtual void Awake()
		{
			Transform = transform;
		}
	}
}
