using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Picus.Extensions;

namespace Picus.Ctrl
{
	public class ManagerCtrl : Picus.Sys.Manager
	{
		public static ManagerCtrl Instance { get; private set; }

		public const float DOUBLE_CLICK_TIME = 0.5f;
		public const float RAY_DISTANCE = 30.0f;

		public bool IsDragged { get { return _mouseDown.IsDragged(); } }
		public bool IsDraggedY { get { return _mouseDown.IsDraggedY(); } }
		public bool IsMouseDown { get { return _mouseDown.IsDown; } }

		MouseDown _mouseDown = new MouseDown();	
		private GameObject _lastClickedObject;
		private float _doubleClickEndTime;

		private int _controlLayerMask = -5;

  		public Vector2 ScreenSizeMeters()
		{
			if (_delta3Dpos.magnitude == 0)
				PrecomputeTranslation();

				return _delta3Dpos;
		}

		private Vector2 _min3Dpos, _delta3Dpos;
		private Ray _mouseRay;

		public Vector2 MouseToSceneZeroZPosition()
		{
			PrecomputeTranslation();
			return (_min3Dpos + new Vector2(Input.mousePosition.x / Screen.width * _delta3Dpos.x, Input.mousePosition.y / Screen.height * _delta3Dpos.y));
		}

		public Vector2 SceneZeroZToScreenPosition(Vector3 pos)
		{
			Debug.Assert(Mathf.Abs(pos.z) < 0.1f, "Ctrl.Manager.SceneToScreenPosition is not on Z");
			PrecomputeTranslation();

			Vector2 point = new Vector2();
			point.x = (pos.x - _min3Dpos.x) / _delta3Dpos.x * Screen.width;
			point.y = (pos.y - _min3Dpos.y) / _delta3Dpos.y * Screen.height;
			return point;
		}

		public static Vector3 SceneToSceneZeroZPosition(Vector3 pos)
		{
			if (pos.z == 0)
				return pos;

			Vector3 camPos = Camera.main.transform.position;
			Vector3 camVec = camPos - pos;
			camVec.Normalize();
			float dist = -pos.z / camVec.z;

			return pos + camVec * dist;
		}

		public Vector2 SceneToScreenPosition(Vector3 pos)
		{
			return SceneZeroZToScreenPosition(SceneToSceneZeroZPosition(pos));
		}

		public bool IsObjectOverObject(GameObject topObj, GameObject bottomObj)
		{
			bool hasTopCollider3D = Picus.Utils.GO.Finder.HasColliderDeep(topObj, true);
			bool hasTopCollider2D = Picus.Utils.GO.Finder.HasColliderDeep(topObj, false);

			if (!hasTopCollider2D && !hasTopCollider3D)
				return false;

			bool hasBottomCollider3D = Picus.Utils.GO.Finder.HasColliderDeep(bottomObj, true);
			bool hasBottomCollider2D = Picus.Utils.GO.Finder.HasColliderDeep(bottomObj, false);
			
			if (!hasBottomCollider2D && !hasBottomCollider3D)
				return false;

			if (hasTopCollider2D == hasTopCollider3D)
			{
				Picus.Sys.Debug.Throw("Ctrl.Manager.IsObjectOverObject top object " + topObj + " has both 2D and 3D colliders. Ignoring 3D.", true);
				hasTopCollider3D = false;
			}

			if (hasBottomCollider2D == hasBottomCollider3D)
			{
				Picus.Sys.Debug.Throw("Ctrl.Manager.IsObjectOverObject bottom object " + bottomObj + " has both 2D and 3D colliders. Ignoring 3D.", true);
				hasBottomCollider3D = false;
			}

			Bounds boundsTopObj, boundsBottomObj;

			Picus.Utils.GO.Bounds.BoundsType topBoundsType = hasTopCollider3D ? Picus.Utils.GO.Bounds.BoundsType.Collider : Picus.Utils.GO.Bounds.BoundsType.Collider2D;
			Picus.Utils.GO.Bounds.BoundsType bottomBoundsType = hasBottomCollider3D ? Picus.Utils.GO.Bounds.BoundsType.Collider : Picus.Utils.GO.Bounds.BoundsType.Collider2D;

			if (!Picus.Utils.GO.Bounds.BoundsComponent(topBoundsType, topObj, out boundsTopObj, true) || !Picus.Utils.GO.Bounds.BoundsComponent(bottomBoundsType, bottomObj, out boundsBottomObj, true)) 
				return false;

			boundsTopObj.SetMinMax(SceneToSceneZeroZPosition(boundsTopObj.min), SceneToSceneZeroZPosition(boundsTopObj.max));
			boundsBottomObj.SetMinMax(SceneToSceneZeroZPosition(boundsBottomObj.min), SceneToSceneZeroZPosition(boundsBottomObj.max));

			bool intersect = boundsTopObj.Intersects(boundsBottomObj);

#if DEBUG
			if (intersect)
				Debug.Log("Ctrl.ManagerCtrl.IsObjectOverObject " + topObj + " at " + topObj.transform.position + " box " + boundsTopObj + " is over " + bottomObj + " at " + bottomObj.transform.position + " box " + boundsBottomObj);
#endif

			return intersect;
		}

