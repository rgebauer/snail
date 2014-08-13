using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;

namespace Picus.Editor
{
	public class SaveSortingLayers : EditorWindow 
	{
		[MenuItem("Window/SaveSortingLayers")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(SaveSortingLayers));
		}
		
		public void OnGUI()
		{
			GUILayout.Label("Save Sorting Layers ");
			
			if (GUILayout.Button("Save"))
			{
				Save();
			}
		}

		private void Save()
		{
			string prefabPath = Picus.Utils.SortingLayerList.PREFAB_PATH + Picus.Utils.SortingLayerList.PREFAB_NAME + ".prefab";

			Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);

			GameObject go = new GameObject(Picus.Utils.SortingLayerList.PREFAB_NAME);

			Picus.Utils.SortingLayerList sortingLayerList = go.AddComponent<Picus.Utils.SortingLayerList>();

			sortingLayerList.Init();

			PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.Default);

			DestroyImmediate(go);
		}
	}

}