using UnityEngine;
using System.Collections;

namespace Collectables
{
	public class Coin : Collectable {

		public int TimeBonus = 5;

		void OnTriggerEnter(Collider other) 
		{
			if(other.tag == "Player")
			{
				Destroy(this.gameObject);
				Characters.PlayerCtrl player = other.GetComponent<Characters.PlayerCtrl>();
				if(player != null)
				{
					player.ProcessTrigger(this.gameObject);
				}
			}
		}
	}
}