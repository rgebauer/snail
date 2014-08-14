using UnityEngine;
using System.Collections;

namespace UI
{
	public class TimeUI : MonoBehaviour {

		public Timer GameTimer;
		public Characters.PlayerCtrl Player;

		public Font font;

		void OnGUI() 
		{
			/*
			float screenScale = Screen.width / 480.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale,screenScale,screenScale));
			GUI.matrix = scaledMatrix;
			*/
			GUI.skin.font = font;

			GUI.Label (new Rect (0, Screen.height / 10, Screen.width / 2, Screen.height / 10), "Level: " + (Application.loadedLevel + 1));
			GUI.Label (new Rect (0, Screen.height / 10, Screen.width, Screen.height / 10), "Time: " + GameTimer.ActualTime);

			if(GameTimer.ActualTime <= 0 && !Player.YouWinFlag) 
			{
				GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
				centeredStyle.alignment = TextAnchor.UpperCenter;

				GUIStyle centeredStyleButton = GUI.skin.GetStyle("Button");  
				centeredStyle.alignment = TextAnchor.UpperCenter;

				GUI.Label(new Rect(0, Screen.height/2, Screen.width, Screen.height/10), "TIME IS UP!!!", centeredStyle);
				if(GUI.Button(new Rect(0, Screen.height/2+Screen.height/10, Screen.width, Screen.height/10), "Restart", centeredStyleButton))
				{
					Application.LoadLevel(Application.loadedLevel);
				}
			}
		}
	}
}