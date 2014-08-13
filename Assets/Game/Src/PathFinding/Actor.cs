using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Actor : MonoBehaviour {
	
	public enum State
	{
		IDLE,
		MOVING,
	}

	public State GetState { get { return _state; } }
	
	float _speed;
	public float SpeedMulti = 10;

	public NodeControl ControllNode;
	public bool DebugMode;

	
	bool _onNode = true;
	Vector3 _target = new Vector3(0, 0, 0);
	Vector3 _currNode;
	int _nodeIndex;
	List<Vector3> _path = new List<Vector3>();

	State _state = State.IDLE;
	float _oldTime = 0;
	float _checkTime = 0;
	float _elapsedTime = 0;

	float _positionFixCounter = 0;

	Rigidbody _rigidBody;
	
	void Awake()
	{
		_rigidBody = GetComponent<Rigidbody>();
	}
	
	void Update () 
	{
		_speed = Time.deltaTime * SpeedMulti;
		_elapsedTime += Time.deltaTime;
		
		if (_elapsedTime > _oldTime)
		{
			switch (_state)
			{
			case State.IDLE:
				break;
				
			case State.MOVING:

				//Falcco
				//real time terrain change test - we dont want it in this case
				_oldTime = _elapsedTime + 0.01f;

				if (_elapsedTime > _checkTime)
				{
					_checkTime = _elapsedTime + 1;
					SetTarget();
				}
				
				if (_path != null)
				{
					if (_onNode)
					{
						_onNode = false;
						if (_nodeIndex < _path.Count)
							_currNode = _path[_nodeIndex];
					} else {
						MoveToward();
					}
				
				} else {
						ChangeState(State.IDLE);
				}
				

				break;
			}
		}
	}
	
	void MoveToward()
	{
		if (DebugMode)
		{
			for (int i=0; i<_path.Count-1; ++i)
			{
				UnityEngine.Debug.DrawLine((Vector3)_path[i], (Vector3)_path[i+1], Color.white, 0.01f);
			}
		}
		
		Vector3 newPos = transform.position;

		float Xdistance = newPos.x - _currNode.x;
		if (Xdistance < 0) Xdistance -= Xdistance*2;
		float Zdistance = newPos.z - _currNode.z;
		if (Zdistance < 0) Zdistance -= Zdistance*2;
	
		if ((Xdistance < 0.1 && Zdistance < 0.1) && _target == _currNode) //Reached target
		{
			ChangeState(State.IDLE);
		}
		else if (Xdistance < 0.1 && Zdistance < 0.1)
		{
			_nodeIndex++;
			_onNode = true;
		}
		
		/***Move toward waypoint***/
		Vector3 motion = _currNode - newPos;
		motion.Normalize();
		newPos += motion * _speed;

//		if(_rigidBody != null) 
//		{
//			_rigidBody.MovePosition(newPos);
//		}
//		else 
//		{
			transform.position = newPos;
//		}

		if(transform.position == newPos)
		{
			_positionFixCounter++;
			if(_positionFixCounter == 50)
			{
				ChangeState(State.IDLE);
			}
		}

	}
	
	private void SetTarget()
	{
		_path = ControllNode.Path(transform.position, _target);
		_nodeIndex = 0;
		_onNode = true;
	}
	
	public void MoveOrder(Vector3 pos)
	{
		_positionFixCounter = 0;
		_target = pos;
		SetTarget();
		ChangeState(State.MOVING);
	}
	
	private void ChangeState(State newState)
	{
		_state = newState;
	}
}
