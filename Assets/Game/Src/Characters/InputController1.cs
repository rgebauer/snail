using UnityEngine;
using System.Collections;

namespace Characters
{

	public class InputController1 : InputCtrl {

		protected override void ProcessInput()
		{
			if(Input.GetKey(ControllKey))
			{
				CtrlState = State.MOVE;
			} 
			else 
			{
				CtrlState = State.ROTATE;
			}
		}
	}

}