		public GameObject IsObjectOverObject(GameObject topObj, List<GameObject> bottomObjs, bool regardMousePos)
		{
			float minDist = float.MaxValue;
			GameObject nearest = null;
			Vector3 mousePosition;
			if (regardMousePos)
			{
				Vector2 mousePosition2D = MouseToSceneZeroZPosition();
				mousePosition = new Vector3(mousePosition2D.x, mousePosition2D.y, 0);
			}

			Vector3 topZeroZPosition = SceneToSceneZeroZPosition(topObj.transform.position);

			foreach (GameObject bottomObj in bottomObjs)
			{
				Vector3 bottomZeroZPosition = SceneToSceneZeroZPosition(bottomObj.transform.position);

				if (IsObjectOverObject(topObj, bottomObj))
				{
					float dist;
					if (regardMousePos) 
						dist = (mousePosition - bottomZeroZPosition).magnitude;
					else 
						dist = (topZeroZPosition - bottomZeroZPosition).magnitude;
					if (minDist > dist)
					{
						minDist = dist;
						nearest = bottomObj;
					}
				}
			}

			return nearest;
		}

		public bool IsMouseOver(GameObject obj)
		{
			bool hasCollider3D = Picus.Utils.GO.Finder.HasColliderDeep(obj, true);
			bool hasCollider2D = Picus.Utils.GO.Finder.HasColliderDeep(obj, false);

			if (hasCollider2D)
			{
				Vector2 from2D = MouseToSceneZeroZPosition();
				RaycastHit2D[] hits2D = Physics2D.RaycastAll(from2D, Vector2.up, 0, _controlLayerMask);

				for (int i = 0; i < hits2D.Length; ++i)
					if (Picus.Utils.GO.Finder.FindChildDeep(obj, hits2D[i].collider.gameObject))
						return true;
			}
			if (hasCollider3D)
			{
				Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

				RaycastHit[] hits3D = Physics.RaycastAll(mouseRay, RAY_DISTANCE, _controlLayerMask);
				for (int i = 0; i < hits3D.Length; ++i)
					if (Picus.Utils.GO.Finder.FindChildDeep(obj, hits3D[i].collider.gameObject))
						return true;
			}

			return false;	
		}

		public GameObject MouseOverObject(out List<GameObject> collidedObjs)
		{
			return MouseOverObject(-5, out collidedObjs); // TODO: GS ignore mask
		}

