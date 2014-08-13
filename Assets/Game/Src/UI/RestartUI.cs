using UnityEngine;
using System.Collections;

namespace UI
{
	public class RestartUI : MonoBehaviour {

		void OnGUI() 
		{
			float screenScale = Screen.width / 480.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale,screenScale,screenScale));
			GUI.matrix = scaledMatrix;

			if(GUI.Button(new Rect(10, 10, 70, 20), "Restart"))
			{
				Application.LoadLevel(0);
			}
		}


	}
}