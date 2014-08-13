using UnityEngine;
using System.Collections;

namespace Picus.Ctrl
{
	[RequireComponent (typeof(Collider2D))]
	public abstract class ObjectCtrl : Picus.MonoBehaviourExtend
	{
		public virtual void OnClicked() {} // touch was pressed and released on this object

		// override these for implement component behaviour

		public virtual void MouseMoved(GameObject newObj) {} // touch was started on this object, but moved to new one

		public virtual void MouseUp(GameObject overObj) {} // touch was started on this object but released on new one

		public virtual void MouseDown() {} // touch on this object just started

		// END override these for implement component behaviour
	}
}