using System;
using UnityEngine;
using System.Collections.Generic;

namespace Picus.Utils
{
	public class Game
	{
		public static void SetSpeed(float speed)
		{
//			Debug.Assert(speed == 1 || (speed != 1 && Time.timeScale == 1 && Time.fixedDeltaTime == 0.02f), "Utils.Game.SetSpeed " + speed + " but already not on default speed " + Time.timeScale + ", " + Time.fixedDeltaTime, true);
			float fixedTimeStep = FixedTimeStep();

			Time.timeScale = speed;
			Time.fixedDeltaTime = speed * fixedTimeStep;
		}

		/** Optimisation when mouse events are not needed. Must be called on every scene (camera) change. */
		public static void DisableOnMouseEvents()
		{
			Camera.main.eventMask = 0;
		}

		public static float FixedTimeStep()
		{
			if (Time.timeScale == 0)
				return 0;

			return Time.fixedDeltaTime / Time.timeScale;
		}
	}
}

