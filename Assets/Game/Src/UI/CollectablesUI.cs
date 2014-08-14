using UnityEngine;
using System.Collections;

namespace UI
{
	public class CollectablesUI : MonoBehaviour {

		public Characters.PlayerCtrl Player;
		public Generators.Generator CollectablesGenerator;

		void OnGUI() 
		{
			/*
			float screenScale = Screen.width / 480.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale,screenScale,screenScale));
			GUI.matrix = scaledMatrix;
			*/

			if(Player != null)
			{
				GUI.Label(new Rect(Screen.width/2, Screen.height/10, Screen.width/2, Screen.height/10), "Count: " + Player.CollectablesCout + "/" + CollectablesGenerator.Count);
			}
		}
	}
}