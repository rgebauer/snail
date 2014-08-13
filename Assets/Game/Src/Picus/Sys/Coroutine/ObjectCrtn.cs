using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Picus.Sys.Coroutine
{
	[System.Serializable]
	public class ObjectCrtn
	{
		public delegate void MethodOneParam<T1>(T1 p1);
		public delegate void MethodExceptionParam(System.Exception e);

		public UnityEngine.Coroutine Coroutine;

//		public bool Permanent = false;

		[SerializeField]
		private string _name; // first field will be showed in inspector
		[SerializeField]
		private bool _active = true;
		[SerializeField]
		private GameObject _gameObject;
		[SerializeField]
		private string _environmentStack;

#if DEBUG && UNITY_EDITOR
		[SerializeField] // inspector is not showing ObjectCrtn itself, need to be in list
		private List<ObjectCrtnLeafInspector> _coroutinesStack = new List<ObjectCrtnLeafInspector>();
#endif

//		[System.Obsolete("Only hotfix for GL. Remove this and do not delete child coroutines on different gameObject.")]
		public bool DestroyWithParent = true;
		public string Name { get { return _name; } }
		public string Stack { get { return _environmentStack; } }
		public bool Active { get { return _active; } }
		public GameObject GameObject { get { return _gameObject; } }
		public ObjectCrtn ParentCoroutine { get; private set; }
		public IEnumerator CoroutineEnumerator { get; private set; }
//		public int Id { get { return CoroutineEnumerator.GetHashCode(); } }

		public ObjectCrtn ChildCoroutine { get; private set; }

		public static ObjectCrtn ActiveCoroutine { get { return _activeCoroutines.Count == 0 ? null : _activeCoroutines.Peek(); } }

		private static Stack<ObjectCrtn> _activeCoroutines = new Stack<ObjectCrtn>();

		protected Exception _exception = null;
		protected MethodOneParam<ObjectCrtn> _onEnd;
		protected MethodExceptionParam _onException; // TODO: GS bubble it to parent, if not defined in child

		public override string ToString()
		{
			return _name; // + "[ID: " + Id + "]";
		}

		public ObjectCrtn(MethodOneParam<ObjectCrtn> onEnd, IEnumerator coroutineEnumerator, MethodExceptionParam onException, GameObject obj, ObjectCrtn parentCoroutine) 
		{ 
			_onException = onException;
			_onEnd = onEnd; 
			_gameObject = obj;
			_name = "NOT STARTED YET";

			ParentCoroutine = parentCoroutine;
			CoroutineEnumerator = coroutineEnumerator;

			if (parentCoroutine != null)
				parentCoroutine.SetChild(this);
		}
		
		public void Kill()
		{
			_active = false;
			if (ChildCoroutine != null)
				ChildCoroutine.Kill();
		}

		public void SetChild(ObjectCrtn child)
		{
			ChildCoroutine = child;
		}
		
		public IEnumerator InternalRoutine(IEnumerator coroutine)
		{
			_name = coroutine.ToString();
			SnapStack();

#if DEBUG && UNITY_EDITOR
			ObjectCrtn crtn = this;
			while (crtn != null)
			{
				_coroutinesStack.Add(crtn);
				crtn = crtn.ParentCoroutine;
			}
#endif

#if BUBBLE_EXCEPTION
			bool firstPass = true;
#endif			
			while(_active)
			{
				_activeCoroutines.Push(this);
				try
				{
					if(!coroutine.MoveNext())
					{
						SnapStack();
						_activeCoroutines.Pop();
						_onEnd(this);
						yield break;
					}
				}
				catch(Exception e)
				{
					_exception = e;
				}
				SnapStack();

				if (_exception != null)
				{
					_activeCoroutines.Pop();
					_onEnd(this);
					
					if (_onException != null)
						_onException(_exception);
					else 
					{
#if BUBBLE_EXCEPTION
						if (firstPass) // force first yield (throw before yield has different behaviour than after yield)
							yield return null; 
						Picus.Sys.Debug.Throw("CoroutineManager coroutine " + Name + " ended on bubbled exception " + _exception.Message + "\n Stack: " + _exception.StackTrace);
#else
						// can't really throw, only log!! (throw before yield has different behaviour than after yield
						Picus.Sys.Debug.Throw("CoroutineManager coroutine " + _name + " ended on exception " + _exception.Message + "\n " + ManagerCrtn.CoroutineStackDebugId + "\n" + _exception.StackTrace, true);
#endif
					}
					
					yield break;
				}
#if BUBBLE_EXCEPTION
				firstPass = false;
#endif
				_activeCoroutines.Pop();
    			object yielded = coroutine.Current;
				if (SolveYieldEnd(yielded))
				{
					_onEnd(this);
					yield break;
				}
				else
				{
					yield return yielded;
				}
			}

			_onEnd(this);
			_active = false;
		}

		protected virtual bool SolveYieldEnd(object yielded)
		{
			return false;
		}

		protected void SnapStack()
		{
			_environmentStack = Environment.StackTrace;
		}
	}
}