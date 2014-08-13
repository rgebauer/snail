using System;
using UnityEngine;
using System.Collections.Generic;

namespace Picus.Utils.GO
{
	public class Creator : Picus.MonoBehaviourExtend
	{
		public static UnityEngine.GameObject InstantiateWithAutodestroy(UnityEngine.GameObject effectTemplate)
		{
			UnityEngine.GameObject effect = InstantiateAndLink(effectTemplate, null, "", false, false);
			Finder.FindComponentAddIfNotExist<Duration>(effect);
			return effect;
		}
		
		public static UnityEngine.GameObject InstantiateAtPosWithAutodestroy(UnityEngine.GameObject effectTemplate, UnityEngine.GameObject toObjPos)
		{
			UnityEngine.GameObject effect = InstantiateAtPosWithAutodestroy(effectTemplate, toObjPos, -1);
			Finder.FindComponentAddIfNotExist<Duration>(effect);
			return effect;
		}
		
		public static UnityEngine.GameObject InstantiateAtPosWithAutodestroy(UnityEngine.GameObject effectTemplate, Vector3 toPos)
		{
			UnityEngine.GameObject effect = InstantiateWithAutodestroy(effectTemplate);
			effect.transform.position = toPos;
			Finder.FindComponentAddIfNotExist<Duration>(effect);
			return effect;
		}

		public static UnityEngine.GameObject InstantiateAtPosWithAutodestroy(UnityEngine.GameObject effectTemplate, UnityEngine.GameObject toObjPos, float time)
		{
			UnityEngine.GameObject effect = InstantiateAndLink(effectTemplate, toObjPos, null, true, false);
			Finder.FindComponentAddIfNotExist<Duration>(effect).SetDuration(time);
			return effect;
		}

		public static UnityEngine.GameObject InstantiateAndLinkWithAutodestroy(UnityEngine.GameObject effectTemplate, UnityEngine.GameObject toObjPos)
		{
			UnityEngine.GameObject effect = InstantiateAndLink(effectTemplate, toObjPos, null, false, false);
			Finder.FindComponentAddIfNotExist<Duration>(effect);
			return effect;
		}

		public static UnityEngine.GameObject InstantiateAndLinkWithAutodestroy(UnityEngine.GameObject effectTemplate, UnityEngine.GameObject toObjPos, bool inheritSorting)
		{
			UnityEngine.GameObject effect = InstantiateAndLink(effectTemplate, toObjPos, null, false, inheritSorting);
			Finder.FindComponentAddIfNotExist<Duration>(effect);
			return effect;
		}

		[Obsolete("Do not use preferedChildName")]
		public static UnityEngine.GameObject InstantiateAndLinkWithAutodestroy(UnityEngine.GameObject effectTemplate, UnityEngine.GameObject toObjPos, string preferedChildName)
		{
			UnityEngine.GameObject effect = InstantiateAndLink(effectTemplate, toObjPos, preferedChildName, false, false);
			Finder.FindComponentAddIfNotExist<Duration>(effect);
			return effect;
		}

		public static UnityEngine.GameObject InstantiateAndLink(UnityEngine.GameObject effectTemplate, UnityEngine.GameObject toObjPos, string childName, bool inheritSorting)
		{
			return InstantiateAndLink(effectTemplate, toObjPos, childName, false, inheritSorting);
		}

		private static UnityEngine.GameObject InstantiateAndLink(UnityEngine.GameObject effectTemplate, UnityEngine.GameObject toObjPos, string childName, bool unlinkNow, bool inheritSorting)
		{
			if (effectTemplate == null) 
				return null;
			
			UnityEngine.GameObject effect = Instantiate(effectTemplate) as UnityEngine.GameObject;
			effect.SetActive(true);
			
			if (toObjPos)
			{
				UnityEngine.GameObject linkTo = null;
				
				if (!string.IsNullOrEmpty(childName))
					linkTo = Finder.FindChildDeep(toObjPos, childName);
				
				if (linkTo == null)
					linkTo = toObjPos;
				
				Linker.LinkTo(effect, linkTo, false, inheritSorting);
				if (unlinkNow)
					Linker.LinkTo(effect, null);
			}
			
			return effect;
		}
	}
}

