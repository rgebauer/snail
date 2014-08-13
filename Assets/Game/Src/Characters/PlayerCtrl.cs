using UnityEngine;
using System.Collections;

namespace Characters
{
	public class PlayerCtrl : Picus.MonoBehaviourExtend {

		public GameObject Visual;

		public InputCtrl InputController;

		public float MaxMovementSpeed;
		public float MovementAcceleration;

		public float MaxRotationSpeed;
		public float StartRotationSmoothness = 0.2f;
		public float EndRotationSmoothness = 0.2f;
		public Generators.Generator CollectablesGenerator;

		public Timer GameTimer;

		public Transform Nose;

		public bool YouWinFlag { get; private set; }

		public int CollectablesCout { get; private set; }

		float _actualRotationSpeed;

		Rigidbody _rigidBody;
		Vector3 _impulsFromEnemy;


		public void ProcessTrigger(GameObject obj)
		{    
			Debug.Log("Player trigger handled");
			Collectables.Coin coin = obj.GetComponent<Collectables.Coin>();
			if(coin != null)
			{
				CollectablesCout++;
				                                          
				if(CollectablesCout == CollectablesGenerator.Count)
				{
					YouWinFlag = true;
				}

				if(coin.TimeBonus != 0)
				{
					GameTimer.IncTime(coin.TimeBonus);
				}

				if(CollectablesGenerator.GenerateOnPlayerCollision)
				{
					CollectablesGenerator.GenerateNew();
				}
			}

			Triggers.OutOfPlayground outOfPlayground = obj.GetComponent<Triggers.OutOfPlayground>();
			if(outOfPlayground != null && GameTimer.ActualTime > 0 && !YouWinFlag)
			{
				Application.LoadLevel(0);
			}
		}

		public void ProcessCollision(GameObject obj)
		{
			if(obj.tag == "Enemy")
			{
				Debug.Log("Collision with Enemy");
				AI.EnemyAI enemyAi = obj.GetComponent<AI.EnemyAI>();
				float impuls = enemyAi.CollisionImpuls;

				_impulsFromEnemy = enemyAi.gameObject.transform.forward * impuls;
				if(_rigidBody.velocity.magnitude <= MaxMovementSpeed)
				{
					_rigidBody.AddRelativeForce(_impulsFromEnemy);
				}
			}
		}


		void Start()
		{
			Debug.Assert (InputController, "PlayerCtrl.Start() - inputController is MISSING!");
			_rigidBody = Visual.GetComponent<Rigidbody>();
		}


		void FixedUpdate()
		{
			if(GameTimer.ActualTime <= 0)
			{
				return;
			}

			if(InputController.CtrlState == InputCtrl.State.MOVE)
			{
				if(_rigidBody.velocity.magnitude <= MaxMovementSpeed)
				{
					_rigidBody.AddRelativeForce(Vector3.forward * MovementAcceleration);
				}
			}


			if(InputController.CtrlState == InputCtrl.State.ROTATE)
			{
				_actualRotationSpeed = Mathf.Lerp(_actualRotationSpeed, MaxRotationSpeed, StartRotationSmoothness);
			}
			else 
			{
				_actualRotationSpeed = Mathf.Lerp(_actualRotationSpeed, 0, EndRotationSmoothness);
			}


			if(_actualRotationSpeed > 0)
			{
				Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, _actualRotationSpeed * InputController.GetRotationDirection, 0));
				_rigidBody.MoveRotation(_rigidBody.rotation * deltaRotation);
			}
		}
	}
}
