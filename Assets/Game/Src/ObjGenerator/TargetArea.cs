using UnityEngine;
using System.Collections;

namespace Generators
{

	public class TargetArea : Picus.MonoBehaviourExtend {

		public Vector3 AvailableAxes = new Vector3(1,1,1);

		public BoxCollider AreaCollider;

		public Vector3 GetSize 
		{
			get 
			{
				return AreaCollider.bounds.size;
			}
		}

		void Start()
		{
			Debug.Assert(AreaCollider, "Generators.TargetArea.Start() - MISSING AreaCollider");
		}
	}
}