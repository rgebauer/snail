using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Picus.Extensions
{
	public static class AnimatorExtension
	{
		public static bool TriggerExists(this UnityEngine.Animator animator, string triggerName)
		{
			if (!animator)
				return false;

			animator.logWarnings = false;
			
			bool origVal = animator.GetBool(triggerName);
			
			animator.SetBool(triggerName, true);
			if (!animator.GetBool(triggerName))
				return false;
			animator.SetBool(triggerName, false);
			if (animator.GetBool(triggerName))
				return false;
			
			animator.SetBool(triggerName, origVal);
			return true;
		}
	}
}