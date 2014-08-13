using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

// TODO: GS
// -hotswap after recompilation
// -autoscroll on scrolling at end
// -styles like in original console (resizing proportional, labels not buttons (allow copyandpaste), do not become slow on more _logCache)
// -if last stackFrame is not in code (but in .net or unity core), open next frame (like original console)
// -test it on Windows
// -when autoscroll disabled and filters changed, scroll to preselected item (if still there)
// -remember filter between restarts (now is ids shuffled) - do not use serialized Dictionary

/** Picus debug console with some new features against classic console:
 * -doubleclick in mainview is resolving stack in coroutines and filtering stackframes from our debug library
 * -in stack view, You can doubleclick on each stack line separate
 * -log filter by namespace
 */

// visual get from http://u3d.as/content/inhuman-games/console-enhanced/5YZ
using System;
using System.Text;


[System.Serializable]
public class DebugConsole : EditorWindow //namespace Picus.Editor // regards ConsoleE: using namespaces seems to cause Unity to not serialize objects as fully, when Editor changes play states
{
	[SerializeField]
	private LinkedList<DebugConsoleItem> _logCache = new LinkedList<DebugConsoleItem>();

	private Vector2 _mainScrollPos = Vector2.zero;
	private Vector2 _infoScrollPos = Vector2.zero;
	private bool _addLogsOnNextUpdate = false;

	// doubleclicks
	private DebugConsoleItem _lastClickedItem = null;
	private int _lastClickedStackId = -1;
	private float _lastClickedTime;
	private float _lastClickedStackTime;

	// userpref
	private bool _autoScroll = true;
	private bool _tryResolveDebugStack = true;

 	// resizable view
	private float _currentScrollViewHeight;
	private float _currentScrollViewStartY;
	private bool _resize = false;
	private Rect _cursorChangeRect;
	
	private const float DOUBLE_CLICK_TIME = 0.5f;

	// skin
	private GUIStyle _styleButtonToolbar;
/*	private GUIStyle _styleRowOddBackground;
	private GUIStyle _styleRowEvenBackground;
	private GUIStyle _styleSelectedBackground;
*/	private GUIStyle _styleInfo;
	private GUIStyle _styleWarning;
	private GUIStyle _styleError;
	private GUIStyle _styleInfoSelected;
	private GUIStyle _styleWarningSelected;
	private GUIStyle _styleErrorSelected;
	private GUIStyle _styleLineButton;
	private GUIStyle _styleLineLabel;

	// filters
	private int _filterFlags = -1;
	[SerializeField]
	private Dictionary<string, int> _usedFilters = new Dictionary<string, int>();
	[SerializeField]
	private string[] _usedFiltersKeys = new string[0];
		
	private bool _skinInited;
	private bool _inited;
	private bool _wasStopped = true;

	private DebugConsoleSolver _solver;
	private DebugConsoleBridge _bridge;

	// ConsoleE
	bool _isLeftMouseDown;
	bool _isRightMouseDown;
	bool _isLeftMouseDownNewClick;
	bool _isLeftMouseDoubleClickNewClick;
	bool _isRightMouseDownNewClick;
	bool _isDraggingSeparator;
	bool _isCollapseEnabled;
	bool _isMainScrollbarVisible;
	bool _isCallstackScrollbarVisible;
	float _scrollbarCursorMain;
	float _scrollbarCursorCallstack;
	float _xScrollCallstack;
	float _xScrollMain;
	float _yScrollMain;  // used only if vertical scrollbar is at max top or bottom
	int _indexFirstEntryOnScreen;
	int _countEntriesOnScreen;
	int _totalEntries;
	int _totalEntriesLast;  // count for previous GUI update 
	string _lastStatusText; // this is a way of detecting changes to the console. if there is a change to the console logs, either the last line will change (or the total number of entries will change).  If the last line changes, the status text will change.
	DateTime _timeSinceLastScrollDragUpdate;	
	DateTime _timeLastGetEntries;
	GUIStyle _textStyleMainAreaBackground;
	GUIStyle _textStyleEven;
	GUIStyle _textStyleOdd;
	GUIStyle _textStyleSelectedBackground;
	GUIStyle _textStyleSeparator;
	GUIStyle _textStyleCallstack;
	GUIStyle _textStyleCallstackSelected;
	GUIStyle _textStyleCallstackSelectedNoWindowFocus;
	GUIStyle _textStyleCollapseCount;
	GUIStyle _textStyleVoid;
	bool _isProSkin = false; // EditorGUIUtility.isProSkin;
	bool _selectionChanged;
	int _indexSelected;
	int _indexSelectedPrevious;
	DateTime _timeLeftMouseDown;
	Vector2 _lastLeftMouseDown;
	Vector2 _lastMousePos;
	Vector2 _panPos;
	bool _canStartPan;	
	bool _isScrollSnappedToEnd;
	bool _scrollToSelectedItem;
	public enum WindowArea
	{
		Unknown,
		MainArea,
		MainAreaScrollbar,
		CallstackArea,
		CallstackAreaScrollbar,
		Seperator,
		Toolbar,
	}
	WindowArea _areaOnMouseDown; // where was the mouse when left button was pressed
	WindowArea _areaKeyboardFocus; 
	const float TOOLBAR_HEIGHT = 18f;
	const float ENTRY_HEIGHT = 32f;
	const float CALLSTACK_ENTRY_HEIGHT = 13f;

	private static DebugConsole _instance;
	
	private static DebugConsole Instance
	{
		get
		{
			if (_instance == null)
				_instance = EditorWindow.GetWindow(typeof(DebugConsole)) as DebugConsole;
			_instance.title = "PicusConsole";
			return _instance;
		}
	}
	
	[MenuItem ("Window/Picus Console", priority = 2198)]
	private static void ShowDebugConsole()
	{
		if (_instance == null)
		{
			DebugConsole window = Instance;
			window.Init();
			window.Show();
		}
		else
		{
			DebugConsole.FocusWindowIfItsOpen<DebugConsole>();
		}
	}

	private void ClearLogCache()
	{
//		_nextRowIdToRead = 0;
		_lastClickedItem = null;
		_lastClickedStackId = -1;
		_logCache.Clear();
	}

	private void OnEnable()
	{
		_nearestUpdateTime = 0;

		EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;

		Picus.Sys.Debug.RegisterLogCallback(OnUnityLog);

		if (_bridge == null)
			_bridge = new DebugConsoleBridge();

		if (_solver == null)
			_solver = new DebugConsoleSolver();

		if (!_inited)
			Init();
	}

	private void OnDisable()
	{
		EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
	}

	private void OnDestroy()
	{
		Picus.Sys.Debug.UnRegisterLogCallback(OnUnityLog);
		_instance =  null;
	}


	System.Exception _exception = null;

	private void OnGUI()
	{
		try
		{
			if (_exception == null)
				OnGuiInternal();
			else
				OnGuiInternalException();
		}
		catch (System.Exception e)
		{
			_exception = e;
		}
	}

	private void OnGuiInternalException()
	{
		EditorGUILayout.BeginHorizontal();

		GUILayout.Label("Some error occured " + _exception);

		if (GUILayout.Button("RETRY"))
			_exception = null;

		EditorGUILayout.EndHorizontal();
	}


