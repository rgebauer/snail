using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Picus.Editor
{
	public class MyShortcuts : UnityEditor.Editor
	{
		[MenuItem("GameObject/ActiveToggle %e")] // ctrl + e
		static void ToggleActivationSelection()
		{
			GameObject[] objs = Selection.gameObjects;
			foreach (GameObject obj in objs)
			{
				obj.SetActive(!obj.activeSelf);
			}
		}

		[MenuItem("GameObject/ActiveToggle %g")] // ctrl + a
		static void ApplyPrefabSelection()
		{			
			List<Object> savedPrefabs = new List<Object>();

			GameObject[] objs = Selection.gameObjects;
			if (objs.Length > 1 && (!EditorUtility.DisplayDialog("Are you sure?", "Are you sure to apply changes to multiple (" + objs.Length + ") gameObjects?", "Yes", "No"))) // TODO: GS implement multi selection
					return;

   			foreach (GameObject obj in objs)
			{
				GameObject instanceRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(obj);
				Object currentPrefab = UnityEditor.PrefabUtility.GetPrefabParent(instanceRoot);
				
				if (savedPrefabs.Contains(instanceRoot))
					continue;

				savedPrefabs.Add(instanceRoot);
				PrefabUtility.ReplacePrefab(instanceRoot, currentPrefab, ReplacePrefabOptions.ConnectToPrefab);
			}
		}

	}
}