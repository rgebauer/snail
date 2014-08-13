using UnityEngine;
using System.Collections;

namespace UI
{
	public class TimeUI : MonoBehaviour {

		public Timer GameTimer;
		public Characters.PlayerCtrl Player;

		void OnGUI() 
		{
			float screenScale = Screen.width / 480.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale,screenScale,screenScale));
			GUI.matrix = scaledMatrix;

			GUI.Label(new Rect(300, 10, 70, 20), "Time:" + GameTimer.ActualTime);

			if(GameTimer.ActualTime <= 0 && !Player.YouWinFlag) 
			{
				GUI.Label(new Rect(200, 100, 200, 20), "TIME IS UP!!!");
				if(GUI.Button(new Rect(200, 150, 70, 20), "Restart"))
				{
					Application.LoadLevel(0);
				}
			}
		}
	}
}