// #define USE_ORIGINAL_COROUTINES
// #define BUBBLE_EXCEPTION

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// based on http://twistedoakstudios.com/blog/Post83_coroutines-more-than-you-want-to-know
// and on http://wiki.unity3d.com/index.php?title=CoroutineScheduler

namespace Picus.Sys.Coroutine
{
	public class ManagerCrtn : Manager
	{
		public static string CoroutineStackDebugId = "CoroutineStack: ";

		public static ManagerCrtn Instance { get; private set; }

		[SerializeField]
		private List<ObjectCrtn> _allCoroutines = new List<ObjectCrtn>();
#if DEBUG && UNITY_EDITOR
		[SerializeField]
		#pragma warning disable 0414 // dissable warning
		private List<ObjectCrtn> _leafCoroutines = new List<ObjectCrtn>(); // TODO: GS show only deepest childs
		#pragma warning restore 0414
#endif
		public override string ToString() 
		{ 
			return DebugListActive(); 
		}

		/// <summary>
		/// Starts the coroutine ret.
		/// </summary>
		/// <returns>The coroutine ret.</returns>
		/// <param name="ecoroutine">Ecoroutine.</param>
		/// <param name="obj">Object.</param>
		/// <param name="ObjectRetCrtn">Object ret crtn.</param>
		/// <param name="onException">On exception.</param>
		/// <param name="id">Identifier.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public UnityEngine.Coroutine StartCoroutineRet<T>(IEnumerator ecoroutine, GameObject obj, out ObjectRetCrtn<T> objectRetCrtn, ObjectCrtn.MethodExceptionParam onException = null, bool destroyWithParent = true)
		{
			PurgeDeletedCoroutines();

			GameObject onGameObject = obj ?? gameObject; // if obj not defined, start it on this gameobject
			objectRetCrtn = new ObjectRetCrtn<T>(OnCoroutineEnd, ecoroutine, onException, onGameObject, ObjectCrtn.ActiveCoroutine);
			objectRetCrtn.DestroyWithParent = destroyWithParent;
	#if USE_ORIGINAL_COROUTINES
			return StartCoroutineOriginal(ecoroutine);
	#endif
			return CreateCoroutine(objectRetCrtn);
		}

		/// <summary>
		/// Starts the coroutine.
		/// </summary>
		/// <returns>If not forked, returns coroutine.</returns>
		/// <param name="ecoroutine">Coroutine method</param>
		/// <param name="obj">Start it on this gameObject. If null, start on coroutineManager gameObject.</param>
		/// <param name="forked">When parent coroutine will be stoped, this will remain.</param> 
		/// <param name="id">Identifier used for kill coroutine with all notforked childrens.</param>
		/// <param name="onException">On exception call. Not implemented yet!</param>
		public UnityEngine.Coroutine StartCoroutine(IEnumerator ecoroutine, GameObject obj, bool forked, ObjectCrtn.MethodExceptionParam onException = null, bool destroyWithParent = true)
		{
			Debug.Assert(forked || ObjectCrtn.ActiveCoroutine != null, "Starting main coroutine " + ecoroutine + ", but no parent coroutine active.", true);
	#if USE_ORIGINAL_COROUTINES
			return StartCoroutineOriginal(ecoroutine);
	#endif
			PurgeDeletedCoroutines();

			GameObject onGameObject = obj ?? gameObject; // if obj not defined, start it on this gameobject
			ObjectCrtn objectCoroutine = new ObjectCrtn(OnCoroutineEnd, ecoroutine, onException, onGameObject, forked ? null : ObjectCrtn.ActiveCoroutine);
			objectCoroutine.DestroyWithParent = destroyWithParent;
			UnityEngine.Coroutine coroutine = CreateCoroutine(objectCoroutine);

			return forked ? null : coroutine; 
		}

		public void KillAllCoroutines(bool permanentToo)
		{
			Debug.Log("CoroutineManager killing all " + _allCoroutines.Count + " coroutines.");

			foreach(ObjectCrtn coroutineObj in _allCoroutines)
			{
	//			if (permanentToo || !coroutineObj.Permanent) 
					coroutineObj.Kill();
			}
		}

		public IEnumerator ThisIsBoolRetCoroutine()
		{
			yield return true;
		}

		public void KillCoroutine(IEnumerator id)
		{
			ObjectCrtn obj = _allCoroutines.Find(x => x.CoroutineEnumerator == id);

			if (obj != null)
				obj.Kill();
		}

		public bool IsCoroutineActive(IEnumerator id)
		{
			ObjectCrtn finded = _allCoroutines.Find(x => x.CoroutineEnumerator == id);
			return finded != null;
		}

		public IEnumerator CurrentCoroutineId()
		{
			ObjectCrtn active = ObjectCrtn.ActiveCoroutine;
			Debug.Assert(active != null, "Coroutine.Manager.CurrentCoroutineId but no coroutine active!", true); 
			return active == null ? default(IEnumerator) : active.CoroutineEnumerator;
		}

		public void OnObjectWillBeDestroyed(GameObject obj)
		{
			List<ObjectCrtn> finded = _allCoroutines.FindAll(x => x.GameObject.GetInstanceID() == obj.GetInstanceID());

			for (int i = 0, cnt = finded.Count; i < cnt; ++i)
			{
				OnCoroutineEnd(finded[i]);
			}
	/*
	#if DEBUG
			List<ObjectCoroutine> finded = _objectCoroutines.FindAll(x => x.GameObject == obj);

			foreach(ObjectCoroutine coroutineObj in finded)
			{
				Debug.Assert(coroutineObj.GameObject == null || !coroutineObj.Active, "CoroutineManger obj " + obj + " will be destroyed, but still some coroutines active on it. " + coroutineObj.Name, true);
			}
	#endif
	*/
		}

