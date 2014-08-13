using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class DebugConsoleSolver
{
	private readonly string[] STACK_LINE_IDENT = new string[] { "(at ", " in " };

	public void OpenStack(DebugConsoleItem cacheItem, bool tryResolveDebugStack)
	{
		List<string> stackFrames = StackFrames(cacheItem.Text, cacheItem.Stack, tryResolveDebugStack);
		OpenFileOnFirstStackFrame(stackFrames, tryResolveDebugStack, cacheItem.DefaultFileName, cacheItem.DefaultLine);
	}

	public void OpenStackFrame(DebugConsoleItem cacheItem, int line, bool tryResolveDebugStack)
	{
		string[] stackFrames = cacheItem.Stack.Split('\n');
		OpenFileOnFirstStackFrame(new List<string> { stackFrames[line] }, tryResolveDebugStack, null, 0);
	}
	
	private bool IsCoroutineStack(DebugConsoleItem item)
	{
		return item.Text.Contains(Picus.Sys.Coroutine.ManagerCrtn.CoroutineStackDebugId);
	}
	
	private List<string> StackFrames(string text, string stack, bool tryCoroutines)
	{
		if (tryCoroutines && text.StartsWith(Picus.Sys.Coroutine.ManagerCrtn.CoroutineStackDebugId))
		{
			List<string> splitted = text.Split('\n').ToList();
			//			splitted.RemoveAt(0);
			return splitted;
		}
		
		return stack.Split('\n').ToList();
	}
	
	private void OpenFileOnFirstStackFrame(List<string> stackFrames, bool ignoreDebug, string defaultFileName, int defaultLine)
	{
		string fileName = null;
		int lineNr = 0;
		
		try
		{
			
			for (int stackIdx = 0, stackCnt = stackFrames.Count; stackIdx < stackCnt; ++stackIdx)
			{
				if (ignoreDebug && stackFrames[stackIdx].Contains("/Sys/Debug"))
					continue;
				
				if (stackFrames[stackIdx].Contains("mono-runtime-and-classlibs") || stackFrames[stackIdx].Contains("filename unknown>")) // internal code
					continue;
				
				for (int identIdx = 0, identCnt = STACK_LINE_IDENT.Length; identIdx < identCnt; ++identIdx)
				{
					if (stackFrames[stackIdx].Contains(STACK_LINE_IDENT[identIdx]))
					{
						string[] fileNameLine = stackFrames[stackIdx].Split(STACK_LINE_IDENT, System.StringSplitOptions.None );
						if (fileNameLine.Length != 2) // not a stack line
							continue;
						string[] fileLink = fileNameLine[1].Split(':'); // fileName:lineNr)
						if (fileLink.Length != 2)
							continue;
						fileLink[1] = fileLink[1].TrimEnd(')');
						
						fileName = fileLink[0];
						lineNr = System.Convert.ToInt32(fileLink[1]);
						break;
					}
				}
				
				if (fileName != null)
					break;
			}
			
			if (fileName != null)
				UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fileName, lineNr);
			else
				UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(defaultFileName, defaultLine);
		}
		catch (System.Exception e)
		{
			Picus.Sys.Debug.Throw(e.ToString(), true);
		}
	}
}