using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using Picus.Utils;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace Picus.Utils
{
	public class SortingLayerList : Picus.MonoBehaviourExtend
	{
		public const string RESOURCES_PATH = "Game/Resources/";
		public const string PREFAB_PATH = "Assets/" + RESOURCES_PATH;
		public const string PREFAB_NAME = "SortingLayerList";

		[System.Serializable]
		public class Layer
		{
			public Layer(int uniqueId, int userId, string name)
			{
				UniqueId = uniqueId;
				Name = name;
				UserId = userId;
			}
		
			public string Name;
			public int UniqueId;
			public int UserId;

		}

		[SerializeField][HideInInspector]
		private List<Layer> LayerList;

		public static SortingLayerList Load()
		{
			string prefabPath = RESOURCES_PATH + PREFAB_NAME;
			GameObject prefab = Picus.Sys.ResourceManager.Instance.Load<GameObject>(prefabPath);
			Debug.Assert(prefab != null, "Prefab " + prefabPath + " not found. Use in UnityEditor/Window/SaveSortingLayers at first.");

			SortingLayerList list = prefab.GetComponent<SortingLayerList>();
			return list;
		}	

		public List<string> GetNames()
		{
			List<string> nameList = new List<string>(LayerList.Count);

			for (int i = 0; i < LayerList.Count; ++i)
			{
				nameList.Add(LayerList[i].Name);
			}

			return nameList;
		}

		public Layer GetAt(int index)
		{
			Debug.Assert(index < LayerList.Count);

			return LayerList[index];
		}

		public Layer Get(string layerName)
		{
			return LayerList.Find(x => x.Name == layerName);
		}

		public Layer Get(int layerUserId)
		{
			return LayerList.Find(x => x.UserId == layerUserId);
		}

		public int GetIndex(int layerUserId)
		{
			return LayerList.FindIndex(x => x.UserId == layerUserId);
		}


#if UNITY_EDITOR
		[ExecuteInEditMode]
		public void Init()
		{
			Renderer renderer = gameObject.AddComponent<SpriteRenderer>();

			LayerList = new List<Layer>();

			List<string> layerNames = new List<string>(GetSortingLayerNames());
			List<int> uniqueIds = new List<int>(GetSortingLayerUniqueIDs());

			Debug.Assert(layerNames.Count == uniqueIds.Count);

			for (int i = 0; i < layerNames.Count; ++i)
			{
				renderer.sortingLayerName = layerNames[i];
				Layer layer = new Layer(uniqueIds[i], renderer.sortingLayerID ,layerNames[i]);
	
				LayerList.Add(layer);
			}

		
			DestroyImmediate (renderer);
		}



		[System.Diagnostics.Conditional("DEBUG")]
		public void PrintLayers()
		{
			Debug.Log ("SortingLayerList.Print()");
			for (int i = 0; i < LayerList.Count; ++i)
			{
				Debug.Log ("Layer " + i + ": " + LayerList[i].Name + " " + LayerList[i].UniqueId + " " + LayerList[i].UserId);
			}
		}

		[ExecuteInEditMode]
		private static string[] GetSortingLayerNames() 
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}

		// Get the unique sorting layer IDs -- tossed this in for good measure
		[ExecuteInEditMode]
		private static int[] GetSortingLayerUniqueIDs() 
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
		}
#endif
	
	}
}