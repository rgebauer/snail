using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Picus.Sys.Coroutine;

namespace Picus
{
	// based on http://devmag.org.za/2012/07/12/50-tips-for-working-with-unity-best-practices/
	public class MonoBehaviourExtend : MonoBehaviour
	{
		public delegate T MethodRet<T>();
		public delegate void MethodNoParam();
		public delegate void MethodOneParam<T1>(T1 p1);
		public delegate void MethodTwoParams<T1, T2>(T1 p1, T2 p2);
		public delegate void MethodExceptionParam(System.Exception e);

		private UnityEngine.GameObject _prefabTop;
		private Utils.GO.TopInfo _prefabTopInfo;

		public void Touch() {}

		/// <summary>
		/// Starts coroutine without coroutine manager. Do not use it, if You are not sure!
		/// </summary>
		public Coroutine StartCoroutineOriginal(IEnumerator ecoroutine)
		{
			return base.StartCoroutine(ecoroutine);
		}

		/// <summary>
		/// Start coroutine in new "thread".
		/// </summary>
		/// <param name="ecoroutine">Coroutine method.</param>
		public void StartForkCoroutine(IEnumerator ecoroutine)
		{
			CheckCoroutineNameDebug(ecoroutine, false);
			ManagerCrtn.Instance.StartCoroutine(ecoroutine, gameObject, true);
		}

		/// <summary>
		/// Start coroutine in this couroutine "thread". Will be stopped when parent coroutine is stopped.
		/// </summary>
		public Coroutine StartChildCoroutine(IEnumerator ecoroutine)
		{
			CheckCoroutineNameDebug(ecoroutine, false);
			return ManagerCrtn.Instance.StartCoroutine(ecoroutine, gameObject, false);
		}

		[System.Obsolete("Only hotfix for GL. Remove this and do not delete child coroutines on different gameObject.")]
		public Coroutine StartChildCoroutine(IEnumerator ecoroutine, bool destroyWithParent)
		{
			return ManagerCrtn.Instance.StartCoroutine(ecoroutine, gameObject, false, null, destroyWithParent);
		}

		/// <summary>
		/// Start coroutine in this couroutine "thread". Will be stopped when parent coroutine is stopped.
		/// </summary>
		/// <returns>The main coroutine ret.</returns>
		/// <param name="ecoroutine">Coroutine method.</param>
		/// <param name="objectCoroutine">Here in .Value will be returned value.</param>
		/// <typeparam name="T">Type of returned parameter.</typeparam>
		public Coroutine StartChildCoroutineRet<T>(IEnumerator ecoroutine, out ObjectRetCrtn<T> objectCoroutine)
		{
			CheckCoroutineNameDebug(ecoroutine, true);
			return StartChildCoroutineRet<T>(ecoroutine, out objectCoroutine, gameObject);
		}

		[System.Obsolete("Only hotfix for GL. Remove this and do not delete child coroutines on different gameObject.")]
		public Coroutine StartChildCoroutineRet<T>(IEnumerator ecoroutine, out ObjectRetCrtn<T> objectCoroutine, bool destroyWithParent)
		{
			return ManagerCrtn.Instance.StartCoroutineRet<T>(ecoroutine, gameObject, out objectCoroutine, null, destroyWithParent);
		}


		/** 
		 * OBSOLETE!!!
		 */
		[System.Obsolete("MonoBehaviourExtend.StartCoroutine do not use it! Use StartForkCoroutine(method(), string id) instead.")]
		public new Coroutine StartCoroutine(string name, object obj)
		{
			Picus.Sys.Debug.Throw("MonoBehaviourExtend.StartCoroutine do not use it! Use StartForkCoroutine(method(), string id) instead.");
			return null;
		}

		/** 
		 * OBSOLETE!!!
		 */
		[System.Obsolete("MonoBehaviourExtend.StartCoroutine do not use it! Use StartForkCoroutine(method(), string id) instead.")]
		public new Coroutine StartCoroutine(string name)
		{
			Picus.Sys.Debug.Throw("MonoBehaviourExtend.StartCoroutine do not use it! Use StartForkCoroutine(method(), string id) instead.");
			return null;
		}

		/** 
		 * OBSOLETE!!!
		 */
		[System.Obsolete("MonoBehaviourExtend.StartCoroutine do not use it! Use StartForkCoroutine if You are not waiting for return or StartMainCoroutine if You are!")]
		public new Coroutine StartCoroutine(IEnumerator coroutine)
		{
			Picus.Sys.Debug.Throw("MonoBehaviourExtend.StartCoroutine do not use it! Use StartForkCoroutine if You are not waiting for return or StartMainCoroutine if You are!", true);
			return StartChildCoroutine(coroutine);
		}

		/** 
		 * OBSOLETE!!!
		 */
		[System.Obsolete("MonoBehaviourExtend.StopCoroutine do not use it! Use StopCoroutine(IEnumerator id) instead.")]
		protected new void StopCoroutine(string id)
		{
			Picus.Sys.Debug.Throw("MonoBehaviourExtend.StopCoroutine do not use it! Use StopCoroutine(IEnumerator id) instead.", true);
		}

		protected UnityEngine.GameObject GameObjectTop(int level)
		{
			return Utils.GO.TopInfo.GameObjectTop(gameObject, level);
		}
		
		protected UnityEngine.GameObject GameObjectTop()
		{
			if (_prefabTop == null)
			{
				_prefabTop = Utils.GO.TopInfo.GameObjectTop(gameObject, 1);
				
				if (_prefabTop != null) 
					_prefabTopInfo = _prefabTop.GetComponent<Utils.GO.TopInfo>();
				Debug.Assert(_prefabTop != null, "GameObjectTop() asked but not found", true);
			}
			
			return _prefabTop;
		}
		
