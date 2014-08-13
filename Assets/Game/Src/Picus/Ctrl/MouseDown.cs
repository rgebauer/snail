using System;
using UnityEngine;
using System.Collections.Generic;

namespace Picus.Ctrl
{
	public class MouseDown
	{
		public const float DRAGGED_DIST_INCH = 0.25f;
		
		public bool IsDown { get; private set; }
		public float StartTime { get; private set; }
		public Vector2 StartPos { get; protected set; }
		public GameObject StartOverObject { get; private set; }		
		public GameObject LastOverObject { get; private set; }
		public List<GameObject> LastOverObjects { get { return _lastOverObjects; } }
		public Ctrl.ObjectCtrl StartControlObject { get; private set; }

		private static float _draggedDistPx = -1;

		private bool _isDragged;
		private bool _isDraggedY;

		private List<GameObject> _lastOverObjects;

		public static float DraggedDistPx()
		{
			if (_draggedDistPx == -1)
			{
				_draggedDistPx = 30;
				if (Screen.dpi > 0) _draggedDistPx = DRAGGED_DIST_INCH * Screen.dpi;
				Debug.Log("MouseDown dpi " + Screen.dpi + " _draggedDistPx " + _draggedDistPx);
			}

			return _draggedDistPx;
		}

		public bool IsDragged()
		{
			if (!_isDragged)
				ComputeDragged();
			return _isDragged;
		}

		public bool IsDraggedY()
		{
			if (!_isDraggedY)
				ComputeDraggedY();
			return _isDraggedY;
		}

		public void RestartClick()
		{
			StopClick();
			StartClick();
		}
		
		public ObjectCtrl StartClick()
		{
			Debug.Assert(!IsDown, "Mouse::MouseDown::StartClick() already started", true);

			GameObject gameObj = Picus.Ctrl.ManagerCtrl.Instance.MouseOverObject(out _lastOverObjects);
			ObjectCtrl controlObj = gameObj == null ? null : gameObj.GetComponent<ObjectCtrl>();

			StartTime = Time.time;
			IsDown = true;
			_isDragged = false;
			_isDraggedY = false;
			StartPos = Input.mousePosition;

			StartOverObject = gameObj;
			LastOverObject = StartOverObject;
			StartControlObject = controlObj;

			return controlObj;
		}
		
		public virtual void StopClick()
		{
			Debug.Assert(IsDown, "Mouse::MouseDown::StopClick() not started", true);
			
			IsDown = false;
			StartOverObject = null;
			LastOverObject = null;			
			_lastOverObjects = null;
		}
		
		public GameObject OverObject()
		{
			LastOverObject = Picus.Ctrl.ManagerCtrl.Instance.MouseOverObject(out _lastOverObjects);

			return LastOverObject;
		}

		public bool OverObjectChanged()
		{
			GameObject lastObj = LastOverObject;

			if (IsDown) LastOverObject = Picus.Ctrl.ManagerCtrl.Instance.MouseOverObject(out _lastOverObjects);
			else LastOverObject = null;

			return LastOverObject != lastObj;
		}

		void ComputeDragged()
		{
			if (!IsDown)
				return;
			
			Vector2 mousePos = Input.mousePosition;
			if (!_isDragged && (mousePos - StartPos).magnitude > DraggedDistPx())
				_isDragged = true;	
		}

		public void ComputeDraggedY()
		{
			if (!IsDown)
				return;
			
			Vector2 mousePos = Input.mousePosition;
			if (!_isDraggedY && Mathf.Abs((mousePos - StartPos).y) > DraggedDistPx())
				_isDraggedY = true;
		}

	}
	
}

