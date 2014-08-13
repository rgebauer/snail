using UnityEngine;
using System.Collections;

namespace UI
{
	public class CollectablesUI : MonoBehaviour {

		public Characters.PlayerCtrl Player;

		void OnGUI() 
		{
			float screenScale = Screen.width / 480.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale,screenScale,screenScale));
			GUI.matrix = scaledMatrix;

			if(Player != null)
			{
				GUI.Label(new Rect(200, 10, 70, 20), "Count:" + Player.CollectablesCout);
			}
		}
	}
}