using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class CountLinesOfCode : OdinEditorWindow
{
	[FolderPath] public List<string> foldersToIgnore = new();

	[MenuItem("Tools/Utilities/Count Lines of Code")]
	public static void ShowWindow()
	{
		GetWindow<CountLinesOfCode>("Count Lines of Code");
	}

	[Button]
	private void CountLines()
	{
		string assetsPath = Application.dataPath;
		string[] allScripts = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);
		var foldersToIgnorePaths = foldersToIgnore.Select(folder => Path.Combine(assetsPath, folder)).Where(Directory.Exists).ToList();

		int totalLines = (from script in allScripts let ignore = foldersToIgnorePaths.Any(script.StartsWith) where !ignore select File.ReadAllLines(script) into lines select lines.Length).Sum();

		Debug.Log("Total lines of code: " + totalLines);
	}
}