		protected T GameObjectTopInfo<T>() where T: Utils.GO.TopInfo
		{
			if (_prefabTopInfo == null && GameObjectTop() != null) 
				_prefabTopInfo = GameObjectTop().GetComponent<Utils.GO.TopInfo>();
			
			Debug.Assert(_prefabTop != null, "GameObjectTopInfo<Utils.GameObjectTopInfo>() asked but not found", true);
			
			return _prefabTopInfo as T;
		}

		protected static UnityEngine.GameObject Instantiate(string resourceName)
		{
#if UNITY_EDITOR
			// do not use resource manager in edit mode
			if(!Application.isPlaying)
				return UnityEngine.GameObject.Instantiate(Resources.Load<GameObject>(resourceName)) as GameObject;
#endif

			return Sys.ResourceManager.Instance.Instantiate(resourceName);
		}

		protected static UnityEngine.GameObject Instantiate(UnityEngine.GameObject prefab)
		{
#if UNITY_EDITOR
			// do not use resource manager in edit mode
			if(!Application.isPlaying)
				return UnityEngine.GameObject.Instantiate(prefab) as GameObject;
#endif
			UnityEngine.GameObject obj = Sys.ResourceManager.Instance.Instantiate(prefab);
			return obj;
		}

		protected static void Destroy(UnityEngine.GameObject obj)
		{
			Sys.ResourceManager.Instance.Destroy(obj);
		}

		protected static void Destroy(UnityEngine.GameObject obj, float time)
		{
			if (obj != null)
				obj.GetComponent<Picus.MonoBehaviourExtend>().StartForkCoroutine(obj.GetComponent<Picus.MonoBehaviourExtend>().CallAfterTimeCoroutine(Destroy, obj, time));
		}

		/// <summary>
		/// Start coroutine in this couroutine "thread". Will be stopped when parent coroutine is stopped.
		/// </summary>
		/// <returns>The main coroutine ret.</returns>
		/// <param name="ecoroutine">Coroutine method.</param>
		/// <param name="objectCoroutine">Here in .Value will be returned value.</param>
		/// <typeparam name="T">Type of returned parameter.</typeparam>
		/// <param name="onObject">Object on which coroutine will be started (and destroyed with this object).</param>
		protected Coroutine StartChildCoroutineRet<T>(IEnumerator ecoroutine, out ObjectRetCrtn<T> objectCoroutine, UnityEngine.GameObject onObject)
		{
			CheckCoroutineNameDebug(ecoroutine, true);
			return ManagerCrtn.Instance.StartCoroutineRet<T>(ecoroutine, onObject, out objectCoroutine);
		}

		/// <summary>
		/// Start coroutine in this couroutine "thread". Will be stopped when parent coroutine is stopped.
		/// </summary>
		/// <param name="ecoroutine">Coroutine method.</param>
		/// <param name="onObject">Object on which coroutine will be started (and destroyed with this object).</param>
		protected Coroutine StartChildCoroutine(IEnumerator ecoroutine, UnityEngine.GameObject onObject)
		{
			CheckCoroutineNameDebug(ecoroutine, false);
			return ManagerCrtn.Instance.StartCoroutine(ecoroutine, onObject, false);
		}

		/// <summary>
		/// Stops the coroutine.
		/// </summary>
		/// <param name="id">Coroutine name.</param>
		protected new void StopCoroutine(IEnumerator id)
		{
			if (ManagerCrtn.Instance != null) // can be on app quiting
				ManagerCrtn.Instance.KillCoroutine(id);
		}

		protected I GetInterfaceComponent<I>() where I : class
		{
			return GetComponent(typeof(I)) as I;
		}
		
		protected static List<I> FindObjectsOfInterface<I>() where I : class
		{
			MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>();
			List<I> list = new List<I>();
			
			foreach(MonoBehaviour behaviour in monoBehaviours)
			{
				I component = behaviour.GetComponent(typeof(I)) as I;
				
				if(component != null)
				{
					list.Add(component);
				}
			}
			
			return list;
		}

		protected IEnumerator CallAfterTimeCoroutine<T1, T2>(MethodTwoParams<T1, T2> method, T1 p1, T2 p2, float sec)
		{
			if (sec > 0) yield return new WaitForSeconds(sec);
			method(p1, p2);
		}

		protected IEnumerator CallAfterTimeCoroutine<T1>(MethodOneParam<T1> method, T1 p1, float sec)
		{
			if (sec > 0) yield return new WaitForSeconds(sec);
			method(p1);
		}

		protected IEnumerator CallAfterTimeCoroutine(MethodNoParam method, float sec)
		{
			if (sec > 0) yield return new WaitForSeconds(sec);
			method();
		}

		protected void DeInitAndDestroy(MonoBehaviourExtend ptr)
		{
			if (ptr != null)
				ptr.DeInit();

			Destroy(ptr);
		}

		public virtual void DeInit() {}

		[System.Diagnostics.Conditional("DEBUG")]
		private void CheckCoroutineNameDebug(IEnumerator ecoroutine, bool isRet)
		{
			if (isRet)
				Debug.Assert(ecoroutine.ToString().Contains("CoroutineRet>"), "Due our code conventions coroutine methods returning value name must end with CoroutineRet suffix. But " + ecoroutine.ToString() + " is not.", true);
			else
				Debug.Assert(ecoroutine.ToString().Contains("Coroutine>") || ecoroutine.ToString().Contains("CoroutineRet>"), "Due our code conventions coroutine methods name must end with Coroutine suffix. But " + ecoroutine.ToString() + " is not.", true);
		}
	}
}
