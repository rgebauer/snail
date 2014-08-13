using System;
using UnityEngine;
using System.Collections.Generic;
using Picus.Extensions;

namespace Picus.Utils.GO
{
	public class LayerChanger
	{
		public static void SetLayerDeep(GameObject obj, string layerName)
		{
			int layerId = LayerMask.NameToLayer(layerName);
			SetLayerDeep(obj, layerId);
		}

		public static void SetLayerDeep(GameObject obj, int layerId)
		{
			obj.layer = layerId;

			Transform tr = obj.transform;
			int count = tr.childCount;
			for(int i = 0 ; i < count; i++)
			{
				Transform child = tr.GetChild(i);
				SetLayerDeep(child.gameObject, layerId);
			}
		}
	}
}

