using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Picus.Utils.Anim
{
	// TODO: remember strings instead of Trigger transforms, remember animator controller and fbxprefab and link it automatically

	/// <summary>
	/// Definition for FbxTrigger.
	/// PrefabToLink[X] will be linked to TriggerScaleAndLinkToParent[X]
	/// PrefabToLink[X] will be showed/hided regards TriggerScaleAndLinkToParent[X].transform.scale.x value.
	/// </summary>
	public class FbxTriggerDefinition : Picus.MonoBehaviourExtend 
	{
		public UnityEngine.GameObject[] PrefabToLink; // will be instantiated = XOBJ
		public Transform[] TriggerScaleAndLinkToParent; // XOBJ will be linked to parent of this obj. XOBJ will be showed when this obj scale.x = 1 (hide scale.x != 1)
		public bool InheritRotation = true;



		private void Start()
		{
			int cnt = TriggerScaleAndLinkToParent.Length;
			int prefabsCnt = PrefabToLink.Length;

			for (int i = 0; i < cnt; ++i)
			{
				int prefabIdx = i < prefabsCnt ? i : prefabsCnt - 1;
				UnityEngine.GameObject obj = new UnityEngine.GameObject("FbxTrigger");
				UnityEngine.GameObject childObj = Instantiate(PrefabToLink[prefabIdx]);
				childObj.transform.parent = obj.transform;

				Transform trigger = TriggerScaleAndLinkToParent[i];
				Picus.Utils.GO.Transformer.TransformTo(obj.transform, trigger.parent);
				obj.transform.parent = trigger.parent;

				Picus.Utils.GO.Finder.FindComponentAddIfNotExist<Picus.Utils.SortingLayer>(childObj).SortAllDeepFromParent();

				FbxTrigger script = obj.AddComponent<FbxTrigger>();
				script.Init(trigger, childObj, InheritRotation);
			}
		}
	}
}