	StringBuilder filteredStringBuilder = new StringBuilder();
	private string FilteredToString()
	{
		filteredStringBuilder.Length = 0;

		for (LinkedListNode<DebugConsoleItem> it = _logCache.First; it != null; it = it.Next)
		{
			DebugConsoleItem cacheItem = it.Value;
			
			if (!IsFilteredIn(cacheItem))
				continue;

			filteredStringBuilder.Append(cacheItem.Text + "\n");
		}

		return filteredStringBuilder.ToString();
	}
	
	private void MainViewGui()
	{
		_mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos, GUILayout.Height(_currentScrollViewHeight));

		for (LinkedListNode<DebugConsoleItem> it = _logCache.First; it != null; it = it.Next)
		{
			DebugConsoleItem cacheItem = it.Value;

			if (!IsFilteredIn(cacheItem))
				continue;

			bool isLastClicked = cacheItem == _lastClickedItem;
			GUIStyle buttonStyle = LogTypeStyle(cacheItem.Type, isLastClicked);

			if (GUILayout.Button(cacheItem.Text + "\n"  + cacheItem.StackFirstLine, buttonStyle)) 
			{
				if (cacheItem.InstanceId != 0)
					Selection.activeInstanceID = cacheItem.InstanceId; // select gameObject
				
				if (isLastClicked && _lastClickedTime + DOUBLE_CLICK_TIME > Time.realtimeSinceStartup) // doubleclick
					_solver.OpenStack(cacheItem, _tryResolveDebugStack);

				_lastClickedItem = cacheItem;
				_lastClickedTime = Time.realtimeSinceStartup;
			}
		}
		
