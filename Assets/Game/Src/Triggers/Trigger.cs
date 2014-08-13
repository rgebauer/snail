using UnityEngine;
using System.Collections;

namespace Triggers
{
	public class Trigger : Picus.MonoBehaviourExtend {

		void OnTriggerEnter(Collider other) 
		{
			if(other.tag == "Player")
			{
				Characters.PlayerCtrl player = other.GetComponent<Characters.PlayerCtrl>();
				if(player != null)
				{
					player.ProcessTrigger(this.gameObject);
				}
			}
		}
	}
}