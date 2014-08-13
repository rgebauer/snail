using UnityEngine;
using System;

namespace Picus.Sys
{
	public class Options
	{	
		public Options()
		{
			Load();
		}

		public void Load()
		{
			// load user pref
			System.Reflection.FieldInfo[] fieldInfos = GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			int cnt = fieldInfos.Length;
			for (int i = 0; i < cnt; ++i)
			{
				System.Reflection.FieldInfo fieldInfo = fieldInfos[i];
				if (!PlayerPrefs.HasKey(fieldInfo.Name))
					continue;

				System.Type type = fieldInfo.FieldType;

				if (type == typeof(bool))
				{
					bool val = Convert.ToBoolean(PlayerPrefs.GetInt(fieldInfo.Name));
					fieldInfo.SetValue(this, val); // if options changed to struct, use SetValueDirect with ref
				}
				else if (type == typeof(Single))
				{
					float val = Convert.ToSingle(PlayerPrefs.GetFloat(fieldInfo.Name));
					fieldInfo.SetValue(this, val); // if options changed to struct, use SetValueDirect with ref
				}
				else
					Picus.Sys.Debug.Throw("Options.Load not implemented for type " + type, true);
			}	
		}

		public void Save()
		{
			System.Reflection.FieldInfo[] fieldInfos = GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			
			int cnt = fieldInfos.Length;
			for (int i = 0; i < cnt; ++i)
			{
				System.Reflection.FieldInfo fieldInfo = fieldInfos[i];

				System.Type type = fieldInfo.FieldType;
				
				if (type == typeof(bool))
				{
					bool val = (bool)fieldInfo.GetValue(this);
					PlayerPrefs.SetInt(fieldInfo.Name, Convert.ToInt32(val));
				}
				else if (type == typeof(Single))
				{
					float val = (float)fieldInfo.GetValue(this);
					PlayerPrefs.SetFloat(fieldInfo.Name, Convert.ToSingle(val));
				}
				else
					Picus.Sys.Debug.Throw("Options.Save not implemented for type " + type, true);
			}	

			PlayerPrefs.Save();
		}

		public void Set(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
		}

		public void DeleteKey(string key)
		{
			PlayerPrefs.DeleteKey (key);
		}

		public string Get(string key)
		{
			return PlayerPrefs.GetString(key);
		}

		public bool Has(string key)
		{
			return PlayerPrefs.HasKey(key);
		}

	}
}