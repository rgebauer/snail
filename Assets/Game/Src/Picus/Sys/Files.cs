using UnityEngine;

namespace Picus.Sys
{
	public class Files
	{	
		static public bool Exists(string filePath)
		{
			return System.IO.File.Exists(GetDataPath(filePath));
		}

		static public void Delete(string filePath)
		{
			System.IO.File.Delete(GetDataPath(filePath));
		}

		static public string ReadAllText(string filePath)
		{
			return System.IO.File.ReadAllText(GetDataPath(filePath));
		}

		static public void WriteAllBytes(string filePath, byte[] data)
		{
			string dataPath = GetDataPath(filePath);
			CreateDataFolder(dataPath);
			System.IO.File.WriteAllBytes(dataPath, data);
		}

		static string CreateDataFolder(string dataPath)
		{
			string folder = System.IO.Path.GetDirectoryName(dataPath);
			if (!System.IO.Directory.Exists(folder))
				System.IO.Directory.CreateDirectory(folder);
			return dataPath;
		}

		static string GetDataPath(string filePath)
		{
			return Application.persistentDataPath + "/" + filePath;
		}
	}
}