		EditorGUILayout.EndScrollView();
	}

	private bool IsFilteredIn(DebugConsoleItem cachedItem)
	{	
		if (_filterFlags == -1 || _usedFilters.Count == 0 || !IsLog(cachedItem.Type))
			return true;

		return (cachedItem.FilterFlag & _filterFlags) != 0;
	}

	private void ResizeScrollViewGui()
	{
		if (Event.current.type == EventType.Repaint)
		{
			_cursorChangeRect = GUILayoutUtility.GetLastRect();
			_cursorChangeRect.y = _cursorChangeRect.y + _cursorChangeRect.height + 1;
			_cursorChangeRect.height = 2f;
		}

		if( Event.current.type == EventType.mouseDown)
			if (_cursorChangeRect.Contains(Event.current.mousePosition))
				_resize = true;

		if(_resize)
		{
			float currentScrollViewEndY = Event.current.mousePosition.y;
			_currentScrollViewHeight = currentScrollViewEndY - _currentScrollViewStartY;
			if (_currentScrollViewHeight < 0)
				_currentScrollViewHeight = 0;
			_cursorChangeRect.Set(_cursorChangeRect.x, currentScrollViewEndY - 1, _cursorChangeRect.width, _cursorChangeRect.height);
		}

		GUI.DrawTexture(_cursorChangeRect, EditorGUIUtility.whiteTexture);
		EditorGUIUtility.AddCursorRect(_cursorChangeRect, MouseCursor.ResizeVertical);

		if (_resize)
			Repaint();

		if(Event.current.type == EventType.MouseUp)
			_resize = false;        
	}

	private void BottomViewGui()
	{
		_infoScrollPos = EditorGUILayout.BeginScrollView(_infoScrollPos);

		if (_lastClickedItem != null)
		{
			GUILayout.Space(3);
			string[] stackFrames = _lastClickedItem.Stack.Split('\n');

			GUILayout.BeginHorizontal();
			GUILayout.Label(_lastClickedItem.Text, _styleLineLabel);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("2clipboard"))
			{
				string allStack = _lastClickedItem.Text + "\n" + _lastClickedItem.Stack;

				CopyToClipboard(allStack);
			}
			GUILayout.EndHorizontal();

			for (int i = 0, cnt = stackFrames.Length; i < cnt; ++i)
			{
				if (GUILayout.Button(stackFrames[i], _styleLineButton)) 
				{
					if (_lastClickedStackId == i && _lastClickedStackTime + DOUBLE_CLICK_TIME > Time.realtimeSinceStartup) // doubleclick
						_solver.OpenStackFrame(_lastClickedItem, _lastClickedStackId, _tryResolveDebugStack);

					_lastClickedStackId = i;
					_lastClickedStackTime = Time.realtimeSinceStartup;

					break;
				}
			}
		}
		
		EditorGUILayout.EndScrollView();
	}

	private void CopyToClipboard(string text)
	{
		TextEditor te = new TextEditor();
		te.content = new GUIContent(text);
		te.SelectAll();
		te.Copy();
	}
	
	private void Init()
	{
		_logCache.Clear();

		_currentScrollViewHeight = this.position.height / 3 * 2;
		_cursorChangeRect = new Rect(0, _currentScrollViewHeight, this.position.width, 2f);

		RefreshLogs();

		_inited = true;
 	}
	
	private void OnPlaymodeStateChanged()
	{
		Picus.Sys.Debug.UnRegisterLogCallback(OnUnityLog);
		Picus.Sys.Debug.RegisterLogCallback(OnUnityLog);

		if (_addLogsOnNextUpdate)
			Repaint();

		if (_wasStopped && EditorApplication.isPlaying)
		{
//			int flags = _bridge.CurrentToolbarFlags();
//			if (_bridge.IsFlagEnabled(flags, DebugConsoleBridge.ToolbarFlags.ClearOnPlay))
			RefreshLogs(); // on start all logs are deleted
		}
		else if (!EditorApplication.isPaused)
			_wasStopped = true;
	}

	private void OnUnityLog(string logString, string stackTrace, LogType type) 
	{
		_addLogsOnNextUpdate = true;
	}

	private float _nearestUpdateTime = 0;
	private void Update()
	{
/*		Event e = Event.current;
		if (e.type == EventType.MouseDown && MouseInside(e.mousePosition))
			Repaint();
*/
		if (_nearestUpdateTime < Time.realtimeSinceStartup)
		{
			if (!_addLogsOnNextUpdate && _logCache.Count != _bridge.EntriesCount())
				_addLogsOnNextUpdate = true;

			if (_addLogsOnNextUpdate)			
				Repaint();

			_nearestUpdateTime = Time.realtimeSinceStartup + 0.5f;
		}
	}

	private GUIStyle LogTypeStyle(LogType type, bool selected)
	{
		if (IsError(type))
			return selected ? _styleErrorSelected : _styleError;
		else if (IsWarning(type))
			return selected ? _styleWarningSelected : _styleWarning;
		else
			return selected ? _styleInfoSelected : _styleInfo;
	}

	private bool IsError(LogType type)
	{
		return (type == LogType.Exception || type == LogType.Error || type == LogType.Assert);
	}

	private bool IsWarning(LogType type)
	{
		return type == LogType.Warning;
	}

	private bool IsLog(LogType type)
	{
		return !IsWarning(type) && !IsError(type);
	}

	private void RefreshLogs()
	{
		ClearLogCache();
		AddNewLogs();
	}

	private void AddNewLogs()
	{
		LinkedListNode<DebugConsoleItem> lastNode = _logCache.Last;
		int readed = _bridge.GetEntries(_logCache, _logCache.Count);
//		_nextRowIdToRead += readed;

		if (lastNode == null)
			lastNode = _logCache.First;
		else
			lastNode = lastNode.Next;

		bool changed = false;
		int filterFlag;
		for (LinkedListNode<DebugConsoleItem> it = lastNode; it != null; it = it.Next)
		{
			string filter = it.Value.Filter;
			if (!string.IsNullOrEmpty(filter))
			{
				if (_usedFilters.TryGetValue(filter, out filterFlag))
				{
					it.Value.FilterFlag = filterFlag;
		   		}	
				else
				{
					it.Value.FilterFlag = 1 << _usedFilters.Count;
					_usedFilters.Add(filter, it.Value.FilterFlag);
					changed = true;
				}
			}
		}

		if (changed)
			_usedFiltersKeys = _usedFilters.Keys.ToArray();

		_addLogsOnNextUpdate = false;

		if (_autoScroll)
			_mainScrollPos = new Vector2(0, int.MaxValue);
	}

	private void ToolbarGui()
	{
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Clear", _styleButtonToolbar))
		{
			_bridge.Clear();
			ClearLogCache();
		}

		_tryResolveDebugStack = GUILayout.Toggle(_tryResolveDebugStack, "Doubleclick debug", _styleButtonToolbar);	

		int flags = _bridge.CurrentToolbarFlags();
		int oldFlags = flags;

		bool oldValue = _bridge.IsFlagEnabled(flags, DebugConsoleBridge.ToolbarFlags.ClearOnPlay);
		bool newValue = GUILayout.Toggle(oldValue, "Clear on Play", _styleButtonToolbar);
		if (oldValue != newValue)
			_bridge.SetFlag(ref flags, DebugConsoleBridge.ToolbarFlags.ClearOnPlay, newValue);

		oldValue = _bridge.IsFlagEnabled(flags, DebugConsoleBridge.ToolbarFlags.ErrorPause);
		newValue = GUILayout.Toggle(oldValue, "Error Pause", _styleButtonToolbar);
		if (oldValue != newValue)
			_bridge.SetFlag(ref flags, DebugConsoleBridge.ToolbarFlags.ErrorPause, newValue);

		oldValue = _autoScroll;
		_autoScroll = GUILayout.Toggle(_autoScroll, "Auto Scroll", _styleButtonToolbar);
		if (_autoScroll && !oldValue)
			_mainScrollPos = new Vector2(0, int.MaxValue);

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("2clipboard"))
			CopyToClipboard(FilteredToString());

		int prevFilter = _filterFlags;
		_filterFlags = EditorGUILayout.MaskField(_filterFlags, _usedFiltersKeys);
		if (prevFilter != _filterFlags && _autoScroll)
			_mainScrollPos = new Vector2(0, int.MaxValue);

		int logCnt, warningCnt, errorCnt;
		_bridge.CountsByType(out logCnt, out warningCnt, out errorCnt);

		oldValue = _bridge.IsFlagEnabled(flags, DebugConsoleBridge.ToolbarFlags.ShowInfos);
		newValue = GUILayout.Toggle(oldValue, " L: " + logCnt, _styleButtonToolbar); //_styleInfo);
		if (newValue != oldValue)
			_bridge.SetFlag(ref flags, DebugConsoleBridge.ToolbarFlags.ShowInfos, newValue);

		oldValue = _bridge.IsFlagEnabled(flags, DebugConsoleBridge.ToolbarFlags.ShowWarnings);
		newValue = GUILayout.Toggle(oldValue, " W: " + warningCnt, _styleButtonToolbar); //_styleWarning);
		if (newValue != oldValue)
			_bridge.SetFlag(ref flags, DebugConsoleBridge.ToolbarFlags.ShowWarnings, newValue);

		oldValue = _bridge.IsFlagEnabled(flags, DebugConsoleBridge.ToolbarFlags.ShowErrors);
		newValue = GUILayout.Toggle(oldValue, " E: " + errorCnt, _styleButtonToolbar); // _styleError);
		if (newValue != oldValue)
			_bridge.SetFlag(ref flags, DebugConsoleBridge.ToolbarFlags.ShowErrors, newValue);

		EditorGUILayout.EndHorizontal();

		if (oldFlags != flags)
		{
			RefreshLogs();
			if (_autoScroll)
				_mainScrollPos = new Vector2(0, int.MaxValue);
		}
	}

	private void InitSkin()
	{
		_styleButtonToolbar= _bridge.UnityStyle("toolbarButton");

//		_styleRowEvenBackground = _bridge.UnityStyle("CN EntryBackEven");		
//		_styleRowOddBackground = _bridge.UnityStyle("CN EntryBackOdd");

//		_styleSelectedBackground = _bridge.UnityStyle("ServerUpdateChangesetOn");

		_styleInfo = _bridge.UnityStyle("CN EntryInfo");
		_styleInfoSelected = new GUIStyle(_styleInfo);
		_styleInfoSelected.normal.textColor = Color.white;

		_styleWarning = _bridge.UnityStyle("CN EntryWarn");
		_styleWarningSelected = new GUIStyle(_styleWarning);
		_styleWarningSelected.normal.textColor = Color.white;

		_styleError = _bridge.UnityStyle("CN EntryError");
		_styleErrorSelected = new GUIStyle(_styleError);
		_styleErrorSelected.normal.textColor = Color.white;

		_styleLineButton = new GUIStyle();
		_styleLineButton.alignment = TextAnchor.MiddleLeft;
		_styleLineButton.normal.textColor = _styleInfo.normal.textColor;

		_styleLineLabel = new GUIStyle(_styleLineButton);
		_styleLineLabel.normal.textColor = Color.grey;

		_skinInited = true;
	}

	private bool MouseInside(Vector2 mousePosition)
	{
		return _instance.position.Contains(mousePosition);
	}

	private void OnGuiInternal()
	{
		if (!_inited)
			return;
		
		if (!_skinInited) // can be called only from OnGUI
			InitSkin();
		
		if (_addLogsOnNextUpdate)
			AddNewLogs();
		
		ToolbarGui();
		
		if (Event.current.type == EventType.Repaint)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			_currentScrollViewStartY = lastRect.yMax + 1;
		}
		
		EditorGUILayout.BeginVertical();
		
		MainViewGui();
		
		ResizeScrollViewGui();
		
		BottomViewGui();
		
		EditorGUILayout.EndVertical();
	}
