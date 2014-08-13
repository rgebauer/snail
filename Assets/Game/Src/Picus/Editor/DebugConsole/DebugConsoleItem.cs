using System;
using UnityEngine;

[System.Serializable]
public class DebugConsoleItem
{
	public string Text { get; private set; }
	public string StackFirstLine { get; private set; }
	public string Stack { get; private set; }
	public LogType Type { get; private set; }
	public int InstanceId { get; private set; }
	public string DefaultFileName { get; private set; }
	public string Filter { get; private set; }
	public int DefaultLine { get; private set; }

	public int FilterFlag; // TODO: GS use for filtering optimisation
	public float MainAreaTextWidthInPixels; // TODO: GS

	public DebugConsoleItem(string condition, int instanceId, LogType type, string defaultFileName, int defaultLine)
	{
		int firstNewLine = condition.IndexOf('\n');

		Text = condition.Substring(0, firstNewLine);
		Stack = condition.Substring(firstNewLine + 1);

		int stackNextNewLine = Stack.IndexOf('\n');
		StackFirstLine = stackNextNewLine == -1 ? Stack : Stack.Substring(0, stackNextNewLine);

		InstanceId = instanceId;
		Type = type;

		DefaultFileName = defaultFileName;
		DefaultLine = defaultLine;

		Filter = Picus.Sys.Debug.FilterNameFromText(Text);
	}
}