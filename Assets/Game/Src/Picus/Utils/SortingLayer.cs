using UnityEngine;
using System.Collections;

namespace Picus.Utils
{
	public class SortingLayer : Picus.MonoBehaviourExtend
	{
		public const int MAX_SORTING_ORDER = 100000; // TODO: GS remove this const and change SortingLevel

		[SerializeField]
		public bool SortingOrderOffsetEnabled = false;

		[SerializeField]
		private bool _defaultSet = false;
		[SerializeField]
		private int _defaultSortingLayerId = 0;
		[SerializeField]
		private int _defaultSortingOrder = 0;

		public bool IsDefaultSet { get { return _defaultSet; } }

		public int DefaultSortingLayerId { get { return _defaultSortingLayerId; } private set { _defaultSortingLayerId = value; } }
		public int DefaultSortingOrder { get { return _defaultSortingOrder; } private set { _defaultSortingOrder = value; } }
		public int SortingOrderOffset { get; set; }

		static SortingLayerList _sortingLayerList;

		public int SortingLayerId
		{
			get
			{
				Renderer renderer = gameObject.GetComponent<Renderer>();
				if (renderer)
					DefaultSortingLayerId = renderer.sortingLayerID;
				return DefaultSortingLayerId;
			}
		}

		public int SortingOrder
		{
			get
			{
				Renderer renderer = gameObject.GetComponent<Renderer>();
				if (renderer)
					DefaultSortingOrder = renderer.sortingOrder;
				return DefaultSortingOrder + (SortingOrderOffsetEnabled ? SortingOrderOffset : 0);
			}
		}

		public void SetDefaults(int layerId, int sortingOrder)
		{
			SetDefaults(layerId, sortingOrder, true, true);
		}

		public void SetDefaultsId(int layerId)
		{
			SetDefaults(layerId, 0, true, false);
		}

		public void SetDefaultsOrder(int order)
		{
			SetDefaults(0, order, false, true);
		}

		public void SortAllDeepFrom(SortingLayer sortingLayer)
		{
			SetDeepStaticOrder(gameObject, sortingLayer.SortingLayerId);
			SetDeepDeltaOrder(gameObject, sortingLayer.SortingOrder);
		}

		public void SortAllDeepFromParent()
		{
			Transform parent = transform.parent;
			Renderer renderer = null;
			SortingLayer sortingLayer = null;
			while (parent != null && renderer == null && sortingLayer == null)
			{
				sortingLayer = parent.GetComponent<SortingLayer>();
				if (sortingLayer == null)
				{
					renderer = parent.GetComponent<Renderer>();
					if (renderer != null)
						sortingLayer = parent.gameObject.AddComponent<SortingLayer>();
				}
				parent = parent.transform.parent;
			}

			if (sortingLayer != null)
				SortAllDeepFrom(sortingLayer);
		}

		/// <summary>
		/// Set deeply new sortingId/name, keep old sortingOrder. Ignore linked objects.
		/// </summary>
		/// <param name="root">Root gameObject.</param>
		/// <param name="newName">SortingLayer name</param>
		public static void SetDeepStaticOrder(UnityEngine.GameObject root, string newName)
		{
			Set(root, newName, -1, 0, true, 0, false, true);
		}

		/// <summary>
		/// Set deeply new sortingId/name, keep old sortingOrder.
		/// </summary>
		/// <param name="root">Root gameObject.</param>
		/// <param name="newName">SortingLayer name</param>
		/// <param name="ignoreLinkedObjects">If set to <c>false</c> reset layer to linked objects too.</param>
		public static void SetDeepStaticOrder(UnityEngine.GameObject root, string newName, bool ignoreLinkedObjects)
		{
			Set(root, newName, -1, 0, true, 0, false, ignoreLinkedObjects);
		}

		/// <summary>
		/// Set deeply new sortingId/name, keep old sortingOrder. Ignore linked objects.
		/// </summary>
		/// <param name="root">Root gameObject.</param>
		/// <param name="newId">Sorting layer id.</param>
		public static void SetDeepStaticOrder(UnityEngine.GameObject root, int newId)
		{
			Set(root, null, -1, 0, true, newId, false, true);
		}

		// set new sortingId/name, set new sortingOrder regards hiearchy
		public static void SetDeepIncreasedOrder(UnityEngine.GameObject root, string newName, int newOrder)
		{
			Set(root, newName, newOrder, 1, true, 0, true, true);
		}

		// add order to existing order, keep layer
		public static void SetDeepDeltaOrder(UnityEngine.GameObject root, int deltaOrder)
		{
			SetDeepDeltaOrder(root, deltaOrder, true);
		}