/*
	// ConsoleE
	bool OnGuiPrivate()
	{
		Event eventCurrent = Event.current;
		
		// optimization 
		if(eventCurrent.type != EventType.Repaint && !eventCurrent.isKey && !eventCurrent.isMouse && eventCurrent.type != EventType.ContextClick && 
		   eventCurrent.type != EventType.ScrollWheel && eventCurrent.type != EventType.ValidateCommand && eventCurrent.type != EventType.ExecuteCommand)
		{
			if(!_isLeftMouseDown && !_isRightMouseDown && !_instance.wantsMouseMove) // otherwise we don't get our mouse button release event (when mouse is not over our window)
				return true;
		}
		
		if (!_skinInited) // can be called only from OnGUI
			InitSkin();

		_totalEntriesLast = _totalEntries;
		_totalEntries = _bridge.EntriesCount();

		ProcessInputEvents();
		
		float xClick = -1.0f;
		float yClick = -1.0f;
		float yClickFromToolbar = -1.0f;
		
		bool isLeftClick = false;
		bool isRightClick = false;
		
		if(_isLeftMouseDownNewClick || _isRightMouseDownNewClick)
		{
			if(eventCurrent.mousePosition.y >= TOOLBAR_HEIGHT - 1.0f)
			{
				isLeftClick = _isLeftMouseDownNewClick;
				isRightClick = _isRightMouseDownNewClick;

				xClick = eventCurrent.mousePosition.x;
				yClick = eventCurrent.mousePosition.y;
				yClickFromToolbar = yClick - TOOLBAR_HEIGHT;// + scrollbarCursorLog;
			}
			
			_isLeftMouseDownNewClick = false;
			_isRightMouseDownNewClick = false;
		}
		
		ToolbarGui();
		
		float heightScrollbar = GetMainScrollBarHeight();
		_isMainScrollbarVisible = _logCache.Count >= Mathf.CeilToInt(heightScrollbar / ENTRY_HEIGHT);
		float widthScrollbarComponent = GetMainScrollbarWidth();
		Rect rectScrollbarComponent = new UnityEngine.Rect(GetWindowWidth() - widthScrollbarComponent + 1.0f, TOOLBAR_HEIGHT - 1.0f, widthScrollbarComponent, heightScrollbar );
		float yScrollMaxValue = GetMainScrollBarAreaHeight();
		
		// there's some weird errors with rendering this last (I commented out where I had this called in an older version) (in Unity 3) - also it's more accurate (by one frame) to render the scrollbar before it's innards, because the innards are based on the scrollbar pos
		RenderMainScrollbar(_isMainScrollbarVisible, rectScrollbarComponent, heightScrollbar, yScrollMaxValue); 
		
		//
		// handle dragging of separator
		//
		var rectSeparatorMouseOver = GetSeparatorMouseOverRect();
		
		// if we are already dragging the separator, then make the cursor rect as big as possible so that it does not flicker during drag
		var rectCursorRect = _isDraggingSeparator ? new UnityEngine.Rect(0, 0, GetWindowWidth(), GetWindowHeight()) : rectSeparatorMouseOver;
		
		float separator = GetSeparator();
		
		if(yClick != -1.0f)
		{
			if(rectSeparatorMouseOver.Contains(new Vector2(xClick, yClick)))
			{
				if(isLeftClick)
				{
					_isDraggingSeparator = true;
					_instance.wantsMouseMove = true;
				}
				
				// dont try to process this click elsewhere
				yClick = -1.0f;
				isLeftClick = false;
				_isLeftMouseDoubleClickNewClick = false;
				isRightClick = false;
			}
		}
		
		RenderCallstack(separator, xClick, yClick, isLeftClick, isRightClick);
		
		// render main _logCache
		float widthScrollbarAreaMin = rectScrollbarComponent.xMin + 1.0f; 
		float widthScrollbarAreaMax = _xScrollMain + widthScrollbarAreaMin;
		
		if(_scrollbarCursorMain < 0)
			_scrollbarCursorMain = 0;
		else if(_scrollbarCursorMain > yScrollMaxValue)
			_scrollbarCursorMain = yScrollMaxValue;

		float yOffset = _isMainScrollbarVisible ? _scrollbarCursorMain % ENTRY_HEIGHT : 0;
		
		int indexStart = _isMainScrollbarVisible ? Mathf.FloorToInt(_scrollbarCursorMain / ENTRY_HEIGHT) : 0;
		
		float yCursor = -yOffset;
		
		int countEntriesVisible = CountMainEntriesVisible(indexStart);
		
		DateTime now = DateTime.Now;
		
		// if there is a change to the console logs, either the last line will change (or the total number of _logCache will change) (I think).  If the last line changes, the status text will change.
		string statusText = _bridge.GetStatusText();
		
		// check for external changes to list, and if so
		if(_logCache == null || _indexFirstEntryOnScreen != indexStart || 
		   _logCache.Count != _totalEntriesLast ||
		   _countEntriesOnScreen != countEntriesVisible// && updateLimiter_ForGetEntries > 0))
		   || 
		   (statusText != _lastStatusText) ||
		   (
		 (now - _timeLastGetEntries).TotalSeconds >= 4) // just in case something changes without the above code detecting the change (it's not clear to me if this ever happens)
		   )
		{
			_timeLastGetEntries = now;
			_lastStatusText = statusText;
			_indexFirstEntryOnScreen = indexStart;
			_countEntriesOnScreen = countEntriesVisible;
			
			try
			{
				// resize if needed
// TODO: GS				if(entries == null || entries.Length != countEntriesVisible)
//					entries = new IConsoleE_Entry[countEntriesVisible];
			}
			catch(Exception e)
			{
				UnityEngine.Debug.Log(string.Format("vars = {0} {1} {2}", countEntriesVisible, heightScrollbar, ENTRY_HEIGHT));
				throw e;
			}
			
			_bridge.GetEntries(_logCache, indexStart);
		}
		
		float toolbarHeightMinusOne = TOOLBAR_HEIGHT - 1.0f;
		float xScroll = Mathf.Round(_xScrollMain); // round off to prevent blurry pixels
		float yScroll = Mathf.Round(_yScrollMain);
		
		float heightMainArea = rectScrollbarComponent.yMax - toolbarHeightMinusOne;
		yScroll = Mathf.Clamp(yScroll, -heightMainArea, heightMainArea); // fix rendering glitch by clamping this 
		
		float yScrollIfNegative = Mathf.Min(0, yScroll);
		
		if(yScroll > 0)
			yCursor -= yScroll;
		
		Rect rectMainArea = new UnityEngine.Rect(-1.0f - xScroll, toolbarHeightMinusOne - yScrollIfNegative, rectScrollbarComponent.xMin + 1.0f + xScroll, heightMainArea + yScrollIfNegative);

		if(_textStyleMainAreaBackground == null || _textStyleMainAreaBackground.normal.background == null)
		{
			_textStyleMainAreaBackground = new GUIStyle(_textStyleEven);
			_textStyleMainAreaBackground.normal.background = Picus.Utils.Graphics.MakeTex( _isProSkin ? new Color32(55, 55, 55, 255) : new Color32(222, 222, 222, 255) );
		}
		
		BeginGroupPrivate(rectMainArea, _textStyleMainAreaBackground);
		
		// render right-side void first, because _logCache can extender over it
		if(xScroll > 0)
		{
			// show void on right-side 
			Rect rectVoid = new UnityEngine.Rect(rectScrollbarComponent.xMin, 0, xScroll + 1.0f, rectMainArea.height - yScroll);
			GUI.DrawTexture( rectVoid, GetTextStyleVoid().normal.background);
		}
		
		bool clickHandled = false;
		
		for(int i = 0; i < countEntriesVisible; i++)
		{
			if(i >= _logCache.Count)
				break;
			
			DebugConsoleItem e = null; // TODO: GS _logCache[i];
			
			if(e == null)
				break;
			
			int indexEntry = indexStart + i;
			
			bool isEven = ((indexEntry + 1) % 2) == 0; // + 1 to match Unity Console
			
			float widthUsed = Mathf.Max(widthScrollbarAreaMin - 1.0f, e.MainAreaTextWidthInPixels + 12.0f); // + some as a buffer
			
			var rect = new UnityEngine.Rect(0, yCursor, widthUsed, ENTRY_HEIGHT);
			
			float yClickAdjusted = yClickFromToolbar + yScrollIfNegative;
			
			if(yClick != -1.0f && yClickAdjusted >= rect.yMin && yClickAdjusted <= rect.yMax && yClick < separator && yClickAdjusted < rectScrollbarComponent.yMax && !_isDraggingSeparator && xClick < rectMainArea.xMax)
			{
				if(isLeftClick)// // TODO: GS || (isRightClick && options != null && options.EnableRightClickMenuInMainArea))
				{
					clickHandled = true;
					
					if(indexEntry != IndexSelected)
					{
						SelectEntryByIndex(indexEntry);
					}
					else
					{
						_scrollToSelectedItem = true;
						
						if(_isLeftMouseDoubleClickNewClick)
						{
							// open
							ClickSelectedMainEntry();
						}
					}
				}
				
				// absorb any double-click whether if was used to open an entry or just select one
				_isLeftMouseDoubleClickNewClick = false;
			}
			
			bool isSelected = indexEntry == IndexSelected;
			
			GUIStyle ts;
			
			if(isSelected)
			{
				_lastClickedItem = e;
				
				ts = _textStyleSelectedBackground;
				
				if(_selectionChanged ||
				   isLeftClick && clickHandled && _areaOnMouseDown == WindowArea.MainArea) // if user clicks on already selected item
				{
					_selectionChanged = false;
					EditorGUIUtility.PingObject(e.InstanceId);
					RepaintLater(); // make sure callstack gets updated
				}
			}
			else
			{
				ts = isEven ? _textStyleEven : _textStyleOdd;
			}
			
				
			GUI.Box(rect, "", ts);
			ts = GetStyleFromLogType(e.Type, isSelected);

			string textShown;
			
			if(e.Rows.Length == 0)
				textShown = "";
			else
			{
				if(e.Rows.Length == 1)
					textShown = e.Rows[0];
				else
				{
					textShown = e.Rows[0] + "\n" + e.SecondLineDisplayed;
				}
			}
			
			GUI.Label(rect, textShown, ts);
			
			if(_isCollapseEnabled && _textStyleCollapseCount != null)
			{
				if(e.CollapseCount > 0)
				{
					GUIContent collapseText = new GUIContent(e.CollapseCount.ToString());
					
					float collapseTextWidth = _textStyleCollapseCount.CalcSize(collapseText).x;
					
					var rectCollapse = new UnityEngine.Rect(widthScrollbarAreaMax - collapseTextWidth - 5.0f, yCursor + 6.0f, collapseTextWidth, ENTRY_HEIGHT);
					
					GUI.Label(rectCollapse, collapseText, _textStyleCollapseCount);
				}
			}
			
			yCursor += ENTRY_HEIGHT;
		}
		
		if(_indexSelectedPrevious >= _logCache.Count)
			_indexSelectedPrevious = -1;

		if(SelectedEntry != null)
		{
			if(IndexSelected == -1 || 
			   IndexSelected >= _logCache.Count ||  // external clear event
			   (isLeftClick && !clickHandled && _areaOnMouseDown == WindowArea.MainArea))
			{
				ClearMainEntrySelection(false);
			}
		}
		
		try
		{
			EndGroupPrivate();
		}
		catch(Exception e)
		{
			// sometimes this is throwing this, and I'm not sure why:
			//InvalidOperationException: Operation is not valid due to the current state of the object
			RepaintLater();
			
			return false;
		}
		
		// render left-side void later, to override a line of pixels in main _logCache (indirect way to match Unity console)
		if(xScroll < 0)
		{
			// show void on left-side 
			Rect rectVoid = new UnityEngine.Rect(0, toolbarHeightMinusOne, -xScroll, rectMainArea.height - yScroll);
			GUI.DrawTexture( rectVoid, GetTextStyleVoid().normal.background);
		}
		
		if(yScroll != 0)
		{
			if(yScroll < 0)
			{
				Rect rectVoid = new UnityEngine.Rect(rectMainArea.xMin, toolbarHeightMinusOne, rectMainArea.width, -yScroll);
				
				GUI.DrawTexture( rectVoid, GetTextStyleVoid().normal.background);
			}
			
			if(yScroll > 0)
			{
				Rect rectVoid = new UnityEngine.Rect(0, rectMainArea.yMax-yScroll, rectMainArea.width-xScroll, yScroll);
				
				GUI.DrawTexture( rectVoid, GetTextStyleVoid().normal.background);
			}
		}
		
		ShowRightClickMenuAsNeeded();
		
		// some kind of bug with slow update of this, so I am commenting out this if check
		EditorGUIUtility.AddCursorRect(rectCursorRect, MouseCursor.ResizeVertical);
		
		return true;
	}

	bool IsMouseOverCallstack(Vector2 vecRawMousePos)
	{
		return vecRawMousePos.y > GetSeparator() + 1.0f;
	}
	
	void ProcessInputEvents()
	{
		Event e = Event.current;
		
		if(e.type == EventType.MouseDown)
		{
			if((e.button == 0) && !_isLeftMouseDown)
			{
				_isLeftMouseDownNewClick = true;
				_isLeftMouseDown = true;
				
				OnMouseClickInArea(e.mousePosition);
				
				CancelDragSeparator();
				window.wantsMouseMove = true; 
				
				DateTime now = DateTime.Now;
				
				if((now - _timeLeftMouseDown).TotalMilliseconds < 500)
				{
					if((_lastLeftMouseDown - e.mousePosition).magnitude < 2.0f)
					{
						_isLeftMouseDoubleClickNewClick = true;
					}
				}
				
				_timeLeftMouseDown = now;
				_lastLeftMouseDown = e.mousePosition;
				_panPos = _lastLeftMouseDown;
			}
			
			if((e.button == 1) && !isRightMouseDown)
			{
				_isRightMouseDownNewClick = true;
				isRightMouseDown = true;
				OnMouseClickInArea(e.mousePosition);
				
				window.wantsMouseMove = true;
			}
		}
		
		// Control+Click on Mac
		if(e.type == EventType.ContextClick ||
		   (e.isKey && e.keyCode == KeyCode.Menu && !IsPlatformOsx)) // special windows support of menu key which Unity is not converting to EventType.ContextClick
		{
			if(e.type == EventType.ContextClick)
			{
				showRightClickMenu_Type = GetWindowAreaType(e.mousePosition);
				//rightClickMenuKeyboardSpawned = false;
			}
			else
			{
				showRightClickMenu_Type = _areaKeyboardFocus;
				//rightClickMenuKeyboardSpawned = true;
			}
			
			showRightClickMenu_Frames = 1;
			//ShowRightClickMenuAsNeeded();
			_isRightMouseDownNewClick = true; // treat same as mouse click
			//isContextClick_NewClick = true;
			e.Use();
		}
		
		if(e.rawType == EventType.MouseUp)// || (e.rawType == EventType.MouseUp && EditorWindow.mouseOverWindow != window))
		{
			if(e.button == 0)
			{
				isLeftMouseDown = false;
				CancelDragSeparator();
				CheckMouseMoveSetting();
			}
			
			if(e.button == 1)
			{
				//UnityEngine.Debug.Log("RIGHT UP");
				isRightMouseDown = false;
				CancelDragSeparator();
				CheckMouseMoveSetting();
			}
		}
		
		if(e.type == EventType.ScrollWheel)
		{
			if(e.delta.y < 0)
				_isScrollSnappedToEnd = false;
			
			float separator = GetSeparator();
			
			if(e.mousePosition.y < separator - 1.0f)
			{
				float speed = options != null ? options.MouseWheelScrollSpeed_MainArea : 1.0f;
				_scrollbarCursorMain += e.delta.y * GetEntryHeight() * speed * 0.625f; // finding the magic value that matches the scroll speed of the Unity console
				RepaintLater();
			}
			else if(IsMouseOverCallstack(e.mousePosition))
			{
				float speed = options != null ? options.MouseWheelScrollSpeed_CallstackArea : 1.0f;
				_scrollbarCursorCallstack += e.delta.y * speed * 13.0f;  // if scrolling by mouse wheel, it's good if the amount is divisible by the row height so that the top line is not cropped
				RepaintLater();
			}
		}
		
		if(e.type == EventType.MouseDrag) // This is mouse move with a button down.  In Unity, it's necessary to use EventType.MouseDrag for properly handling when the mouse exits the window but is still dragging
		{
			if(_isDraggingSeparator)
			{
				//UnityEngine.Debug.Log(e.delta);
				if(e.delta.y != 0)
				{
					float fPrev = ratioSeparator;
					ratioSeparator += e.delta.y / GetWindowHeight(); 
					//ratioSeparator = Mathf.Clamp(ratioSeparator, 0.0f, 1.0f);
					ratioSeparator = Mathf.Clamp(ratioSeparator, GetMinSeparatorRatio(), GetMaxSeparatorRatio());
					
					if(fPrev != ratioSeparator)
						RepaintLater();
				}
			}
		}
		else if (e.type == EventType.MouseMove) // EventType.MouseMove means movement without any buttons down -- the no buttons down part is what we really care about.  See EventType.MouseDrag for movement with buttons down.
		{
			//UnityEngine.Debug.Log("Move");
			
			isLeftMouseDown = false;
			isRightMouseDown = false;
			CancelDragSeparator();
			CheckMouseMoveSetting();
		}
		
		if(e.isMouse)
		{
			_lastMousePos = e.mousePosition;
		}
		
		if(e.isMouse || _canStartPan)
		{
			if(isLeftMouseDown && _areaOnMouseDown == WindowArea.MainArea)
			{
				float dx = e.mousePosition.x - _panPos.x;
				float dy = e.mousePosition.y - _panPos.y;
				
				if(dx != 0 || dy != 0)
				{
					if(options == null || options.EnableDragScrolling)
					{
						if(IsButtonHeldLongEnoughForPanning() && 
						   (_panPos != _lastLeftMouseDown || // if we already started panning
						 (_lastLeftMouseDown - e.mousePosition).magnitude >= 1.5f)) // user has moved mouse significant from basis (helps minimize unintentional pans)
						{
							_canStartPan = false;
							_xScrollMain = -(_panPos.x - _lastLeftMouseDown.x);//-Math.Sign(dx) * Mathf.Pow(Mathf.Abs(dx * 1.0f), 0.95f);
							
							_panPos = e.mousePosition;
							
							if(dy != 0)
							{
								_yScrollMain -= dy;
								
								if( _isMainScrollbarVisible )
								{
									if(_yScrollMain < 0)  // upwards movement
									{
										// transfer movement to scrollbar
										
										_scrollbarCursorMain += _yScrollMain;
										
										if(_scrollbarCursorMain < 0)
										{
											_yScrollMain = _scrollbarCursorMain;
											_scrollbarCursorMain = 0;
										}
										else
										{
											_yScrollMain = 0;
										}
									}
									else // downwards movement
									{
										if(_yScrollMain > 0)
										{
											float max = GetMainScrollBarMaxValue();
											
											if(_scrollbarCursorMain > max)
											{
												_scrollbarCursorMain = max;
											}
											else
											{
												if(_scrollbarCursorMain < max)
												{
													// transfer movemnt from yScrollMain to scrollbarCursorMain, as much as possible
													float transfer = Mathf.Min(max - _scrollbarCursorMain, _yScrollMain);
													_scrollbarCursorMain += transfer;
													_yScrollMain -= transfer;
												}
											}
										}
									}
								}
							}
							
							RepaintLater();
						}
					}
				}
			}
		}
		
		if(e.type == EventType.ValidateCommand)
		{
			WindowArea windowArea = GetWindowAreaType(_lastMousePos);
			switch(e.commandName)
			{
			case "Copy":
				if(menus.IsCommandValid_Copy(windowArea))
					e.Use();
				break;
				
			case "SelectAll":
				if(menus.IsCommandValid_SelectAll(windowArea))
					e.Use();
				break;
			}
		}
		
		if(e.type == EventType.ExecuteCommand)
		{
			if(SelectedEntry != null)
			{
				WindowArea windowArea = GetWindowAreaType(_lastMousePos);
				switch(e.commandName)
				{
				case "Copy":
				{
					if(menus.UseCommand_Copy(windowArea))
						e.Use();
				}
					break;
				case "SelectAll":
				{
					if(menus.UseCommand_SelectAll(windowArea))
						e.Use();
				}
					break;
				}
			}
		}
		
		if(e.isKey)
		{
			if(e.type == EventType.KeyDown)
			{
				int index = IndexSelected;
				if(index == -1)
					index = _indexSelectedPrevious;
				bool selectIndex = true;
				
				switch(e.keyCode)
				{
				case KeyCode.UpArrow:
					if(_areaKeyboardFocus != WindowArea.CallstackArea)
					{
						if(IndexSelected != -1)
						{
							index--;
							_isScrollSnappedToEnd = false;
							_scrollToSelectedItem = true;
						}
					}
					else
					{
						UsePreviousCallstackIndexIfNoneSelected();
						if(IndexSelectedCallstackRow != -1 && !IsPartialTextSelectionActive)
						{
							if(IndexSelectedCallstackRow > 0)
								IndexSelectedCallstackRow--;
							scrollToSelectedItem_Callstack = true;
						}
						else
						{
							if(_isCallstackScrollbarVisible)
								_scrollbarCursorCallstack -= GetCallstackEntryHeight();
						}
					}
					break;
					
				case KeyCode.DownArrow:
					if(_areaKeyboardFocus != WindowArea.CallstackArea)
					{
						if(IndexSelected != -1)
						{
							index++;
							_scrollToSelectedItem = true;
						}
					}
					else
					{
						UsePreviousCallstackIndexIfNoneSelected();
						if(IndexSelectedCallstackRow != -1 && SelectedEntry != null && !IsPartialTextSelectionActive)
						{
							int max = SelectedEntry.Rows.Length-1;
							if(IndexSelectedCallstackRow < max)
								IndexSelectedCallstackRow++;
							scrollToSelectedItem_Callstack = true;
							
							if(_isCallstackScrollbarVisible)
							{
								if(IndexSelectedCallstackRow == max)
									_scrollbarCursorCallstack = float.MaxValue;
							}
						}
						else
						{
							if(_isCallstackScrollbarVisible)
								_scrollbarCursorCallstack += GetCallstackEntryHeight();
						}
					}
					break;
					
				case KeyCode.Home:
					if(_areaKeyboardFocus != WindowArea.CallstackArea)
					{
						index = 0;
						_scrollbarCursorMain = 0;
						_isScrollSnappedToEnd = false;
					}
					else
					{
						if(!IsPartialTextSelectionActive)
						{
							if(_logCache.Count > 0)
							{
								IndexSelectedCallstackRow = 0;
								scrollToSelectedItem_Callstack = true;
							}
						}
						else
						{
							if(_isMainScrollbarVisible)
								_scrollbarCursorCallstack = 0;
						}
					}
					break;
					
				case KeyCode.End:
					if(_areaKeyboardFocus != WindowArea.CallstackArea)
					{
						index = _logCache.Count - 1;
						_scrollbarCursorMain = GetMainScrollBarAreaHeight(_logCache.Count);
					}
					else
					{
						if(SelectedEntry != null && !IsPartialTextSelectionActive)
						{
							if(_logCache.Count > 0)
							{
								IndexSelectedCallstackRow = SelectedEntry.Rows.Length - 1;
								scrollToSelectedItem_Callstack = true;
							}
						}
						
						if(_isMainScrollbarVisible)
							_scrollbarCursorCallstack = float.MaxValue;
					}
					break;
					
				case KeyCode.PageUp:
					
					if(!IsPlatformOsx)
					{
						if(_areaKeyboardFocus != WindowArea.CallstackArea)
						{
							if(IndexSelected != -1)
							{
								index -= Mathf.Max(1, Mathf.RoundToInt(GetMainScrollBarHeight() / GetEntryHeight()));
								_isScrollSnappedToEnd = false;
								_scrollToSelectedItem = true;
							}
							else
							{
								if(_isMainScrollbarVisible)
									_scrollbarCursorMain -= GetMainScrollBarHeight();
							}
						}
						else
						{
							UsePreviousCallstackIndexIfNoneSelected();
							if(IndexSelectedCallstackRow != -1 && !IsPartialTextSelectionActive)
							{
								IndexSelectedCallstackRow -= Mathf.Max(1, Mathf.FloorToInt(GetCallstackScrollBarHeight() / GetCallstackEntryHeight()));
								if(IndexSelectedCallstackRow < 0)
									IndexSelectedCallstackRow = 0;
								scrollToSelectedItem_Callstack = true;
							}
							else
							{
								if(_isCallstackScrollbarVisible)
									_scrollbarCursorCallstack -= GetCallstackScrollBarHeight();
							}
						}
					}
					else // OSX
					{
						if(_areaKeyboardFocus != WindowArea.CallstackArea)
						{
							if(_isMainScrollbarVisible)
								_scrollbarCursorMain -= GetMainScrollBarHeight();
						}
						else
						{
							if(_isCallstackScrollbarVisible)
								_scrollbarCursorCallstack -= GetCallstackScrollBarHeight();
						}
					}
					
					break;
					
				case KeyCode.PageDown:
					
					if(!IsPlatformOsx)
					{
						if(_areaKeyboardFocus != WindowArea.CallstackArea)
						{
							if(IndexSelected != -1)
							{
								index += Mathf.Max(1, Mathf.RoundToInt(GetMainScrollBarHeight() / GetEntryHeight()));
								_scrollToSelectedItem = true;
							}
							else
							{
								if(_isMainScrollbarVisible)
									_scrollbarCursorMain += GetMainScrollBarHeight();
							}
						}
						else
						{
							UsePreviousCallstackIndexIfNoneSelected();
							if(IndexSelectedCallstackRow != -1 && SelectedEntry != null && !IsPartialTextSelectionActive)
							{
								IndexSelectedCallstackRow += Mathf.Max(1, Mathf.FloorToInt(GetCallstackScrollBarHeight() / GetCallstackEntryHeight()));
								
								int max = SelectedEntry.Rows.Length - 1;
								if(IndexSelectedCallstackRow > max)
									IndexSelectedCallstackRow = max;
								
								if(_isCallstackScrollbarVisible)
								{
									if(IndexSelectedCallstackRow == max)
										_scrollbarCursorCallstack = float.MaxValue;
									else
										scrollToSelectedItem_Callstack = true;
								}
							}
							else
							{
								if(_isCallstackScrollbarVisible)
									_scrollbarCursorCallstack += GetCallstackScrollBarHeight();
							}
						}
					}
					else // OSX
					{
						if(_areaKeyboardFocus != WindowArea.CallstackArea)
						{
							if(_isMainScrollbarVisible)
								_scrollbarCursorMain += GetMainScrollBarHeight();
						}
						else
						{
							if(_isCallstackScrollbarVisible)
								_scrollbarCursorCallstack += GetCallstackScrollBarHeight();
						}
					}
					
					break;
					
				case KeyCode.Escape:
				{
					if(_areaKeyboardFocus == WindowArea.CallstackArea)
					{
						ClearSingleLineSelectionInCallstack();
						ClearPartialTextSelection();
					}
					else
					{
						ClearMainEntrySelection(false);
					}
					selectIndex = false;
					break;
				}
					
				case KeyCode.LeftArrow:
				case KeyCode.RightArrow:
				{
					e.Use();  // turn off beep on OSX
					selectIndex = false;
				}
					break;
				default:
					selectIndex = false;
					break;
				}
				
				if(selectIndex)
				{
					SelectEntryByIndexPrivate(index);
					e.Use();
					RepaintLater();
				}
			}
		}
	}
	float GetMainScrollBarHeight()
	{
		return GetSeparator() - TOOLBAR_HEIGHT;
	}
	
	float GetSeparator()
	{
		return TOOLBAR_HEIGHT + GetWindowHeightWithoutToolbar() * Mathf.Clamp(ratioSeparator, GetMinSeparatorRatio(), GetMaxSeparatorRatio());
	}

	float GetMainScrollbarWidth()
	{
		return _isMainScrollbarVisible ? 16.0f : 0.0f;
	}
	
	float GetCallstackScrollbarWidth()
	{
		return _isCallstackScrollbarVisible ? 16.0f : 0.0f;
	}

	float GetWindowWidth()
	{
		return _instance.position.width;
	}

	float GetWindowHeight()
	{
		return window.position.height;
	}

	float GetMainScrollBarAreaHeight()
	{
		return GetMainScrollBarAreaHeight(_totalEntries);
	}

	void RenderMainScrollbar(bool isScrollbarVisible, Rect rectScrollbarComponent, float heightScrollbar, float maxScrollValue)
	{
		if(isScrollbarVisible)
		{
			var prevalue = _scrollbarCursorMain;
			
			AutoScrollAsNeeded(heightScrollbar);
			
			_scrollbarCursorMain = GUI.VerticalScrollbar( rectScrollbarComponent, _scrollbarCursorMain, heightScrollbar - 1.0f, 0, maxScrollValue);
			
			float max = maxScrollValue - heightScrollbar;// - GetEntryHeight();
			
			if(_scrollbarCursorMain >= max - 1.0f)
			{
				_scrollbarCursorMain = max;
				if(!isLeftMouseDown)
				{
					_isScrollSnappedToEnd = true;
				}
			}
			else
			{
				_isScrollSnappedToEnd = false;
			}
			
			if(prevalue != _scrollbarCursorMain)
				RepaintLater();
		}
		else
		{
			_isScrollSnappedToEnd = true;
		}
	}

	UnityEngine.Rect GetSeparatorMouseOverRect()
	{
		float separator = GetSeparator();
		
		return new UnityEngine.Rect(0, separator - 3.0f, GetWindowWidth(), 6.0f);
	}

	private void AutoScrollAsNeeded(float heightScrollbar)
	{
		if (_totalEntriesLast != _totalEntries)
		{
			CheckSnapToEnd();
		}
		
		if (_scrollToSelectedItem)
		{
			_scrollToSelectedItem = false;
			
			float selectedPos = IndexSelected * GetEntryHeight();
			
			if (selectedPos < _scrollbarCursorMain)
				_scrollbarCursorMain = selectedPos;
			else
			{
				float bottom = _scrollbarCursorMain + heightScrollbar - GetEntryHeight();
				if (selectedPos > bottom)
					_scrollbarCursorMain = selectedPos - heightScrollbar + GetEntryHeight() - 1.0f;
			}
		}
	}

	bool RenderCallstack(float separator, float xClick, float yClick, bool isLeftClick, bool isRightClick)
	{
		if(_areaOnMouseDown != WindowArea.CallstackArea)
		{
			isLeftClick = false;
			isRightClick = false;
			yClick = -1.0f;
			xClick = -1.0f;
		}
		
		var rectCallStackWithSeparator = new UnityEngine.Rect(-1.0f, separator - 1.0f, GetWindowWidth() + 1.0f, GetWindowHeight() - separator + 1.0f);
		
		//GUILayout.BeginArea(rectCallStackWithSeparator, styleSeparator);
		// styleSeparator will show a horziontal line
		BeginGroupPrivate(rectCallStackWithSeparator, _textStyleSeparator);
		
		// match Unity's callstack as best possible
		float xScroll = Mathf.Round(_xScrollCallstack); // round to prevent blurriness
		var rectCallStack = new UnityEngine.Rect(-xScroll, 1.0f, rectCallStackWithSeparator.width + xScroll, rectCallStackWithSeparator.height - 2.0f);
		//GUILayout.BeginArea(rectCallStack, textStyleCallstack);
		BeginGroupPrivate(rectCallStack, _textStyleCallstack);
		
		float yClickCallstack = yClick;
		
		if (yClickCallstack != -1.0f) 
		{
			yClickCallstack -= rectCallStackWithSeparator.yMin;
			//UnityEngine.Debug.Log("yClick= " + yClickCallstack.ToString());
			//UnityEngine.Debug.Log("rectCallStack.yMin= " + rectCallStackWithSeparator.yMin.ToString());
		}
		
		
		bool endgroup = RenderCallstackPrivate(rectCallStack, xClick, yClickCallstack, isLeftClick, isRightClick);
		
		try
		{
			if(endgroup)
				EndGroupPrivate();
			
			//GUILayout.EndArea();
			//GUILayout.EndArea();
			EndGroupPrivate();
			EndGroupPrivate();
		}
		catch(Exception e)
		{
			// sometimes this is throwing this, and I'm not sure why:
			//InvalidOperationException: Operation is not valid due to the current state of the object
			RepaintLater();
			
			#if TESTING_CONSOLE
			UnityEngine.Debug.LogException(e);
			#endif
			
			return false;
		}
		
		return true;
	}

	int CountMainEntriesVisible(int indexStart)
	{
		float heightEntry = GetEntryHeight();
		
		float heightScrollbar = GetMainScrollBarHeight();
		
		int left = _totalEntries - indexStart;
		
		return Mathf.Min(left, 1 + Mathf.CeilToInt(heightScrollbar / heightEntry));
	}

	void BeginGroupPrivate(Rect rectCallStack, GUIStyle ts)
	{
		GUI.BeginGroup(rectCallStack, ts);
		refGroup++;
	}
	
	bool EndGroupPrivate()
	{
		if(refGroup > 0)
		{
			refGroup--;
			GUI.EndGroup();
			return true;
		}
		return false;
	}

	GUIStyle GetTextStyleVoid()
	{
		if(_textStyleVoid.normal.background == null)
			_textStyleVoid.normal.background = MakeTextureWithSolidColor(_isProSkin ? new Color32(43, 43, 43, 255) : new Color32(193, 193, 193, 255));
		return _textStyleVoid;
	}

	public int IndexSelected { get { return _indexSelected; } set { SelectEntryByIndex(value); }  }

	void SelectEntryByIndex(int index) // SelectItem SelectIndex
	{
		if(index != IndexSelected)
		{
			if(index == -1)
			{
				ClearMainEntrySelection();
				return;
			}
			
			if(totalEntries == 0)
			{
				index = -1;
				//selectedEntry = null;
			}
			else
			{
				if(index >= totalEntries)
					index = totalEntries-1;
				//selectedEntry = Entries[index];
			}
			
			if(index != IndexSelected)
			{
				indexSelectedCallstackRowPrevious = -1;
				ClearSingleLineSelectionInCallstack();
				ClearPartialTextSelection();
				ClearCallstackHorizontalScroll();
				_indexSelectedPrevious = _indexSelected;
				_indexSelected = index;
				scrollbarCursorCallstack = 0;
				_selectionChanged = true;  // ensures scroll position is on-screen
				_scrollToSelectedItem = true;
			}
			
			RepaintLater();
		}
	}

	public bool ClickSelectedMainEntry()
	{
		if(SelectedEntry != null)
		{
			OnClick(SelectedEntry, SelectedEntry.IndexPrimaryRow, WindowArea.MainArea);
		}
		return false;
	}

	void RepaintLater(int count = 1)
	{
		if(repaint < count)
			repaint = count;
	}

	GUIStyle GetStyleFromLogType(LogType type, bool isSelected)
	{
		switch(type)
		{
		case LogType.Assert:
		case LogType.Error:
		case LogType.Exception:
			return isSelected ? styleEntryError_Selected : styleEntryError;
		case LogType.Warning:
			return isSelected ? styleEntryWarn_Selected : styleEntryWarn;
		default:
			return isSelected ? styleEntryInfo_Selected : styleEntryInfo;
		}
	}

	void OnMouseClickInArea(Vector2 mousePosition)
	{
		_areaOnMouseDown = GetWindowAreaType(mousePosition);
		
		if (_areaOnMouseDown == WindowArea.MainAreaScrollbar)
			_areaKeyboardFocus = WindowArea.MainArea;
		else
		{
			if (_areaOnMouseDown == WindowArea.CallstackAreaScrollbar)
				_areaKeyboardFocus = WindowArea.CallstackArea;
			else
			{
				_areaKeyboardFocus = _areaOnMouseDown;
			}
		}
	}
*/
}