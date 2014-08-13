using UnityEngine;
using System.Collections;

namespace UI
{
	public class YouWinUI : MonoBehaviour {
		
		public Timer GameTimer;
		public Characters.PlayerCtrl Player;

		void OnGUI() 
		{
			/*
			float screenScale = Screen.width / 480.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale,screenScale,screenScale));
			GUI.matrix = scaledMatrix;
			*/

			if(GameTimer.ActualTime >= 0 && Player.YouWinFlag) 
			{
				GUI.Label(new Rect(200, 100, 200, 20), "YOU WIN!!!");
				if(GUI.Button(new Rect(200, 150, 70, 20), "Restart"))
				{
					Application.LoadLevel(0);
				}
			}
		}
	}
}