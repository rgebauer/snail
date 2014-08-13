using System;
using System.Collections.Generic;
using UnityEngine;

namespace Picus.Sys
{
	// TODO: GS
	// -not sending mail on windows (MH computer)

	public class Logger : MonoBehaviourExtend
	{
		public string GameNameAndVersion = "default 0.0";
		public string MailAddress = "jindrich.gottwald@disney.com";

		/// <summary>
		/// Save to disk on every log entry. (slow)
		/// </summary>
		public bool SaveImmediate = false;

		public bool SaveNewFileOnLevelChange = true;

		protected const string LastLogFileName = "last.log";
		protected const string LogFileNamePrefix = "history_";
		protected const string LogFileExtension = ".log";
		protected const string LastCrashLogSuffix = "_crashLog";
		protected const int LogFilesHistory = 10;

		private System.Text.StringBuilder _history = new System.Text.StringBuilder(10000);
		private int _lastSceneSavedIdx = 0;

		public void Clear()
		{
			_history.Length = 0;
			_lastSceneSavedIdx = 0;
		}

		/// <summary>
		/// Find last crash log (not neccessary last game) and send it if exist.
		/// </summary>
		public void SendLastCrashLogByEmail() // TODO: GS maybe not working, maybe lastcrashlog is already deleted
		{
			System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(Application.persistentDataPath);
			System.IO.FileInfo[] fileInfos = dirInfo.GetFiles(LogFileNamePrefix + "*" + LastCrashLogSuffix + LogFileExtension, System.IO.SearchOption.TopDirectoryOnly);

			if (fileInfos.Length == 0)
				return;

			Array.Sort(fileInfos, (f1, f2) => f2.LastWriteTime.CompareTo(f1.LastWriteTime));

			System.IO.FileInfo lastCrashFile = fileInfos[0];
			System.Text.StringBuilder history = new System.Text.StringBuilder(1000);
			history.Append("Last crash log " + lastCrashFile.FullName + "\n");
			using (System.IO.StreamReader reader = new System.IO.StreamReader(lastCrashFile.FullName))
			{
				history.Append(reader.ReadToEnd());
			}

			SendByEmail(history);
		}

		/// <summary>
		/// Send current log.
		/// </summary>
		public void SendByEmail()
		{
			SendByEmail(_history);
		}

		/// <summary>
		/// Saves current log to new file.
		/// </summary>
		public void SaveNewFileToDisk()
		{
			Debug.Log("Logger.SaveToDisk ver " + GameNameAndVersion + " file " + NewLogFileName());
			SaveToLogFile(NewLogFileName(), System.IO.FileMode.Create);
			PurgeOldLogFiles();
		}

		private void OnLevelWasLoaded()
		{
			if (SaveNewFileOnLevelChange)
				SaveNewFileToDisk();

			// remove last but one scene
			_history.Remove(0, _lastSceneSavedIdx);
			_lastSceneSavedIdx = _history.Length;
		}

		private void SendByEmail(System.Text.StringBuilder history)
		{
			string subject = WWW.EscapeURL(GameNameAndVersion.Replace("+", "%20"));
			string body = WWW.EscapeURL(history.ToString());
			Application.OpenURL("mailto:" + MailAddress + "?subject=" + subject + "&body=" + body.Replace("+", "%20"));
		}

		private void Start()
		{
			_history.Append(System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + " client " + GameNameAndVersion + " scene " + Application.loadedLevelName + "\n\n");
			string fullPathLastLog = FullPath(LastLogFileName);
			if (System.IO.File.Exists(fullPathLastLog))
				System.IO.File.Move(fullPathLastLog, FullPath(NewLogFileName(LastCrashLogSuffix)));
//			SaveToLogFile(fullPathLastLog, FileMode.Create); // File.Move is not atomic. wait for end if needed http://stackoverflow.com/questions/50744/wait-until-file-is-unlocked-in-net

			Debug.RegisterLogCallback(OnUnityLog);
		}

		private void OnDestroy()
		{
			Debug.UnRegisterLogCallback(OnUnityLog);

			string fullPathNewLog = FullPath(NewLogFileName());
			string fullPathLastLog = FullPath(LastLogFileName);

			if (System.IO.File.Exists(fullPathNewLog))
				System.IO.File.Delete(fullPathNewLog);

			if (System.IO.File.Exists(fullPathLastLog))
				System.IO.File.Move(fullPathLastLog, fullPathNewLog);
			else
				SaveNewFileToDisk();
		}

		private void OnApplicationPause(bool pauseStatus) // OnDestroy is not called on device when killing. Better to save it on minimization.
		{
			if (!pauseStatus)
				return; 

			SaveToLogFile(LastLogFileName, System.IO.FileMode.Create);
		}

		private void OnUnityLog(string logString, string stackTrace, LogType type) 
		{
			if (IsHighPriority(type)) 
			{
				_history.Append("Exception! ");
				_history.Append(logString);
				_history.Append("\nStack: ");
				_history.Append(stackTrace);
			}
			else
			{
				_history.Append(logString);
				_history.Append("\n");
			}

			if (SaveImmediate) // TODO: GS append only last step
				SaveToLogFile(LastLogFileName, System.IO.FileMode.Append);
			else
			{
				if (IsHighPriority(type))
					SaveToLogFile(LastLogFileName, System.IO.FileMode.Create);
			}
		}

		private bool IsHighPriority(LogType type)
		{
			return (type == LogType.Exception || type == LogType.Error || type == LogType.Assert);
		}

		private void PurgeOldLogFiles()
		{
			System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(Application.persistentDataPath);
			System.IO.FileInfo[] fileInfos = dirInfo.GetFiles(LogFileNamePrefix + "*" + LogFileExtension, System.IO.SearchOption.TopDirectoryOnly);
			Array.Sort(fileInfos, (f1, f2) => f1.LastWriteTime.CompareTo(f2.LastWriteTime));
			int cntToDelete = fileInfos.Length - LogFilesHistory;
			for (int i = 0; i < cntToDelete; ++i)
			{
				System.IO.FileInfo file = fileInfos[i];
				file.Delete();
			}
		}

		private string NewLogFileName(string mySuffix = "")
		{
			return LogFileNamePrefix + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + mySuffix + LogFileExtension;
		}

		private void SaveToLogFile(string fileName, System.IO.FileMode fileMode)
		{
			try
			{
				string fileFullName = FullPath(fileName);
				_history.Append("Saving " + fileFullName + "\n");
				System.IO.Stream stream = System.IO.File.Open(fileFullName, fileMode);
				byte[] buffer = System.Text.Encoding.ASCII.GetBytes(_history.ToString());
				stream.Write(buffer, 0, buffer.Length);
				stream.Close();
			}
			catch (Exception e)
			{
				Picus.Sys.Debug.Throw("Logger.SaveLogFile exception " + e, true);
			}
		}

		private string FullPath(string fileName)
		{
			return Application.persistentDataPath + "/" + fileName;
		}
	}
}

