using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Picus.Utils
{
	/** Solve transforming and alphing by time with dynamic. */
	public class TweenController : Picus.MonoBehaviourExtend
	{
		public struct TweenTransform
		{
			public Vector3 Position { get; private set; }
			public Quaternion Rotation { get; private set; }
			public float LocalScaleMulti { get; private set; }

			public bool ChangePosition { get; private set; }
			public bool ChangeRotation { get; private set; }
			public bool ChangeScale { get; private set; }

			public TweenTransform(Transform transform) : this()
			{ 
				ChangePosition = true; 
				ChangeRotation = true; 
				Position = transform.position; 
				Rotation = transform.rotation; 
				// TODO: scale?
			}

			public TweenTransform(Vector3 position) : this()
			{ 
				Position = position; 
				ChangePosition = true;
			}

			public TweenTransform(Vector3 position, Quaternion rotation) : this()
			{ 
				Position = position; 
				ChangePosition = true;
				Rotation = rotation;
				ChangeRotation = true;
			}

			public TweenTransform(Vector3 position, Quaternion rotation, float localScaleMulti) : this()
			{
				Position = position; 
				ChangePosition = true;
				Rotation = rotation;
				ChangeRotation = true;
				LocalScaleMulti = localScaleMulti;
				ChangeScale = true;
			}

			public TweenTransform(Vector3 position, float localScaleMulti) : this()
			{
				Position = position; 
				ChangePosition = true;
				LocalScaleMulti = localScaleMulti;
				ChangeScale = true;
			}
		}

		public struct TweenDefinition
		{
			public enum TransformType
			{
				NoSmooth, // just speed + accel
				SmoothOut, // Mathf.SmoothDamp wo accel
				SmoothInOut, // Mathf.SmoothStep wo accel
				SmoothIn, // Mathf.SmoothDamp wo accel
			}

			public float StartSpeed;
			public float Acceleration;
//			public float ForTime; // 0 = ignored
			public TransformType Type;

			public TweenDefinition(float startSpeed, TransformType type) : this() {  StartSpeed = startSpeed; Type = type; }
		}

		public bool IsTransforming { get { return _transformingCnt == 0; } }

		private int _movingId;
		private int _rotatingId;
		private int _invisiblingId;
		private int _scalingId;

		private int _transformingCnt = 0;

		private Vector3 _startingScale;


		public void StopAll()
		{
			_movingId++;
			_rotatingId++;
			_invisiblingId++;
			_scalingId++;
		}

		public void ScaleBy(float multi)
		{
			transform.localScale = _startingScale * multi;
		}

		public IEnumerator RotateToTimeCoroutine(Quaternion destRot, float time)
		{
			int rotatingId = ++_rotatingId;

			Quaternion startRot = transform.rotation;
			float factor = 1 / time;
			float t = 0;

			while (rotatingId == _rotatingId && t < 1)
			{
				t = t + Time.deltaTime * factor;
				transform.rotation = Quaternion.Slerp(startRot, destRot, t);
/*
				if (Quaternion.Angle(destRot, transform.rotation) < 0.01f)
				{
					transform.rotation = destRot;
					break;
				}
*/
				yield return null;
			}
		} 

		public IEnumerator ScaleByTimeCoroutine(float multi, float time)
		{
			int scalingId = ++_scalingId;

			Vector3 startScale = transform.localScale;
			Vector3 newScale = _startingScale * multi;
			float factor = 1 / time;
			float t = 0;

			while (scalingId == _scalingId && t < 1)
			{
				t = t + Time.deltaTime * factor;
				transform.localScale = Vector3.Lerp(startScale, newScale, t);

				yield return null;
			}
		}

		public IEnumerator ScaleBySpeedCoroutine(float multi, float speed)
		{
			yield return StartChildCoroutine(ScaleToSpeedCoroutine(_startingScale * multi, speed));
		}

		public IEnumerator ScaleToSpeedCoroutine(Vector3 newScale, float speed)
		{
			int scalingId = ++_scalingId;
			Vector3 startScale = transform.localScale;
			Vector3 delta = (newScale - startScale);
			float originalDistance = delta.magnitude;
			Vector3 deltaCounted = delta;
			deltaCounted.Normalize();
			Vector3 velocity;

			float startTime = Time.time;
			float endTime = startTime + originalDistance / (deltaCounted.magnitude * speed);

			while (scalingId == _scalingId)
			{
				if (Time.time >= endTime)
				{
					transform.localScale = newScale;
					break;
				}

				velocity = Vector3.Lerp(Vector3.zero, deltaCounted, Time.deltaTime);

				velocity = velocity * speed;
				transform.localScale += velocity;

				yield return null;
			}
		}

		/** Make object transparent by time.
		 * @param time Duration.
		 * @param deep Apply to children.
		 * @param ignoreLinked Do not apply to childs with TopInfo script attached (by GO.Linker.LinkTo)
		 */
		public IEnumerator InvisibleTimeCoroutine(float time, bool deep, bool ignoreLinked)
		{
			return InvisibleTimeCoroutine(gameObject, time, deep, true, ignoreLinked);
		}

		/** Make object transparent by time.
		 * @param time Duration.
		 * @param deep Apply to children.
		 */
		public IEnumerator InvisibleTimeCoroutine(float time, bool deep)
		{
			return InvisibleTimeCoroutine(gameObject, time, deep, true, false);
		}

		/** Make object transparent and then untransparent it slowly to original values. */
		public IEnumerator UnInvisibleTimeCoroutine(float time, bool deep)
		{
			return InvisibleTimeCoroutine(gameObject, time, deep, false, false);
		}

		/** Change position, rotation and scale. */
		public IEnumerator TweenToCoroutine(TweenTransform tweenTransform, TweenDefinition definition)
		{
			int movingId = tweenTransform.ChangePosition && tweenTransform.Position != transform.position ? ++_movingId : -1;
			int rotatingId = tweenTransform.ChangeRotation && tweenTransform.Rotation != transform.rotation ? ++_rotatingId : -1;
			int scalingId = tweenTransform.ChangeScale && !IsLocalScaleMulti(tweenTransform.LocalScaleMulti) ? ++_scalingId : -1;

			if (movingId == -1 && rotatingId == -1 && scalingId == -1)
				yield break;

			Debug.Log("TweenTo " + tweenTransform.Position + " gameObject " + gameObject);

			// position nosmooth
			Vector3 deltaPos = (tweenTransform.Position - transform.position);
			Vector3 deltaNormalized = deltaPos;
			deltaNormalized.Normalize();
			Vector3 velocity;
			float factor;

			// starting values
			Vector3 startScale = transform.localScale;
			Vector3 newScale = _startingScale * tweenTransform.LocalScaleMulti;
			Quaternion startRot = transform.rotation;
			Vector3 startPos = transform.position;

			// precompute duration
			float duration = 1;
			if (movingId != -1)
				duration = GetMoveTime(transform.position, tweenTransform.Position, definition.StartSpeed, definition.Acceleration);
			else if (scalingId != -1)
				duration = (newScale - startScale).sqrMagnitude;
			else
				duration = Quaternion.Angle(tweenTransform.Rotation, startRot);

			// smoothdamp
			float smoothSpeed = 0;

			float timeFactor = 1 / duration;
			float t = 0;
			float smoothT = 0;
			
		    // for time
//			float timeEnd = Time.time + definition.ForTime;
			float timeStart = Time.time;

			_transformingCnt++;
			while (true)
			{
/* debug
				if (_movingId != -1)
					transform.position = tweenTransform.Position;
				if (_rotatingId != -1)
					transform.rotation = tweenTransform.Rotation;
				if (_scalingId != -1)
					transform.localScale = newScale;
				break;
*/
//				if (definition.ForTime != 0 && Time.time >= timeEnd)
//					break;

				t = t + Time.deltaTime * timeFactor;

				if (definition.Type == TweenDefinition.TransformType.NoSmooth)
				{
					if (movingId == _movingId)
					{
						// TODO: some bug here (from minion on table to opponent hero it takes longer time than from opponent hero to minion on table)
						velocity = Vector3.Lerp(Vector3.zero, deltaNormalized, Time.deltaTime);
						factor = definition.StartSpeed;
						if (definition.Acceleration != 0)
							factor = factor + definition.Acceleration * (Time.time - timeStart);

//						Debug.Log("MOVE base velocity " + velocity + " factor " + factor + " timeDelta " + Time.deltaTime);
						velocity = velocity * factor;
						transform.position += velocity;

						if (t >= 1)
						{
							transform.position = tweenTransform.Position;
//							Debug.Log("MOVE END from " + transform.position + " to " + tweenTransform.Position);
							movingId = -1;
						}
					}

					if (rotatingId == _rotatingId)
						transform.rotation = Quaternion.Slerp(startRot, tweenTransform.Rotation, t);
					
					if (scalingId == _scalingId)
						transform.localScale = Vector3.Lerp(startScale, newScale, t);

					if (t >= 1)
						break;

				}
				else if (definition.Type == TweenDefinition.TransformType.SmoothOut)
				{
					smoothT = Mathf.SmoothDamp(smoothT, 1f, ref smoothSpeed, duration);

					if (smoothT > 0.999f)
						smoothT = 1;

					if (movingId == _movingId)
						transform.position = Vector3.Lerp(startPos, tweenTransform.Position, smoothT);

					if (rotatingId == _rotatingId)
						transform.rotation = Quaternion.Slerp(startRot, tweenTransform.Rotation, smoothT);
					
					if (scalingId == _scalingId)
						transform.localScale = Vector3.Lerp(startScale, newScale, smoothT);

					if (smoothT >= 1)
						break;
				}
				// TODO: GS opposite to smooth out
				else if (definition.Type == TweenDefinition.TransformType.SmoothIn)
				{
					smoothT = Mathf.SmoothDamp(smoothT, 1f, ref smoothSpeed, duration);
					
					if (smoothT > 0.999f)
						smoothT = 1;
					
					if (movingId == _movingId)
						transform.position = Vector3.Lerp(startPos, tweenTransform.Position, smoothT);
					
					if (rotatingId == _rotatingId)
						transform.rotation = Quaternion.Slerp(startRot, tweenTransform.Rotation, smoothT);
					
					if (scalingId == _scalingId)
						transform.localScale = Vector3.Lerp(startScale, newScale, smoothT);
					
					if (smoothT >= 1)
						break;
				}
				else if (definition.Type == TweenDefinition.TransformType.SmoothInOut)
				{
     				smoothT = Mathf.SmoothStep(0f, 1f, t);

					if (movingId == _movingId)
						transform.position = Vector3.Lerp(startPos, tweenTransform.Position, smoothT);
					
					if (rotatingId == _rotatingId)
						transform.rotation = Quaternion.Slerp(startRot, tweenTransform.Rotation, smoothT);
					
					if (scalingId == _scalingId)
						transform.localScale = Vector3.Lerp(startScale, newScale, smoothT);

					if (smoothT >= 1)
						break;
				}

				yield return null;
			}	

			_transformingCnt--;

			Debug.Log("TweenTo ENDED " + tweenTransform == null ? "null" : tweenTransform.Position + " gameObject " + gameObject);
		}

		public IEnumerator MoveToCoroutine(Vector3 destPos, TweenDefinition moveToDefinition)
		{
			return TweenToCoroutine(new TweenTransform(destPos), moveToDefinition);
		}

		public float GetMoveTime(Vector3 startPos, Vector3 destPos, float speed, float accel)
		{
			float distance = (destPos - startPos).magnitude;

			if (accel == 0)
			{
				if (speed == 0)
					return float.MaxValue;
				else
					return distance / speed;
			}
			else
			{
				// ax^2 + bx + c = 0  <=>  (1/2) * accel * time^2 + speed * time - distance = 0

				// D = b^2 - 4ac
				float disc = speed * speed + 2 * accel * distance;
				// r12 = (-b +- sqrt(D)) / 2a
				float sqrtDisc = Mathf.Sqrt(disc);
				float r1 = (sqrtDisc - speed) / accel;
				if (r1 > 0)
					return r1;
				else // r2
					return (-sqrtDisc - speed) / accel;
			}
		}

		/* private */
		class InvisibleObjectCache
		{
			private SpriteRenderer _spriteRenderer;
			private Renderer _generalRenderer;
			private ParticleSystem _particleSystem;
			private Color _startingColor;

			public Color Color()
			{ 
				if (_spriteRenderer != null) 
					return _spriteRenderer.color;
				else if (_generalRenderer != null)
					return _generalRenderer.material.color;
				else 
					return default(Color);
			}

			public InvisibleObjectCache(UnityEngine.GameObject obj)
			{
				if (obj.particleSystem != null && obj.particleSystem.isPlaying)
					_particleSystem = obj.particleSystem;

				SpriteRenderer spriteRenderer = obj.transform.renderer as SpriteRenderer;
				
				if (spriteRenderer)
				{
					_spriteRenderer = spriteRenderer;
					_startingColor = spriteRenderer.color;
				}
				else
				{
					if (obj.transform.renderer == null) return;
					if (!obj.renderer.material.HasProperty("_Color")) return;
					_generalRenderer = obj.transform.renderer;
					_startingColor = _generalRenderer.material.color;
				}
			}

			public bool IsValid()
			{
				return _spriteRenderer != null || _generalRenderer != null || _particleSystem != null;
			}

			public void SetAlpha(float alpha)
			{
				Color color = Color();
				color.a = alpha;

				if (_spriteRenderer != null) 
					_spriteRenderer.color = color;
				else if (_generalRenderer != null)
					_generalRenderer.material.color = color;

				if (_particleSystem != null)
				{
					if (alpha == 0)
						_particleSystem.Stop();
					else if (_particleSystem.isStopped)
						_particleSystem.Play();
				}
			}

			public bool AddAlpha(float addAlpha, bool dec)
			{
				Color startColor = Color();
				float alpha;

				if (dec)
				{
					alpha = startColor.a - addAlpha;
					if (alpha < 0) 
						alpha = 0;
					SetAlpha(alpha);
					return alpha == 0;
				}
				else
				{
					alpha = startColor.a + addAlpha;
					if (alpha > _startingColor.a) 
						alpha = _startingColor.a;
					SetAlpha(alpha);
					return alpha == startColor.a;
				}

			}
		}

		private void InvisibleFillCache(UnityEngine.GameObject obj, ref List<InvisibleObjectCache> objectsCaches, bool ignoreLinked)
		{
			InvisibleObjectCache mainObjCache = new InvisibleObjectCache(obj);
			if (mainObjCache.IsValid()) objectsCaches.Add(mainObjCache);

			int count = obj.transform.childCount;
			for(int i = 0; i < count; ++i)
			{
				Transform child = obj.transform.GetChild(i);
				if (!ignoreLinked || child.GetComponent<Picus.Utils.GO.TopInfo>() == null)
					InvisibleFillCache(child.gameObject, ref objectsCaches, ignoreLinked);
			}
		}

		private IEnumerator InvisibleTimeCoroutine(UnityEngine.GameObject obj, float time, bool deep, bool hide, bool ignoreLinked)
		{
			List<InvisibleObjectCache> objectsCaches = new List<InvisibleObjectCache>();
			int invisiblingId = ++_invisiblingId;

			if (deep)
			{
				InvisibleFillCache(obj, ref objectsCaches, ignoreLinked);
			}
			else
			{
				InvisibleObjectCache mainObjCache = new InvisibleObjectCache(obj);
				if (mainObjCache.IsValid())
					objectsCaches.Add(mainObjCache);
			}

			if (objectsCaches.Count == 0)
				yield break;

			if (!hide) // unhide => hide first
			{
				foreach (InvisibleObjectCache objectCache in objectsCaches)
				{
					objectCache.SetAlpha(0);
				}
			}

			bool done = false;
			float timeDelta;
			while (!done && invisiblingId == _invisiblingId)
			{
				done = true;
				timeDelta = Time.deltaTime / time;
				foreach (InvisibleObjectCache objectCache in objectsCaches)
				{
					if (objectCache.IsValid()) // can be invalidated (when linked object is destroyed before this effect ends)
					{
						done = objectCache.AddAlpha(timeDelta, hide) && done;
					}
				}
				yield return null;
			}	

			yield break;
		}

		private void Awake()
		{
			_startingScale = transform.localScale;
		}

		private bool IsLocalScaleMulti(float multi)
		{
			return transform.lossyScale == _startingScale * multi;
		}
	}
}