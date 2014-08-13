using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {


	public int PlayingTime = 10;
	public int ActualTime { get; private set; }

	public void IncTime(int value)
	{
		PlayingTime += value;
	}


	// Use this for initialization
	void Start () {
		ActualTime = PlayingTime;
	}

	// Update is called once per frame
	void Update () {
		ActualTime = PlayingTime - (int)Time.timeSinceLevelLoad;

		if(ActualTime < 0) 
		{
			ActualTime = 0;
		}
	}
}
