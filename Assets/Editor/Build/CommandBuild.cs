using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CommandBuild {
	
	public static void BuildIOS()
	{
		//mine custom arguments from commandline
		PlayerSettings.bundleVersion = CommandLineReader.GetCustomArgument("bundleVersion", "1.0.0");
		PlayerSettings.bundleIdentifier = CommandLineReader.GetCustomArgument("bundleId");
		PlayerSettings.iPhoneBundleIdentifier = CommandLineReader.GetCustomArgument("bundleId");
		
		Debug.Log("Build iOS:");
		Debug.Log("   bundleVersion:" + PlayerSettings.bundleVersion + " bundleId:" + PlayerSettings.iPhoneBundleIdentifier);
		
		string[] levels = FindEnabledEditorScenes();
		string path = "Builds/Devices/IOS";
		
		BuildOptions opts = BuildOptions.SymlinkLibraries;
		BuildPipeline.BuildPlayer(levels, path, BuildTarget.iPhone, opts);
	}
	
	public static void BuildAndroid()
	{
		//mine custom arguments from commandline
		PlayerSettings.keyaliasPass = CommandLineReader.GetCustomArgument("keystorePass");
		PlayerSettings.keystorePass = CommandLineReader.GetCustomArgument("keystorePass");
		PlayerSettings.Android.keyaliasName = CommandLineReader.GetCustomArgument("keyAlias");
		PlayerSettings.Android.keyaliasPass = CommandLineReader.GetCustomArgument("keystorePass");
		PlayerSettings.Android.keystoreName = CommandLineReader.GetCustomArgument("keystoreName");
		PlayerSettings.Android.keystorePass = CommandLineReader.GetCustomArgument("keystorePass");
		string bundleVersionCode = CommandLineReader.GetCustomArgument("bundleVersionCode");
		PlayerSettings.Android.bundleVersionCode = Convert.ToInt32(bundleVersionCode);
		PlayerSettings.bundleVersion = CommandLineReader.GetCustomArgument("bundleVersion");
		PlayerSettings.bundleIdentifier = CommandLineReader.GetCustomArgument("bundleId");
		
		Debug.Log("Build android:");
		Debug.Log("   keystoreName:" + PlayerSettings.Android.keystoreName);
		Debug.Log("   keyaliasName:" + PlayerSettings.Android.keyaliasName);
		Debug.Log("   bundleVersion:" + PlayerSettings.bundleVersion);
		Debug.Log("   bundleVersionCode:" + PlayerSettings.Android.bundleVersionCode);
		Debug.Log("   bundleIdentifier:" + PlayerSettings.bundleIdentifier);
		
		string[] levels = FindEnabledEditorScenes();
		string product = CommandLineReader.GetCustomArgument("product", "SWCCG") + ".apk";
		string path = "Builds/Devices/Android/" + product;
		
		BuildOptions opts = BuildOptions.None;
		BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, opts);
	}
	
	private static string[] FindEnabledEditorScenes()
	{
		List<string> EditorScenes = new List<string>();
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled) continue;
			EditorScenes.Add(scene.path);
		}
		return EditorScenes.ToArray();
	}
}
