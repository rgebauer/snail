using UnityEngine;
using UnityEditor;
using System.Reflection; 
using System.Linq;

namespace Picus.Editor
{
	public class ParticleScale : EditorWindow
	{
		private float _scale = 1;

		[MenuItem("GameObject/Scale Deep Particles")]
		private static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(ParticleScale));
		}

		public void OnGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			_scale = EditorGUILayout.FloatField("Future scale", _scale);

			EditorGUILayout.Space();

			if (GUILayout.Button("Set Scale on all Particles in selected GameObjects"))
			{
				if (EditorUtility.DisplayDialog("Are you sure?", "All particle parameters in all children will be multiplied by Future scale. This is undoable operation!", "Yes", "No"))
				{
					ScaleInSelected(_scale);
					_scale = 1;
					GUI.FocusControl("HackButtonForRemovingFocusFromTextField");
				}
			}

			EditorGUILayout.Space();
			GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
			EditorGUILayout.Space();

			GUI.SetNextControlName("HackButtonForRemovingFocusFromTextField");
			if (GUILayout.Button("Play all Particles on selected GameObjects."))
			{
				PlayAllParticles();
			}
		}
		
		private static void ScaleInSelected(float scale)
		{
			GameObject[] objs = Selection.gameObjects;
			foreach (GameObject obj in objs)
			{
				ParticleSystem particleSystem = obj.GetComponent<ParticleSystem>();
				if (particleSystem)
				{
					particleSystem.startSize = particleSystem.startSize * scale;
					particleSystem.startSpeed = particleSystem.startSpeed * scale;
					// TODO: GS other values
	/*
					SetValue<float>(particleSystem, "startSize", particleSystem.startSize * scale); 
					SetValue<float>(particleSystem, "startSpeed", particleSystem.startSpeed * scale); 
					*/
				}
			}
		}

		private static void PlayAllParticles()
		{
			GameObject[] objs = Selection.gameObjects;
			foreach (GameObject obj in objs)
			{
				ParticleSystem particleSystem = obj.GetComponent<ParticleSystem>();
				if (particleSystem)
				{
					particleSystem.Stop(true);
					particleSystem.Clear(true);
					particleSystem.Play(true);
				}
			}
		}

		private static T GetValue<T>(object obj, string fieldName)
		{
			System.Type type = typeof(T);
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			FieldInfo field = type.GetField(fieldName, bindFlags);
			object ret = field.GetValue(obj);
			return (T) System.Convert.ChangeType(ret, typeof(T));
		}
		
		private static void SetValue<T>(object obj, string fieldName, T newValue) 
		{ 
			System.Type type = typeof(T);
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic| BindingFlags.Static;
			FieldInfo field = type.GetField(fieldName, bindFlags);
			Picus.Sys.Debug.Assert(field != null, "SetValue (" + newValue.GetType() + ") " + fieldName + " = " + newValue + "\nAvailable fields: " + System.String.Join(" ", type.GetFields().Select(x => x.Name).ToArray()));
			field.SetValue(obj, System.Convert.ChangeType(newValue, field.FieldType));
		} 
	}
}