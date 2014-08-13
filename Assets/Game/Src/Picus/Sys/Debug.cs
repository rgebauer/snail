using System;
using System.Collections.Generic;

// TODO: GS optimise with stringbuilder

#if !STANDALONE
using UnityEngine;

/**
 * Handle log messages without Picus namespace. 
 * Now, if You want to use original unity log without Picus handler, You must use UnityEngine namespace.
 */
public static class Debug 
{	
	public static void Log (object message)
	{   
		Picus.Sys.Debug.LogUsingStack(message.ToString(), 2, LogType.Log);
	}

	public static void Log(object message, UnityEngine.Object context)
	{   
		Picus.Sys.Debug.LogUsingStack(message.ToString(), 1, LogType.Log, context);
	}

	public static void LogError(object message)
	{   
		Picus.Sys.Debug.LogUsingStack(message.ToString(), 1, LogType.Error);
	}

	public static void LogError (object message, UnityEngine.Object context)
	{   
		Picus.Sys.Debug.LogUsingStack(message.ToString(), 1, LogType.Error, context);
	}

	public static void LogWarning (object message)
	{   
		Picus.Sys.Debug.LogUsingStack(message.ToString(), 1, LogType.Warning);
	}
	
	public static void LogWarning (object message, UnityEngine.Object context)
	{   
		Picus.Sys.Debug.LogUsingStack(message.ToString(), 1, LogType.Warning, context);
	}

	public static void Assert(bool cond, object message = null, bool onlyLog = false)
	{
		Picus.Sys.Debug.Assert(cond, message as string, onlyLog);
	}

	public static void Throw(object message = null, bool onlyLog = false)
	{
		Picus.Sys.Debug.Throw(message as string, onlyLog);
	}
}
#endif

namespace Picus.Sys
{
	// TODO: GS disable on release

	/* Similar to class with some new features:
	 * -added log time
	 * -added callers method namespace for later filtering
	 * -DoNotLogFilter for spammers
	 */
    public class Debug
    {
		/** Namespace containing this will be filtered out. */
		public static string[] DoNotLogFilter = null; // new string[1] { "Picus.Sys.Coroutine" };

		public static readonly string[] FILTER_START_DELIMITER = new string[] { "] " };
		public static readonly string[] FILTER_END_DELIMITER = new string[] { ": " };

		public delegate void OnUnityLogDelegate(string logString, string stackTrace, LogType type);

        public enum LogColor
        {
            Black,
            DarkBlue,
            DarkGreen,
            DarkCyan,
            DarkRed,
            DarkMagenta,
            DarkYellow,
            Gray,
            DarkGray,
            Blue,
            Green,
            Cyan,
            Red,
            Magenta,
            Yellow,
            White
        }

        #if !STANDALONE
		private class LogInfo
		{
			public string Name { get; private set; }
			public float StartTime { get; private set; }
			public string FilterName { get; private set; }
			public float LastStepTime { get; private set; }

			public LogInfo(string name, string filterName)
			{
				StartTime = Time.realtimeSinceStartup;
				LastStepTime = StartTime;
				Name = name;
				FilterName = filterName;
			}

			public void Step()
			{
				LastStepTime = Time.realtimeSinceStartup;
			}
		}
#endif
        #if !STANDALONE
		
		private static Stack<LogInfo> _logInfos = new Stack<LogInfo>();

		private static List<OnUnityLogDelegate> _onUnityLogDelegates = new List<OnUnityLogDelegate>();

		/** Do not use classic Application.RegisterLogCallback. With this more delegates can be defined. */
		[ExecuteInEditMode] // TODO: GS why on game stop _onUnityLogDelegates is cleared? (maybe delegates just cant be serialized, maybe removing from namespace should happen, regards ConsoleE info)
		public static void RegisterLogCallback(OnUnityLogDelegate onUnityLog)
		{
			_onUnityLogDelegates.Add(onUnityLog);

			if (_onUnityLogDelegates.Count == 1)
				Application.RegisterLogCallback(OnUnityLog);
		}

		[ExecuteInEditMode]
		public static void UnRegisterLogCallback(OnUnityLogDelegate onUnityLog)
		{
			_onUnityLogDelegates.Remove(onUnityLog);

			if (_onUnityLogDelegates.Count == 0)
				Application.RegisterLogCallback(null);
		}

		/** Time measure for code section. */
//		[System.Diagnostics.Conditional("DEBUG")]
		public static void StartLogSection(string name)
		{
			string filterName = FilterName();
			if (IsFilteredOut(filterName))
				return;

			LogInfo logInfo = new LogInfo(name, filterName);
			_logInfos.Push(logInfo);

			WriteLog(filterName, "START" + new string('>', _logInfos.Count) + " " + name);
		}

//		[System.Diagnostics.Conditional("DEBUG")]
		public static void StopLogSection()
		{
			if (_logInfos.Count == 0)
				return;

			LogInfo logInfo = _logInfos.Pop();

			WriteLog(logInfo.FilterName, "END  " + new string('>', _logInfos.Count + 1) + " " + logInfo.Name + " duration " + (Time.realtimeSinceStartup * 1000 - logInfo.StartTime * 1000) + " ms; delta from last step " + (Time.realtimeSinceStartup * 1000 - logInfo.LastStepTime * 1000) + " ms");
		}
#endif
        #if !STANDALONE

		/** Log current section time */
//		[System.Diagnostics.Conditional("DEBUG")]
		public static void StepLogSection(string stepName)
		{
			if (_logInfos.Count == 0)
				return;

			LogInfo logInfo = _logInfos.Peek();
			WriteLog(logInfo.FilterName, "STEP " + new string('>', _logInfos.Count) + " " + stepName + " " + logInfo.Name + " duration " + (Time.realtimeSinceStartup * 1000 - logInfo.StartTime * 1000) + " ms; delta from last step " + (Time.realtimeSinceStartup * 1000 - logInfo.LastStepTime * 1000) + " ms");
			logInfo.Step();
		}
#endif

