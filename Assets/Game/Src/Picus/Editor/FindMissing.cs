using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;

namespace Picus.Editor
{
	public class FindMissing : EditorWindow 
	{
		static int _goCnt = 0, _componentsCnt = 0, _missingCnt = 0;

		enum MissingType
		{
			Script,
			LinkInScript,
			AudioLinkInScript
		}

		[MenuItem("Window/Find Missing")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(FindMissing));
		}
		
		public void OnGUI()
		{
			GUILayout.Label("In selected game objects find recursively: ");
			if (GUILayout.Button("Missing Scripts"))
			{
				FindInSelected(MissingType.Script);
			}

			if (GUILayout.Button("Broken Links"))
			{
				FindInSelected(MissingType.LinkInScript);
			}

			if (GUILayout.Button("Broken Audio Links"))
			{
				FindInSelected(MissingType.AudioLinkInScript);
			}
		}

		private static void FindInSelected(MissingType missing)
		{
			GameObject[] gos = Selection.gameObjects;

			_goCnt = 0;
			_componentsCnt = 0;
			_missingCnt = 0;

			foreach (GameObject go in gos)
			{
				FindInGO(go, missing);
			}

			Debug.Log(string.Format("Searched for missing " + missing + " {0} GameObjects, {1} components, found {2} missing", _goCnt, _componentsCnt, _missingCnt));
		}
		
		private static void FindInGO(GameObject go, MissingType missing)
		{
			_goCnt++;
			Component[] components = go.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				_componentsCnt++;
				Component comp = components[i];
				if (comp == null)
				{
					if (missing == MissingType.Script)
					{
						_missingCnt++;
						Debug.Log(FullPathToGO(go) + " has an empty script attached in position: " + i, go);
					}
				}
				else
				{
					if (missing == MissingType.LinkInScript || missing == MissingType.AudioLinkInScript)
					{
						FieldInfo[] fields = comp.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

						int fiCnt = fields.Length;
						for (int fiIdx = 0; fiIdx < fiCnt; ++fiIdx)
						{
							FieldInfo fi = fields[fiIdx];
							System.Object value = fi.GetValue(comp) as System.Object;

							if (missing == MissingType.AudioLinkInScript)
							{
								System.Type elementType = fi.FieldType.GetElementType();
								if (fi.FieldType != typeof(AudioClip) && !(fi.FieldType.IsArray && elementType == typeof(AudioClip)))
									continue;
							}

							if (IsBroken(value))
							{
								_missingCnt++;
								Debug.Log(FullPathToGO(go) + " component " + comp.GetType() + " has broken link for " + fi.Name + " (" + fi.FieldType + ")", go);
							}
							else if (value != null && !value.Equals(null))
							{
								if (fi.FieldType.IsSubclassOf(typeof(IList)) || fi.FieldType.IsSubclassOf(typeof(System.Array)))
								{
									IList list = value as IList;
									int listCnt = list.Count;
									for (int listIdx = 0; listIdx < listCnt; ++listIdx)
									{
										System.Object valueInList = list[listIdx];
										if (IsBroken(valueInList))
										{
											_missingCnt++;
											Debug.Log(FullPathToGO(go) + " component " + comp.GetType() + " has broken link in list " + fi.Name + " (" + fi.FieldType + ") on position " + listIdx, go);
										}
									}
								}
							}
						}
					}
				}
			}

			// Now recurse through each child GO (if there are any):
			foreach (Transform childT in go.transform)
			{
				//Debug.Log("Searching " + childT.name  + " " );
				FindInGO(childT.gameObject, missing);
			}
		}

		private static bool IsBroken(System.Object obj)
		{
			if (obj == null || !obj.Equals(null))
				return false;

			System.Type type = obj.GetType();

			FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);


			foreach(FieldInfo fi in fields)
			{
				try
				{
					System.Object value = fi.GetValue(obj) as System.Object;
					value.Equals(null); // touch
				}
				catch (System.Exception e)
				{
					if (e.GetType() == typeof(UnityEngine.MissingReferenceException))
						return true;
				}
			}

			foreach(PropertyInfo pi in properties) // sprite use only properties
			{
				try
				{
					System.Object value = pi.GetValue(obj, null) as System.Object;
					value.Equals(null); // touch
				}
				catch (System.Exception e)
				{
					if (e.GetType() == typeof(UnityEngine.MissingReferenceException))
						return true;
				}
			}

			return false;
		}

		
		private static string FullPathToGO(GameObject go)
		{
			string s = go.name;
			Transform t = go.transform;
			while (t.parent != null) 
			{
				s = t.parent.name +"/"+s;
				t = t.parent;
			}

			return s;
		}
	}
}