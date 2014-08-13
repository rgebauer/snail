using System;
using UnityEngine;
using System.Collections.Generic;

namespace Picus.Utils.GO
{
	public class Bounds
	{
		public enum BoundsType
		{
			Sprite,
			Collider,
			Collider2D
		}

		// !!! this is really slow TODO: GS cache with gameobjecttopinfo
		public static bool BoundsComponent(BoundsType boundsType, UnityEngine.GameObject obj, out UnityEngine.Bounds findedBounds, bool onlyFirst)
		{
			bool finded = false;
			findedBounds = new UnityEngine.Bounds();

			System.Type type = ComponentType(boundsType);
			Component boundComponent = obj.GetComponent(type);

			if (boundComponent != null)
			{
				switch (boundsType)
				{
				case BoundsType.Sprite:
					SpriteRenderer spriteRenderer = boundComponent as SpriteRenderer;
					finded = spriteRenderer != null && spriteRenderer.enabled;
					if (finded)
						findedBounds = spriteRenderer.bounds;
					break;
				case BoundsType.Collider:
					Collider collider = boundComponent as Collider;
					finded = collider != null && collider.enabled;
					if (finded)
						findedBounds = collider.bounds;
					break;
				case BoundsType.Collider2D:
					BoxCollider2D boxCollider = boundComponent as BoxCollider2D;
					if (boxCollider)
					{
						finded = boxCollider.enabled;
						if (finded)
						{
							Vector3 size = new Vector3(boxCollider.size.x * obj.transform.lossyScale.x, boxCollider.size.y * obj.transform.lossyScale.y, 0);
							Vector3 center = boxCollider.center;
							center += obj.transform.position;
							findedBounds = new UnityEngine.Bounds(center, size);
						}
					}
					else
					{
						CircleCollider2D circleCollider = boundComponent as CircleCollider2D;
						if (circleCollider)
						{
							finded = circleCollider.enabled;
							if (finded)
							{
								Vector3 size = new Vector3(circleCollider.radius * obj.transform.lossyScale.x, circleCollider.radius * obj.transform.lossyScale.y, 0);
								Vector3 center = boxCollider.center;
								center += obj.transform.position;
								findedBounds = new UnityEngine.Bounds(center, size);
							}
						}
						else
						{
							Picus.Sys.Debug.Throw("GameObject.BoundsComponent unimplemented 2d collider. Ignoring this collider.", true);
							finded = false;
							break;
						}
					}
					break;
				default:
					throw new System.NotImplementedException();
				}
			}

			int count = obj.transform.childCount;
			for(int i = 0; i < count; i++)
			{
				if (finded && onlyFirst) return finded;
				
				UnityEngine.GameObject child = obj.transform.GetChild(i).gameObject;
				UnityEngine.Bounds childBounds;
				
				if (BoundsComponent(boundsType, child, out childBounds, onlyFirst)) 
				{
					if (finded) 
						findedBounds.Encapsulate(childBounds);
					else 
						findedBounds = childBounds;
					
					finded = true;
				}
			}
			
			return finded;
		}	

		private static System.Type ComponentType(BoundsType boundsType)
		{
			switch(boundsType)
			{
			case BoundsType.Collider:
				return typeof(Collider);
			case BoundsType.Collider2D:
				return typeof(Collider2D);
			case BoundsType.Sprite:
				return typeof(SpriteRenderer);
			}

			throw new System.NotImplementedException();
		}
	}
}

