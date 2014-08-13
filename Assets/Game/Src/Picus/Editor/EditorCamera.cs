using UnityEngine; 
using UnityEditor; 
using System.Collections; 
using System.Reflection; 

namespace Picus.Editor
{
	// TODO: GS fov, distance, ortho. Or just get all params

	// based on http://www.olivereberlei.com/517/wrestling-with-the-editor-camera-in-unity/
	[ExecuteInEditMode] 
	[CustomEditor(typeof(Camera))]
	public class EditorCamera : UnityEditor.Editor
	{ 
		private GameObject _rootGameObj;
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			Camera templateCam = _rootGameObj.GetComponent<Camera>();
			if (templateCam != null && GUILayout.Button("Apply settings to viewport camera"))
				EditorCamera.CopySettings(templateCam);
		}
		
		private void OnEnable()
		{
			_rootGameObj = (target as Camera).gameObject;
		}

		public static void CopySettings(Camera template)
		{
			SetPosition(template.transform.position);
			SetRotation(template.transform.rotation);
//			SetOrthographic(template.orthographic);

		}

		public static void SetPosition(Vector3 newPosition, SceneView sceneView = null) 
		{ 
			SetEditorCameraValue<Vector3>("m_Position", newPosition, sceneView); 
		}  

		public static void SetRotation(Quaternion newRotation, SceneView sceneView = null) 
		{ 
			SetEditorCameraValue<Quaternion>("m_Rotation", newRotation, sceneView); 
		} 

		public static void SetSize(float newSize, SceneView sceneView = null) 
		{ 
			SetEditorCameraValue<float>("m_Size", newSize, sceneView); 
		}  

		public static void SetOrthographic(bool newOrthographic, SceneView sceneView = null) 
		{ 
			SetEditorCameraValue<float>("m_Ortho", (newOrthographic == true) ? 1f : 0f, sceneView); 
		}  

		public static float GetSize(SceneView sceneView = null) 
		{ 
			return GetEditorCameraValue<float>("m_Size", sceneView); 
		}  

		public static Vector3 GetPosition(SceneView sceneView = null) 
		{ 
			return GetEditorCameraValue<Vector3>("m_Position", sceneView); 
		}  

		public static Quaternion GetRotation(SceneView sceneView = null) 
		{ 
			return GetEditorCameraValue<Quaternion>("m_Rotation", sceneView); 
		}  

		public static bool GetOrthographic(SceneView sceneView = null) 
		{ 
			return GetEditorCameraValue<float>("m_Ortho", sceneView) == 1f; 
		}  

		public static SceneView GetActiveSceneView() 
		{ 
			if(EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof(SceneView)) 
				return (SceneView)EditorWindow.focusedWindow;
			ArrayList temp = SceneView.sceneViews;  
			return (SceneView)temp[0]; 
		}  

		static T GetEditorCameraValue<T>(string fieldName, SceneView sceneView = null) 
		{ 
			FieldInfo field = typeof(SceneView).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);  
			object animBool = field.GetValue((sceneView != null) ? sceneView : GetActiveSceneView());  
			FieldInfo field2 = animBool.GetType().GetField("m_Value", BindingFlags.Instance | BindingFlags.NonPublic);  
			return (T)field2.GetValue(animBool); 
		}  

		static void SetEditorCameraValue<T>(string fieldName, T newValue, SceneView sceneView) 
		{ 
			FieldInfo field;
			field = typeof(SceneView).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);  
			if (sceneView == null)
				sceneView = GetActiveSceneView();
			object animBool = field.GetValue(sceneView);  
			FieldInfo field2 = animBool.GetType().GetField("m_Value", BindingFlags.Instance | BindingFlags.NonPublic);  
			T currentValue = (T)field2.GetValue(animBool);  
			object[] param = new object[ 2 ]; 
			param[ 0 ] = newValue; 
			param[ 1 ] = currentValue;  
			Debug.Log("SetEditorCameraValue " + fieldName + " from " + currentValue + " to " + newValue);
			animBool.GetType().GetMethod("BeginAnimating", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(animBool, param); 
		} 
	}
}