using System;
using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DebugConsoleBridge // reflection bridge into original console entries
{
	Assembly _assembly;

	// entries array
	Type _typeLogEntries;
	MethodInfo _startGettingEntriesMethod;
	MethodInfo _getEntryInternalMethod;
	MethodInfo _endGettingEntriesMethod;
	MethodInfo _getCountMethod;
	MethodInfo _clearMethod;
	MethodInfo _setConsoleFlagMethod;
	MethodInfo _getConsoleFlagsMethod;
	MethodInfo _getCountsByTypeMethod;
	MethodInfo _getStyleMethod;
	MethodInfo _getStatusTextMethod;

	// one entry
	Type _typeLogEntry;
	FieldInfo _conditionField;
	FieldInfo _fileField;
	FieldInfo _lineField;
	FieldInfo _modeField;
	FieldInfo _instanceIdField;

	[Flags]
	enum UnityModeFlags
	{
		PreventDataCorruption = 1, // Setting the parent of a transform which resides in a prefab is disabled to prevent data corruption.
		InternalErrorValue = 2, // mode was == 2 in SVF's project, "fence != expectedFence" UnityEditor.DockArea:OnGUI()
		MonoBit = 4, // compiler error or warning, or run-time exception
		UnityWarning = 128, // seen with "inconsistant endings
		LogErrorBit = 256,
		LogWarningBit = 512,
		LogInfo = 1024,
		MonoError = 2048,
		MonoWarning = 4096,
	}

	[Flags]
	public enum ToolbarFlags
	{
		Collapse = 1,
		ClearOnPlay = 2,
		ErrorPause = 4,
		ShowInfos = 128,
		ShowWarnings = 256,
		ShowErrors = 512,
	}
	
	object[] _getEntryInternalParams;
	
	public DebugConsoleBridge()
	{
		_assembly = Assembly.GetAssembly(typeof(SceneView));

		_typeLogEntries = _assembly.GetType("UnityEditorInternal.LogEntries");

		_startGettingEntriesMethod = _typeLogEntries.GetMethod("StartGettingEntries");
		_getEntryInternalMethod = _typeLogEntries.GetMethod("GetEntryInternal");
		_endGettingEntriesMethod = _typeLogEntries.GetMethod("EndGettingEntries");
		_getCountMethod = _typeLogEntries.GetMethod("GetCount");
		_clearMethod = _typeLogEntries.GetMethod("Clear");
		_getCountsByTypeMethod = _typeLogEntries.GetMethod("GetCountsByType");
		_setConsoleFlagMethod = _typeLogEntries.GetMethod("SetConsoleFlag");
		_getConsoleFlagsMethod = _typeLogEntries.GetMethod("get_consoleFlags");
		_getStatusTextMethod = _typeLogEntries.GetMethod("GetStatusText");

		_typeLogEntry = _assembly.GetType("UnityEditorInternal.LogEntry");

		_conditionField = _typeLogEntry.GetField("condition");
		_modeField = _typeLogEntry.GetField("mode");
		_instanceIdField = _typeLogEntry.GetField("instanceID");
		_fileField = _typeLogEntry.GetField("file");
		_lineField = _typeLogEntry.GetField("line");

		Type typeStyles = typeof(EditorGUIUtility);
		if(typeStyles != null)
			_getStyleMethod = typeStyles.GetMethod("GetStyle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Static);

		
		object entryPlaceholder = Activator.CreateInstance(_typeLogEntry);
  		_getEntryInternalParams = new object[2] { 0, entryPlaceholder };
	}

	public string GetStatusText()
	{
		return _getStatusTextMethod != null ? _getStatusTextMethod.Invoke(null, null) as string : null;
	}
	
	public void Clear()
	{
		_clearMethod.Invoke(null, null);
	}

	public int CurrentToolbarFlags()
	{
		return (int) _getConsoleFlagsMethod.Invoke(null, null);
	}

	/**
	 * Get entries from console starting on entry id @a fromId and add it into @a items.
	 * @param items List of readed console items.
	 * @param fromId Start reading from this id.
	 * @return Number of readed entries.
	 */
	public int GetEntries(LinkedList<DebugConsoleItem> items, int fromId)
	{
		_startGettingEntriesMethod.Invoke(null, null);

		int cnt = EntriesCount();
		int readedCnt = 0;

		for (int i = fromId; i < cnt; ++i, ++readedCnt)
		{
			_getEntryInternalParams[0] = i;
			object entry = _getEntryInternalMethod.Invoke(null, _getEntryInternalParams);
			if (entry != null)
			{
				object unityEntry = _getEntryInternalParams[1];
				string condition = _conditionField.GetValue(unityEntry) as string;
				int instanceId = (int)_instanceIdField.GetValue(unityEntry);
				string fileName = _fileField.GetValue(unityEntry) as string;
				int line = (int)_lineField.GetValue(unityEntry);
				UnityEngine.LogType logType = LogTypeFromEntryType((int)_modeField.GetValue(unityEntry));
				DebugConsoleItem item = new DebugConsoleItem(condition, instanceId, logType, fileName, line);

				items.AddLast(item);
			}
		}

		_endGettingEntriesMethod.Invoke(null, null);

		return readedCnt;
	}

	public int EntriesCount()
	{
		object cnt = _getCountMethod.Invoke(null, null);
		return cnt == null ? -1 : (int)cnt;
	}

	public static LogType LogTypeFromEntryType(int mode)
	{
		bool isMonoRelated = (mode & (int) UnityModeFlags.MonoBit) != 0;
			
		if(!isMonoRelated)
		{
			if((mode & (int) UnityModeFlags.PreventDataCorruption) != 0)
				return LogType.Error;
			else if((mode & (int) UnityModeFlags.LogErrorBit) != 0 || mode == (int) UnityModeFlags.InternalErrorValue)
				return LogType.Error;
			else if((mode & (int) UnityModeFlags.LogWarningBit) != 0)
				return LogType.Warning;
			else if((mode & (int) UnityModeFlags.LogInfo) != 0)
				return LogType.Log;
			else
				return LogType.Log;
		}
		else
		{
			if((mode & (int) UnityModeFlags.MonoError) != 0)
				return LogType.Error;
			else if((mode & (int) UnityModeFlags.MonoWarning) != 0)
				return LogType.Warning;
			else if((mode & (int) UnityModeFlags.LogErrorBit) != 0)
				return LogType.Exception;
			else if((mode & (int) UnityModeFlags.UnityWarning) != 0)
				return LogType.Warning;
			else
				return LogType.Log;  
		}
	}

	public void SetFlag(ref int flagsConsoleToolbar, ToolbarFlags flag, bool set)
	{
		if (set)
			flagsConsoleToolbar |= (int)flag;
		else
			flagsConsoleToolbar &= (int) ~flag;
		
		if(_setConsoleFlagMethod != null)
			_setConsoleFlagMethod.Invoke(null, new object[] { (int)flag, set });
	}

	public void CountsByType(out int logCnt, out int logWarning, out int logError)
	{
		object[] entryCounts = { (int) 0, (int) 0, (int) 0 };

		if(_getCountsByTypeMethod != null)
			_getCountsByTypeMethod.Invoke(null, entryCounts);

		logCnt = (int)entryCounts[2];
		logWarning = (int)entryCounts[1];
		logError = (int)entryCounts[0];
	}

	public bool IsFlagEnabled(int flags, ToolbarFlags flag)
	{
		return (flags & (int) flag) != 0;
	}

	public GUIStyle UnityStyle(string name) // can only be called from OnGUI
	{
		return _getStyleMethod.Invoke(null, new object[] { name } ) as GUIStyle;
	}

}