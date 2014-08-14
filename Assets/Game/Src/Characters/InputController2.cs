using UnityEngine;
using System.Collections;

namespace Characters
{

	public class InputController2 : InputCtrl {

		protected override void ProcessInput()
		{
			if(Input.GetKey(ControllKey) || Input.touchCount > 0)
			{
				CtrlState = State.ROTATE;
			} 
			else 
			{
				CtrlState = State.MOVE;
			}
		}


	}

}