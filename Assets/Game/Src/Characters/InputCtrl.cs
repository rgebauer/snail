using UnityEngine;
using System.Collections;

namespace Characters
{

	abstract public class InputCtrl : MonoBehaviour {

		public enum State : int
		{
			MOVE,
			ROTATE
		}

		public int GetRotationDirection { get { return _rotationDirection; } }
		public State CtrlState { get; protected set; }
		public KeyCode ControllKey;
		public bool SwitchingRotateDirection = false;
		public bool StateChange = false;


		State _state = State.MOVE;
		State _previousState = State.MOVE;
		int _rotationDirection = 1;

		protected virtual void ProcessInput()
		{}

		protected virtual void StateChanged(State oldState, State newState)
		{
			if(SwitchingRotateDirection)
			{
				Debug.Log("---- state changed");
				if(oldState == State.ROTATE && newState == State.MOVE)
				{
					_rotationDirection *= (-1);
				}
			}

			StateChange = true;
		}

		void FixedUpdate()
		{
			StateChange = false;

			_previousState = CtrlState;

			ProcessInput();

			if(_previousState != CtrlState)
			{
				StateChanged(_previousState, CtrlState);
			}
		}


	}
}