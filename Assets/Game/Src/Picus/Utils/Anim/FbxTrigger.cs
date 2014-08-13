using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Picus.Utils.Anim
{
	/// <summary>
	/// Link and solve gameObjects enabling/disabling in depend on parent gameObject scale.
	/// Scale == 1 mean enabled. Scale != 1 mean dissabled.
	/// </summary>
	public class FbxTrigger : Picus.MonoBehaviourExtend 
	{
		private Transform _triggerScale = null;
		private UnityEngine.GameObject _childObj = null;
		private bool _inheritRotation;

		/// <summary>
		/// Init the FbxTrigger component.
		/// </summary>
		/// <param name="triggerScale">Get scale.x from this component and show/hide childObj at runtime regard this trigger.</param>
		/// <param name="childObj">Linked and show/hided gameObject.</param>
		/// <param name="inheritRotation">If set to <c>true</c> childObj will inherit rotation.</param>
		public void Init(Transform triggerScale, UnityEngine.GameObject childObj, bool inheritRotation)
		{
			_triggerScale = triggerScale;
			_childObj = childObj;
			_inheritRotation = inheritRotation;
		}

		private void LateUpdate()
		{
			if (_triggerScale == null)
				return;

			if (_triggerScale.localScale.x == 1)
			{
				if (!_childObj.activeSelf)
					_childObj.SetActive(true);
			}
			else
			{
				if (_childObj.activeSelf)
					_childObj.SetActive(false);
			}

			if (!_inheritRotation)
			{
				transform.rotation = Quaternion.identity;
			}
		}

		private void Start()
		{
			_childObj.SetActive(false); // test
		}
	}
}