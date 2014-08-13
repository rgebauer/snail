using UnityEngine;
using System.Collections;

public class SmoothCamFollow : Picus.MonoBehaviourExtend {

	public Transform Target;
	public float SmoothFactor = 5.0f;

	Vector3 _offset;

	// Use this for initialization
	void Start () {
		_offset = transform.position - Target.transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {

		transform.position = Vector3.Lerp(transform.position, Target.transform.position + _offset, SmoothFactor * Time.deltaTime);;
	}
}