		/** Returns sorted list of touched objects. First is nearest. */
		public GameObject MouseOverObject(LayerMask mask, out List<GameObject> collidedObjs)
		{
			GameObject topObj = null;
			int topObjSortingLevel = -2;
			collidedObjs = new List<GameObject>();

			// get collided objs with 2d colliders
			Vector2 from2D = MouseToSceneZeroZPosition();
			RaycastHit2D[] hits2D = Physics2D.RaycastAll(from2D, Vector2.up, 0, mask);
			int cnt2D = hits2D.Length;
			for (int i = 0; i < cnt2D; ++i)
			{
				UnityEngine.GameObject obj = hits2D[i].collider.gameObject;
				collidedObjs.Add(obj);
				Debug.Assert(Mathf.Abs(obj.transform.position.z) < 0.1f, "Touched object " + obj + " top " + obj.GameObjectTop() + " collision is not on zero Z " + obj.transform.position.z , true);
			}

			// get collided objs with 3d colliders
			_mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);	
			RaycastHit[] hits3D = Physics.RaycastAll(_mouseRay, RAY_DISTANCE, mask);
			int cnt3D = hits3D.Length;
			for (int i = 0; i < cnt3D; ++i)
				collidedObjs.Add(hits3D[i].collider.gameObject);

#if DEBUG
			string mouseRayText = "mouseRay px " +  _mouseRay.origin.x + " py " + _mouseRay.origin.y + " pz " + _mouseRay.origin.z + " dx " + _mouseRay.direction.x + " dy " + _mouseRay.direction.y + " dz " + _mouseRay.direction.z;
			Debug.Log(mouseRayText);
			string debug = "ManagerCtrl.MouseOverObject choosing between " + collidedObjs.Count + ": "; // mouseRay " + _mouseRay + "
#endif
			foreach(GameObject collidedObj in collidedObjs)
			{
				int hitSortingLevel = Picus.Utils.SortingLayer.RecomputedSortingLevel(collidedObj);

				if (hitSortingLevel > topObjSortingLevel)
				{
					topObjSortingLevel = hitSortingLevel;
					topObj = collidedObj;
				}
#if DEBUG
				string topInfo = collidedObj.ToString(); // (collidedObj.GameObjectTopInfo<GameObjectTopInfo>() == null ? "NO_GO_TOPINFO " + collidedObj : collidedObj.GameObjectTopInfo<GameObjectTopInfo>().ShortName());
				debug += " '" + topInfo + "'(" + hitSortingLevel + ") ";
#endif
			}
#if DEBUG
			if (topObj != null) 
				debug = debug + " => '" + topObj.ToString(); // (topObj.GameObjectTop() == null ? "NONAME" : topObj.GameObjectTopInfo<Utils.GameObjectTopInfo>() == null ? "NO_TOPINFO`" : topObj.GameObjectTopInfo<Utils.GameObjectTopInfo>().Name()) + "'";
			Debug.Log(debug);
#endif

			return topObj;
		}	
  
		public void MouseDown()
		{
			_mouseDown.OverObject();

			Debug.Log("TouchController::StartTouch() startOver: " + _mouseDown.LastOverObject);
			ObjectCtrl controlObj = _mouseDown.StartClick();
			MessengerGlobal<GameObject>.Broadcast(EventsCtrl.TouchStart, _mouseDown.LastOverObject);
//			MessengerGlobal<List<GameObject>>.Broadcast(EventsCtrl.TouchStartAll, _mouseDown.LastOverObjects);
			MessengerGlobal<GameObject, GameObject>.Broadcast(EventsCtrl.TouchOverStart, null, _mouseDown.LastOverObject);

			if (controlObj != null) controlObj.MouseDown();
		}

		public void MouseUp()
		{
			Debug.Log("TouchController::EndTouch() startOver: " + _mouseDown.StartOverObject + " lastOver: " + _mouseDown.LastOverObject);
			MessengerGlobal<GameObject, GameObject>.Broadcast(EventsCtrl.TouchEnd, _mouseDown.StartOverObject, _mouseDown.LastOverObject);	
			MessengerGlobal<GameObject, GameObject, GameObject>.Broadcast(EventsCtrl.TouchOverEnd, _mouseDown.StartOverObject, _mouseDown.LastOverObject, null);

			if (_mouseDown.StartOverObject == _mouseDown.LastOverObject)
			{
				if (_lastClickedObject == _mouseDown.LastOverObject && _doubleClickEndTime > Time.time)
				{
					MessengerGlobal<GameObject>.Broadcast(EventsCtrl.DoubleClick, _mouseDown.LastOverObject);
					_lastClickedObject = null;
				}
				else
				{
					_lastClickedObject = _mouseDown.LastOverObject;
					_doubleClickEndTime = Time.time + DOUBLE_CLICK_TIME;
					MessengerGlobal<GameObject>.Broadcast(EventsCtrl.Click, _mouseDown.LastOverObject);
				}
			}
			else
			{
				_lastClickedObject = null;
			}

			if (_mouseDown.StartControlObject)
			{
				if (_mouseDown.LastOverObject == _mouseDown.StartControlObject.gameObject)
					_mouseDown.StartControlObject.OnClicked();
				_mouseDown.StartControlObject.MouseUp(_mouseDown.LastOverObject);
			}

			_mouseDown.StopClick();
		}

