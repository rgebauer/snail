using UnityEngine;
using Picus.Sys.Coroutine;

namespace Picus.Sys
{
	public class ResourceManager : Manager
	{
		// TODO: GS make Manager child of Singleton<> and create instance on access (after GL)
		public static ResourceManager Instance { get; private set; }

		private Transform _runtimeParent;

		public new UnityEngine.GameObject Instantiate(string resourceName)
		{
			GameObject prefab = Resources.Load<GameObject>(resourceName);

#if DEBUG
			// TODO: GS check findMissing (now in editor project)
#endif		
			return Instantiate(prefab);
		}

		public T Load<T>(string resourceName) where T : Object
		{
			return Resources.Load<T>(resourceName);
		}

		public override void OnSceneChanged() {}

		public GameObject CreateNew(string name)
		{
			Debug.Log("CreateNew " + name);
			GameObject obj = new GameObject(name);
			obj.transform.parent = RuntimeParent();
			return obj;
		}

		public virtual UnityEngine.Transform RuntimeParent() 
		{
			if (_runtimeParent == null)
			{
				GameObject runtimeParent = GameObject.Find("/Runtime");
				if (runtimeParent == null)
				{
					//				Picus.Sys.Debug.Throw("ResourceManager.RuntimeParent /Runtime not found. Creating one.", true);
					runtimeParent = new GameObject();
					runtimeParent.name = "Runtime";
				}
				_runtimeParent = runtimeParent.transform;
			}
			
			return _runtimeParent;
		}

		public new virtual UnityEngine.GameObject Instantiate(UnityEngine.GameObject prefab)
		{
			if (prefab == null)
			{
				Picus.Sys.Debug.Throw("ResourceManager.Instantiate null", true);
				return null;
			}

			UnityEngine.GameObject obj = UnityEngine.GameObject.Instantiate(prefab) as UnityEngine.GameObject;
			Debug.Log("Instantiate " + obj + " id " + obj.GetInstanceID());
			obj.transform.parent = RuntimeParent();
			Picus.Utils.GO.Finder.FindComponentAddIfNotExist<Picus.Utils.GO.TopInfo>(obj);
			return obj;
		}

		public new virtual void Destroy(GameObject obj)
		{
			try
			{
				Debug.Log("Destroy " + obj + " id " + obj.GetInstanceID());
				if (obj == null)  // already destroyed
				{
					Picus.Sys.Debug.Throw("Destroying already destroyed id " + obj.GetInstanceID() + " can happen on scene change with no issues.", true);
					return;
				}
			}
			catch (System.NullReferenceException)
			{
				Debug.Log("Destroy null.");
				return;
			}			

			// notify others
			Utils.GO.Linker.OnObjectWillBeDestroyed(obj);
			if (ManagerCrtn.Instance != null)
				ManagerCrtn.Instance.OnObjectWillBeDestroyed(obj);
			
			MonoBehaviour.Destroy(obj);
		}

		protected virtual void Awake()
		{
			Debug.Assert(Instance == null, "You are starting another Picus.Coroutine.Manager. It should be singleton.", true);
			
			if (Instance == null)
				Instance = this;
		}
		
		protected virtual void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}
	}
}