		/** Stop current section measuring. */
//		[System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool cond, string msg = "", bool onlyLog = false)
        {
            if (cond)
                return;
#if !STANDALONE
			if (onlyLog)
				WriteLog("", "LOG_ASSERT: " + msg, LogType.Assert);
#else
            if (onlyLog)
                Console.WriteLine(msg);
#endif
			else
                throw new Exception(msg);
        }

		public static void Empty()
		{
			return;
		}

		/** Like classic unity Log but with condition. */
//		[System.Diagnostics.Conditional("DEBUG")]
        public static void Log(bool cond, string msg)
        {
            if (cond)
                return;

			string filterName = FilterName();
			if (IsFilteredOut(filterName))
				return;

			WriteLog(filterName, msg);
        }

		/** Like classic unity Log. */
//		[System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string msg)
        {
		
#if STANDALONE
            Console.WriteLine(msg);
#else
			string filterName = FilterName();
			if (IsFilteredOut(filterName))
				return;

			WriteLog(filterName, msg);
#endif
        }

		/** Parse text and try to get filter name from it */
		public static string FilterNameFromText(string text)
		{
			string[] splitFilter = text.Split(Picus.Sys.Debug.FILTER_START_DELIMITER, System.StringSplitOptions.None);
			string filter = splitFilter.Length > 1 ? splitFilter[1] : "";
			splitFilter = filter.Split(Picus.Sys.Debug.FILTER_END_DELIMITER, System.StringSplitOptions.None);
			filter = splitFilter.Length > 1 ? splitFilter[0] : "";
			filter.Trim();

			for (int i = 0; i < filter.Length; i++)
				if (char.IsLower(filter[i]))
					return "";

			return filter;
		}

		/** Try to resolve filtername from deeper stack. */
		internal static void LogUsingStack(string msg, int stackDeep, LogType logType, UnityEngine.Object context = null)
		{			
			#if STANDALONE
			Console.WriteLine(msg);
			#else
			string filterName = FilterName(stackDeep);
			if (IsFilteredOut(filterName))
				return;
			
			WriteLog(filterName, msg, logType, context);
			#endif
		}

//		[System.Diagnostics.Conditional("DEBUG")]
        public static void Throw(string msg, bool onlyLog = false)
        {
#if STANDALONE
            Console.WriteLine(msg);
#else
			WriteLog("", "LOG_ASSERT: " + msg, LogType.Exception);
#endif
            if (!onlyLog)
                throw new Exception(msg);
        }

        public static string WrapInUnityColor(LogColor color, string text)
        {
            string unityColor;
            switch (color)
            {
            case LogColor.Black:
                unityColor = "black";
                break;
            case LogColor.DarkBlue:
                unityColor = "darkblue";
                break;
            case LogColor.DarkGreen:
                unityColor = "#00a000ff";
                break;
            case LogColor.DarkCyan:
                unityColor = "#00a0a0ff";
                break;
            case LogColor.DarkRed:
                unityColor = "#a00000ff";
                break;
            case LogColor.DarkMagenta:
                unityColor = "#a000a0ff";
                break;
            case LogColor.DarkYellow:
                unityColor = "#a0a000ff";
                break; 
            case LogColor.Gray:
                unityColor = "#c0c0c0ff";
                break;
            case LogColor.DarkGray:
                unityColor = "a0a0a0ff";
                break;
            case LogColor.Blue:
                unityColor = "blue";
                break;
            case LogColor.Green:
                unityColor = "green";
                break;
            case LogColor.Cyan:
                unityColor = "cyan";
                break;
            case LogColor.Red:
                unityColor = "red";
                break;
            case LogColor.Magenta:
                unityColor = "magenta";
                break;
            case LogColor.Yellow:
                unityColor = "yellow";
                break;
            case LogColor.White:
                unityColor = "white";
                break;
            default:
                unityColor = "black";
                break;
            }

            return "<color=" + unityColor + ">" + text + "</color>";
                   
        }

        #if !STANDALONE      		
		private static void WriteLog(string filterName, string text, LogType logType = LogType.Log, UnityEngine.Object context = null)
		{
			text = "[" + System.DateTime.Now + FILTER_START_DELIMITER[0] + filterName + FILTER_END_DELIMITER[0] + text;
			switch(logType)
			{
			case LogType.Assert:
			case LogType.Error:
			case LogType.Exception:
				UnityEngine.Debug.LogError(text, context);
				break;
			case LogType.Warning:
				UnityEngine.Debug.LogWarning(text, context);
				break;
			case LogType.Log:
				UnityEngine.Debug.Log(text, context);
				break;
			}
		}

		[ExecuteInEditMode]
		private static void OnUnityLog(string logString, string stackTrace, LogType type)
		{
			for (int i = 0, cnt = _onUnityLogDelegates.Count; i < cnt; ++i)
				_onUnityLogDelegates[i](logString, stackTrace, type);
		}
        #endif

		private static string FilterName(int howDeep = 1)
		{
			System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(howDeep + 1);

			if (frame == null)
				return string.Empty;

			System.Reflection.MethodBase methodInfo = frame.GetMethod();
			string ns = methodInfo.DeclaringType.Namespace;
			string filterName = ns == null ? "" : ns.ToUpper();

			return filterName;
		}

		private static bool IsFilteredOut(string ns)
		{
			if (DoNotLogFilter == null)
				return false;

			for (int i = 0, cnt = DoNotLogFilter.Length; i < cnt; ++i)
				if (ns.StartsWith(DoNotLogFilter[i], StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}
    }
}

