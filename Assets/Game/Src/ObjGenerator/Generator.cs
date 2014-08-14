using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Generators
{
	public class Generator : Picus.MonoBehaviourExtend {

		public LayerMask IntersectionLayerMask;
		public GameObject ObjectsContainer;
		public GameObject ObjectPrefab;
		public int Count = 0;
		public int TimeBonus = 0;
		public bool GenerateOnStart = true;
		public bool GenerateOnPlayerCollision = false;

		public List<TargetArea> TargetAreas;

		int _actualCount;


		public void GenerateNew()
		{
			Generate();
		}

		void Start()
		{
			//check collectables
			GameObject[] collectables = GameObject.FindGameObjectsWithTag("Collectable");

			_actualCount = Count;

			Count += collectables.Length;

			if(GenerateOnStart)
			{
				while(_actualCount != 0)
				{
					Generate();
				}
			} 
			else if (collectables.Length == 0)
			{
				Generate();
			}
		}


		void Generate()
		{
			if(_actualCount == 0 || TargetAreas.Count == 0)
			{
				return;
			}

			TargetArea area = TargetAreas[Random.Range(0,TargetAreas.Count)];

			GameObject obj = Instantiate(ObjectPrefab);

			Vector3 position = GeneratePosition(area.transform.position, area.GetSize, area.AvailableAxes);
			while(IntersectionTest(position, 1.0f, IntersectionLayerMask))
			{
				position = GeneratePosition(area.transform.position, area.GetSize, area.AvailableAxes);
			}

			obj.transform.position = position;

			//set time bonus
			Collectables.Coin coinScript = (Collectables.Coin)obj.GetComponent<Collectables.Coin>();
			coinScript.TimeBonus = TimeBonus;

			_actualCount--;
		}

		static public Vector3 GeneratePosition(Vector3 position, Vector3 size, Vector3 availableAxes)
		{
			float x = position.x;
			float y = position.y;
			float z = position.z;
			
			if(availableAxes.x == 1)
			{
				float delta = size.x*0.5f * Random.Range(0.0f, 1.0f);
				delta = (Random.Range(-1.0f,1.0f) < 0) ? delta : (-1.0f * delta);
				x += delta;
			}
			
			if(availableAxes.y == 1)
			{
				float delta = size.y*0.5f * Random.Range(0.0f, 1.0f);
				delta = (Random.Range(-1.0f,1.0f) < 0) ? delta : (-1.0f * delta);
				y += delta;
			}
			
			if(availableAxes.z == 1)
			{
				float delta = size.z*0.5f * Random.Range(0.0f, 1.0f);
				delta = (Random.Range(-1.0f,1.0f) < 0) ? delta : (-1.0f * delta);
				z += delta;
			}

			return new Vector3(x,y,z);
		}

		static public bool IntersectionTest(Vector3 position, float radius, LayerMask mask)
		{
			Collider[] hits = Physics.OverlapSphere(position, radius, mask);
			return (hits.Length > 0);
		}
	}
}
