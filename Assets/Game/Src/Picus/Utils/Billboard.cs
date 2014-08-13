using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Picus.Utils
{
	[ExecuteInEditMode]
	public class Billboard : Picus.MonoBehaviourExtend 
	{
		// TODO: rotation only in some axis
		private void LateUpdate()
		{
			Vector3 targetPos = transform.position + Camera.main.transform.rotation * Vector3.forward;
			Vector3 targetOrient = Camera.main.transform.rotation * Vector3.up;
			transform.LookAt(targetPos, targetOrient);
		}
	}
}