using UnityEngine;
using System.Collections;

namespace AI
{

	public class EnemyAI : MonoBehaviour {


		public Actor ActorScript;
		public Generators.TargetArea Area;
		public Characters.PlayerCtrl Player;
		public LayerMask GeneratedPositionLayerMask;

		public float CollisionImpuls = 1.0f;
		public float AttackPlayerChance = 50.0f;

		void OnCollisionEnter(Collision collision)
		{
			Characters.PlayerCtrl player = collision.gameObject.GetComponent<Characters.PlayerCtrl>();
			if(player != null)
			{
				player.ProcessCollision(this.gameObject);
			}
		}

		void Update()
		{
			if(ActorScript.GetState == Actor.State.MOVING)
			{
				return;
			}

			if(Random.Range(0f, 1.0f) > (AttackPlayerChance/100))
			{
				ActorScript.MoveOrder(GetRandomPosition());
			}
			else
			{
				ActorScript.MoveOrder(new Vector3(Player.transform.position.x,
				                                  transform.position.y,
				                                  Player.transform.position.z));
			}
		}


		Vector3 GetRandomPosition()
		{
			Vector3 result = Generators.Generator.GeneratePosition(Area.transform.position, Area.GetSize, Area.AvailableAxes);
			while(Generators.Generator.IntersectionTest(result, 0.5f, GeneratedPositionLayerMask))
			{
				result = Generators.Generator.GeneratePosition(Area.transform.position, Area.GetSize, Area.AvailableAxes);
			}
			//dont change Y axe
			result.y = transform.position.y;
			return result;
		}


	}
}