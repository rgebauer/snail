using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Picus.Sys
{
    public class GuiButton : Picus.MonoBehaviourExtend
    {
		private const int FirstX = 20;
		private const int FirstY = 20;
		private const int MinDeltaY = 5;

		private struct Item
		{
			public string Text;
			public MethodNoParam Callback;
			public bool Separated;
			public GameObject GameObject;
		}

		private static List<Item> _items = new List<Item>();
		private static GameObject _gameObject;
		private static float _maxSizeX, _maxSizeY;
		private static GUIStyle _guiStyle;

		/// <summary>
		/// Create new gui button. Will be destroyed automatically.
		/// </summary>
		/// <param name="callback">Called on click.</param>
		/// <param name="destroyWith">When destroyWith is destroyed, gui button is destroyed too.</param>
		/// <param name="YSpace">Insert new line above?</param>
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Add(string text, MethodNoParam callback, GameObject destroyWith = null, bool YSpace = false)
		{
			CreateGameObjectIfNeeded();
			_items.Add(new Item { Text = text, Callback = callback, GameObject = destroyWith == null ? _gameObject : destroyWith, Separated = YSpace });
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void Remove(MethodNoParam callback)
		{
			_items.RemoveAll(x => x.Callback == callback);
			DeleteGameObjectIfNotNeeded();
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void Clear()
		{
			_items.Clear();
			DeleteGameObjectIfNotNeeded();
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private static void CreateGameObjectIfNeeded()
		{
			if (_gameObject != null)
				return;

			_gameObject = new GameObject("(singleton) GuiButtons");
			_gameObject.AddComponent<GuiButton>();

			DontDestroyOnLoad(_gameObject);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private static void DeleteGameObjectIfNotNeeded()
		{
			if (_items.Count != 0)
				return;

			Destroy(_gameObject);
			_gameObject = null;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private void OnGUI() 
		{
			if (_guiStyle == null)
			{
				_guiStyle = new GUIStyle("button");
				_guiStyle.alignment = TextAnchor.MiddleLeft;
			}

			float y = FirstY;

			int removed = _items.RemoveAll(x => x.GameObject == null || x.Callback == null);

			if (removed != 0)
				_maxSizeX = 0;

			if (_maxSizeX == 0)
			{
				for (int i = 0, cnt = _items.Count; i < cnt; ++i)
					RecomputeMaxSize(_items[i]);
			}

			for (int i = 0, cnt = _items.Count; i < cnt; ++i)
			{
				Item item = _items[i];

				if (item.Separated)
					y = y + MinDeltaY + _maxSizeY;

				if (GUI.Button(new Rect (FirstX, y, _maxSizeX, _maxSizeY), item.Text, _guiStyle)) 
				{
					item.Callback();
					break;
				}

				y = y + MinDeltaY + _maxSizeY;
			}
		}

		private static void RecomputeMaxSize(Item item)
		{
			GUIContent content = new GUIContent(item.Text);

			Rect buttonRect = GUILayoutUtility.GetRect(content, _guiStyle);
			if (Event.current.type == EventType.Repaint)
			{
				_maxSizeX = Mathf.Max(buttonRect.x, _maxSizeX);
				_maxSizeY = Mathf.Max(buttonRect.y, _maxSizeY);
			}
		}

		private GuiButton() {}
    }
}