		public override void OnSceneChanged() // scene loaded
		{
			PurgeDeletedCoroutines();
	//		KillAllCoroutines(false);
		}

		public string DebugListActive()
		{
			string ret = "Currently active " + _allCoroutines.Count + " coroutines.";
			UnityEngine.Debug.Log(ret);
			int idx = 0;
			foreach(ObjectCrtn coroutineObj in _allCoroutines)
			{
				string info = "";
				if (coroutineObj.GameObject == null)
					info = " DELETED ";
				else if (!coroutineObj.Active)
					info = " KILLING ";
				string text = ++idx + info + ": " + coroutineObj + " stack " + coroutineObj.Stack + ".";
				ret += text;
				UnityEngine.Debug.Log(text);
			}

			return ret;
		}

		/* private */
		private ManagerCrtn() {}

		private void OnCoroutineEnd(ObjectCrtn objectCoroutine)
		{
			Debug.Log("START CoroutineManager.OnCoroutineEnd " + objectCoroutine + " remaining " + _allCoroutines.Count + " coroutines.");

			if (objectCoroutine.ParentCoroutine != null)
			{
				Debug.Assert(objectCoroutine.ParentCoroutine.ChildCoroutine == objectCoroutine, "OnCoroutineEnd parent " + objectCoroutine.ParentCoroutine + " has child " + objectCoroutine.ParentCoroutine.ChildCoroutine + " instead of my " + objectCoroutine, true);
				objectCoroutine.ParentCoroutine.SetChild(null);
			}

			if (objectCoroutine.ChildCoroutine != null && objectCoroutine.ChildCoroutine.DestroyWithParent) // TODO: GS && objectCoroutine.ChildCoroutine.GameObject == objectCoroutine.GameObject
			{
// TODO: GS now GO is not deleted				Debug.Assert(objectCoroutine.GameObject == null, "Killing coroutine " + objectCoroutine.Name + ", but it still has child " + objectCoroutine.ChildCoroutine.Name + " active. I am killing it too.", true);
				objectCoroutine.ChildCoroutine.Kill();
			}
#if DEBUG && UNITY_EDITOR
			if (objectCoroutine.GameObject && objectCoroutine != ObjectCrtn.ActiveCoroutine)
				Picus.Utils.GO.Finder.FindComponentAddIfNotExist<GameObjectInfoCrtn>(objectCoroutine.GameObject).Remove(objectCoroutine);
#endif
			RemoveObject(objectCoroutine);

			Debug.Log("END CoroutineManager.OnCoroutineEnd " + objectCoroutine + " remaining " + _allCoroutines.Count + " coroutines.");
		}

		private UnityEngine.Coroutine CreateCoroutine(ObjectCrtn objectCoroutine)
		{
	//		Picus.Sys.Debug.StartLogSection("CoroutineManager.StartCoroutine " + ecoroutine.ToString(), Picus.Sys.Debug.Filter.LOG_COROUTINES);
			Debug.Log("START CoroutineManager.OnCoroutineStart " + objectCoroutine + " totally " + _allCoroutines.Count + " coroutines.");

			AddObject(objectCoroutine);
#if DEBUG && UNITY_EDITOR
			if (objectCoroutine.GameObject)
				Picus.Utils.GO.Finder.FindComponentAddIfNotExist<GameObjectInfoCrtn>(objectCoroutine.GameObject).Add(objectCoroutine);
#endif
			objectCoroutine.Coroutine = objectCoroutine.GameObject.GetComponent<Picus.MonoBehaviourExtend>().StartCoroutineOriginal(objectCoroutine.InternalRoutine(objectCoroutine.CoroutineEnumerator));

			Debug.Log("END CoroutineManager.OnCoroutineStart " + objectCoroutine + " totally " + _allCoroutines.Count + " coroutines.");

	//		Picus.Sys.Debug.StopLogSection();
			return objectCoroutine.Coroutine;
		}

		private void PurgeDeletedCoroutines()
		{
			List<ObjectCrtn> objectCoroutinesCopy = new List<ObjectCrtn>(_allCoroutines);
			int cnt = objectCoroutinesCopy.Count;
			for (int i = 0; i < cnt; ++i)
			{
				ObjectCrtn coroutineObj = objectCoroutinesCopy[i];
				if (coroutineObj.GameObject == null)
				{
					OnCoroutineEnd(coroutineObj);
				}
			}
		}

		private void RemoveObject(ObjectCrtn objectCoroutine)
		{
			_allCoroutines.Remove(objectCoroutine);
#if DEBUG && UNITY_EDITOR
			_leafCoroutines = _allCoroutines.FindAll(x => x.ChildCoroutine == null);
#endif
		}

		private void AddObject(ObjectCrtn objectCoroutine)
		{
			_allCoroutines.Add(objectCoroutine);
#if DEBUG && UNITY_EDITOR
			_leafCoroutines = _allCoroutines.FindAll(x => x.ChildCoroutine == null);
#endif
		}

		private void Awake()
		{
			Debug.Assert(Instance == null, "You are starting another Picus.Coroutine.Manager. It should be singleton.", true);

			if (Instance == null)
				Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}
	}
}