using UnityEditor; 
using UnityEngine;

namespace Picus.Editor
{
	[ExecuteInEditMode] 
	[CustomEditor(typeof(Utils.GameObjectEditorInfo))]
	public class GameObjectEditorInfo : UnityEditor.Editor
	{ 
		private Transform _rootTransform;

		public override void OnInspectorGUI()
		{
			if (_rootTransform == null)
				return;

			Vector3 pos = EditorGUILayout.Vector3Field("World pos", _rootTransform.position);
			if (pos != _rootTransform.position)
			{
				_rootTransform.position = pos;
			}

			Vector3 rotDeg = EditorGUILayout.Vector3Field("World rot", _rootTransform.rotation.eulerAngles);
			if (rotDeg != _rootTransform.rotation.eulerAngles)
			{
				Quaternion rotEueler = Quaternion.identity;
				rotEueler.eulerAngles = rotDeg;
				_rootTransform.rotation = rotEueler;
			}

			Vector3 loosyScale = EditorGUILayout.Vector3Field("World scale", _rootTransform.lossyScale);
			if (loosyScale != _rootTransform.lossyScale)
			{
				Vector3 divScale = new Vector3(_rootTransform.localScale.x == 0 ? 1 : _rootTransform.lossyScale.x / _rootTransform.localScale.x, 
				                               _rootTransform.localScale.y == 0 ? 1 : _rootTransform.lossyScale.y / _rootTransform.localScale.y, 
				                               _rootTransform.localScale.z == 0 ? 1 : _rootTransform.lossyScale.z / _rootTransform.localScale.z);
				Vector3 newLocalScale = new Vector3 (loosyScale.x / divScale.x, loosyScale.y / divScale.y, loosyScale.z / divScale.z);

				_rootTransform.localScale = newLocalScale;
			}
		}

		private void OnEnable()
		{
			_rootTransform = (target as Utils.GameObjectEditorInfo).transform;
		}
	/*
		void OnSelectionChange() 
		{
			if (Selection.activeObject is MonoBehaviour) 
				_rootTransform = (Selection.activeObject as MonoBehaviour).transform;
			else
				_rootTransform = null;
		}
		*/
	}
}