		public void RestartTouch()
		{
			_mouseDown.RestartClick();
		}

		public override void OnSceneChanged() 
		{
			_delta3Dpos = new Vector2(0, 0);
		}

		public GameObject MouseOverObjectCached()
		{
			return _mouseDown.LastOverObject;
		}

		public List<GameObject> MouseOverObjectsTopCached()
		{
			// TODO: GS cache
			List<GameObject> topObjects = new List<GameObject>();
			for (int i = 0, cnt = _mouseDown.LastOverObjects.Count; i < cnt; ++i)
			{
				GameObject topObj = _mouseDown.LastOverObjects[i].GameObjectTop();
				if (topObj != null && !topObjects.Contains(topObj))
					topObjects.Add(topObj);
			}

			return topObjects;
		}

		void LateUpdate()
		{
			if (_mouseDown.IsDown)
			{
				// test only, move it back to FixedUpdate
				GameObject prevObj = _mouseDown.LastOverObject;
				if (_mouseDown.OverObjectChanged())
				{
					MessengerGlobal<GameObject, GameObject, GameObject>.Broadcast(EventsCtrl.TouchOverEnd, _mouseDown.StartOverObject, prevObj, _mouseDown.LastOverObject);
					MessengerGlobal<GameObject, GameObject>.Broadcast(EventsCtrl.TouchOverStart, prevObj, _mouseDown.LastOverObject);
					if (_mouseDown.StartControlObject != null) _mouseDown.StartControlObject.MouseMoved(_mouseDown.LastOverObject);
				}

				if (!Input.GetMouseButton(0)) MouseUp();

			}
			else
			{
				if (Input.GetMouseButton(0)) MouseDown();
			}
		}

		private void PrecomputeTranslation()
		{
			if (_delta3Dpos.x == 0 && _delta3Dpos.y == 0)
			{
				GameObject table = new GameObject();
				table.layer = LayerMask.NameToLayer("ControlMouseOnly");
				BoxCollider collider = table.AddComponent<BoxCollider>();
				collider.size = new Vector3(int.MaxValue, int.MaxValue, 1);
				collider.center = new Vector3(collider.center.x, collider.center.y, collider.size.z / 2);
				
				Ray mouseRay = Camera.main.ScreenPointToRay(new Vector2(0, 0));
				RaycastHit rayHit;
				
				// TODO: compute instead od ray
				if (Physics.Raycast(mouseRay, out rayHit, RAY_DISTANCE, _controlLayerMask))
				{
					Debug.Assert(rayHit.point.z == 0, "Ctrl.Manager.MousePositionInScene min table collision is not on zero Z", true);
					_min3Dpos = rayHit.point;
				}
				else 
					Picus.Sys.Debug.Throw("Ctrl.Manager.MousePositionInScene can't compute min. Missing table collision?", true);
				
				mouseRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height));
				if (Physics.Raycast(mouseRay, out rayHit, int.MaxValue, _controlLayerMask))
				{
					Debug.Assert(rayHit.point.z == 0, "Ctrl.Manager.MousePositionInScene max table collision is not on zero Z", true);
					_delta3Dpos = new Vector2(rayHit.point.x, rayHit.point.y) - _min3Dpos;
				}
				else 
					Picus.Sys.Debug.Throw("Ctrl.Manager.MousePositionInScene can't compute max. Missing table collision?", true);
				
				Destroy(table);
			}
		}

#if DEBUG
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(_mouseRay.origin, _mouseRay.origin + _mouseRay.direction * 20.0f);
		}
#endif

		protected virtual void Awake()
		{
			_controlLayerMask = 1 << LayerMask.NameToLayer("ControlMouseOnly");
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
