using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public GameObject Actor;
	public string FloorTag;

	RaycastHit _hit;
	bool _leftClickFlag = true;
	Actor _actorScript;
	
	void Start()
	{
		if (Actor != null)
		{
			_actorScript = (Actor)Actor.GetComponent(typeof(Actor));
		}
	}
	
	void Update () 
	{
		/***Left Click****/
		if (Input.GetKey(KeyCode.Mouse0) && _leftClickFlag)
			_leftClickFlag = false;
		
		if (!Input.GetKey(KeyCode.Mouse0) && !_leftClickFlag)
		{
			_leftClickFlag = true;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out _hit, 100))
			{
				if (_hit.transform.tag == FloorTag)
				{
					float X = _hit.point.x;
					float Y = _hit.point.y;
					float Z = _hit.point.z;

					//Falcco
					Vector3 target = new Vector3(X, Actor.transform.position.y, Z); //original line
//					Vector3 target = new Vector3(X, Y, Actor.transform.position.z); //Falcco`s line
					
					_actorScript.MoveOrder(target);
				}
			}
		}
	}
}