		// add order to existing order, keep layer
		public static void SetDeepDeltaOrder(UnityEngine.GameObject root, int deltaOrder, bool ignoreLinkedObjects)
		{
			SortingLayer sortingLayer = Picus.Utils.GO.Finder.FindComponentAddIfNotExist<SortingLayer>(root);
			
			sortingLayer.SetDefaults(sortingLayer.SortingLayerId, sortingLayer.SortingOrder + deltaOrder, true, true);

			Transform transform = root.transform;
			int count = transform.childCount;
			for(int i = 0; i < count; i++)
			{
				Transform child = transform.GetChild(i);

				if (ignoreLinkedObjects && child.GetComponent<GO.TopInfo>() != null)
					continue;

				SetDeepDeltaOrder(child.gameObject, deltaOrder);
			}
		}

		public static int RecomputedSortingLevel(UnityEngine.GameObject obj)
		{
			Transform transform = obj == null ? null : obj.transform;
			// TODO: optimalise through GameObjectTopInfo
			while (transform != null)
			{
				SortingLayer sortingLayerComp = transform.GetComponent<SortingLayer>();
				int sortingLayer;
				int sortingOrder;
				
				if (sortingLayerComp != null)
				{
					sortingLayer = sortingLayerComp.SortingLayerId; // need to be sorted in layers order
					sortingOrder = sortingLayerComp.SortingOrder;
				}
				else
				{
					Renderer renderer = transform.GetComponent<Renderer>();
					if (renderer == null)
					{
						transform = transform.parent;
						continue;
					}
					sortingLayer = renderer.sortingLayerID;
					sortingOrder = renderer.sortingOrder;
				}
				Debug.Assert(sortingOrder < MAX_SORTING_ORDER, "Utils.GameObject.SortingLevel too high sorting order " + sortingOrder);
				
				int ret = sortingLayer * MAX_SORTING_ORDER + sortingOrder;
				//				Debug.Log("Utils.GameObject.SortingLevel sName: " + renderer.sortingLayerName + "; sId: " + sortingLayer + "; sOrder: " + sortingOrder + " => " + ret);
				
				return ret;
			}
			
			return -1;
		}

		/* private */
		private static void Set(UnityEngine.GameObject root, string newName, int newOrder, int deltaOrder, bool deep, int newId, bool setOrder, bool ignoreLinked) // get newId only if newName is null
		{
			if (newName != null && newName.Equals("Default"))
				Picus.Sys.Debug.Empty();

			SortingLayer sortingLayer = Picus.Utils.GO.Finder.FindComponentAddIfNotExist<SortingLayer>(root);

			if (newName == null)
				sortingLayer.SetDefaults(newId, newOrder, true, setOrder);
			else
				sortingLayer.SetDefaults(newName, newOrder, setOrder);

			if (!deep)
				return;

			Transform transform = root.transform;
			int count = transform.childCount;
			for(int i = 0; i < count; i++)
			{
				Transform child = transform.GetChild(i);
				if (ignoreLinked && child.GetComponent<GO.TopInfo>() != null)
					continue;
				Set(child.gameObject, newName, newOrder + deltaOrder, deltaOrder, deep, newId, setOrder, ignoreLinked);
			}
		}

		private void SetDefaults(int layerId, int sortingOrder, bool setLayer, bool setOrder)
		{
			_defaultSet = true;
			if (setLayer)
				DefaultSortingLayerId = layerId;
			if (setOrder)
				DefaultSortingOrder = sortingOrder;
			
			Renderer renderer = gameObject.GetComponent<Renderer>();
			if (renderer)
			{
				if (setLayer && renderer.sortingLayerID != layerId) 
					renderer.sortingLayerID = layerId;
				if (setOrder && renderer.sortingOrder != sortingOrder) 
					renderer.sortingOrder = sortingOrder;
			}
		}

		/** Very SLOW, use layerId instead */
		private void SetDefaults(string layerName, int sortingOrder, bool setOrder)
		{
			Renderer renderer = gameObject.GetComponent<Renderer>();
			Renderer createdRenderer = null;
			
			if (renderer == null)
			{
				renderer = gameObject.AddComponent<SpriteRenderer>();
				createdRenderer = renderer;
			}
			
			int layerId = 0;
			
			if (renderer)
			{
				if (renderer.sortingLayerName != layerName)
					renderer.sortingLayerName = layerName;
				layerId = renderer.sortingLayerID;
			}
			
			if ( Application.isPlaying )
			{
				Destroy(createdRenderer);
			}
			else
			{
				DestroyImmediate(createdRenderer);
			}
						
			SetDefaults(layerId, sortingOrder, true, setOrder);
		}

		private SortingLayerList SortingLayerList()
		{
			if (_sortingLayerList == null)
				_sortingLayerList = Utils.SortingLayerList.Load();

			return _sortingLayerList;
		}
	}
}
