using UnityEngine;
using System.Collections;

namespace AI
{

	public class EnemyNewAI : MonoBehaviour {


		public Characters.PlayerCtrl Player;

		public float CollisionImpuls = 100.0f;
		public float AttackPlayerChance = 50.0f;
		public float walkRadius = 10.0f;

		NavMeshAgent agent;

		enum State {
			IDLE,
			WALK,
			ATTACK,
			DEFEND
		};
		State state;

		void Start() {
			agent = GetComponent<NavMeshAgent> ();
			state = State.IDLE;
		}

		void OnCollisionEnter(Collision collision)
		{
			Characters.PlayerCtrl player = collision.gameObject.GetComponent<Characters.PlayerCtrl>();
			if(player != null)
			{
				player.ProcessCollision(this.gameObject);

				Vector3 finalPosition = GetRandomPosition();
				agent.SetDestination(finalPosition);
#if UNITY_EDITOR
				UnityEngine.Debug.DrawLine(transform.position, finalPosition, Color.green, 0.01f);
#endif
				state = State.WALK;
			}
		}

		void Update()
		{
			doAI ();
		}


		//FSM Enemy AI
		public void doAI() 
		{
			switch (state) 
			{
				case State.IDLE:
				{
					Vector3 finalPosition;
					if(Random.Range(0f, 1.0f) > (AttackPlayerChance/100))
					{
						finalPosition = GetRandomPosition();
						state = State.WALK;
					}
					else
					{
						finalPosition = new Vector3(Player.transform.position.x, 0, Player.transform.position.z);
						state = State.ATTACK;
					}
					
					agent.SetDestination(finalPosition);

#if UNITY_EDITOR
					UnityEngine.Debug.DrawLine(transform.position, finalPosition, Color.white, 0.01f);
#endif
				}

				break;

				case State.ATTACK: 
				{
					Vector3 playerDirection = Player.transform.position - transform.position;
					playerDirection.Normalize();

					Vector3 finalPosition = Player.transform.position + playerDirection;

					agent.SetDestination(finalPosition);

#if UNITY_EDITOR
					UnityEngine.Debug.DrawLine(transform.position, finalPosition, Color.red, 0.01f);
#endif
				}
				break;

				case State.WALK:
				{
					if (agent.velocity.sqrMagnitude <= 1) 
					{
						Vector3 finalPosition = GetRandomPosition();
						agent.SetDestination(finalPosition);
#if UNITY_EDITOR
						UnityEngine.Debug.DrawLine(transform.position, finalPosition, Color.green, 0.01f);
#endif
					}
				}
				break;

				case State.DEFEND:
				{
					state = State.IDLE;
				}
				break;
			}
		}


		Vector3 GetRandomPosition()
		{
			Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
			randomDirection.y = 0;

			randomDirection += transform.position;

			NavMeshHit hit;

			NavMesh.SamplePosition (randomDirection, out hit, walkRadius, 1);
			Vector3 finalPosition = hit.position;

			return finalPosition;
		}


	}
}