using UnityEngine;
using System.Collections;

namespace UI
{
	public class NextLevelUI : MonoBehaviour {

		void OnGUI() 
		{
			/*
			float screenScale = Screen.width / 480.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale,screenScale,screenScale));
			GUI.matrix = scaledMatrix;
			*/
			if(GUI.Button(new Rect(Screen.width/2, 0, Screen.width/2, Screen.height/10), "Next level"))
			{
				Application.LoadLevel((Application.loadedLevel+1)%Application.levelCount);
			}
		}


	}
}