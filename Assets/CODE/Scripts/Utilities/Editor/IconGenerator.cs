using System.IO;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;

public class IconGenerator : OdinEditorWindow
{
	[Header("Prefabs")] [Tooltip("IconGenerator will generate icons from these prefabs / objects")]
	public Target[] targets;

	[Tooltip("These images will be applied to EVERY icon generated. Higher index = on top")]
	public Sprite[] overlays;

	[Tooltip("Custom folder used to save the icon. LEAVE EMPTY FOR DEFAULT!")]
	public string customFolder = ""; // Create the folder manually before running

	[Header("Debugging")] public Texture rawIcon;
	public Texture2D icon;
	public List<Texture2D> overlayIcons = new();

	private Texture2D finalIcon;
	private byte[] overlayBytes;

	[MenuItem("Tools/Utilities/Icon Generator")]
	public static void OpenWindow()
	{
		IconGenerator window = GetWindow<IconGenerator>();
		window.titleContent = new GUIContent("Icon Generator");
	}

	[Button(ButtonSizes.Large)]
	public void Generate()
	{
		GetOverlayTextures();
		int targetCount = 0;

		if (targets == null || targets.Length == 0)
		{
			Debug.LogError("You need to specify targets!");
			return;
		}

		string folderPath = string.IsNullOrEmpty(customFolder) ? Application.dataPath : Path.Combine(Application.dataPath, customFolder);

		if (!Directory.Exists(folderPath))
		{
			Debug.LogError("Could not find the directory " + folderPath + ". Please create it first!");
			return;
		}

		foreach (Target target in targets)
		{
			if (!target.Prefab)
			{
				// Skip if no prefab is assigned.
				continue;
			}

			GameObject targetObj = target.Prefab;
			rawIcon = AssetPreview.GetAssetPreview(targetObj);
			icon = (Texture2D)rawIcon;

			if (overlayIcons.Count != 0)
			{
				if (!icon)
				{
					Debug.LogError("There was an error generating image from " + targetObj.name + "! Are you sure this is a 3D object?");
					continue;
				}

				icon = GetFinalTexture(icon, targetCount);
			}
			else
			{
				if (!icon)
				{
					Debug.LogError("There was an error generating image from " + targetObj.name + "! Are you sure this is a 3D object?");
					continue;
				}
			}

			// Optionally, you can rescale the icon here.
			byte[] bytes = icon.EncodeToPNG();

			// Use the target name if custom name is empty.
			string iconName = string.IsNullOrWhiteSpace(target.Name) ? targetObj.name : target.Name;
			string iconPath = string.IsNullOrEmpty(customFolder) ? Path.Combine(Application.dataPath, iconName + ".png") : Path.Combine(folderPath, iconName + ".png");

			File.WriteAllBytes(iconPath, bytes);

			TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(iconPath);
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Single;
			EditorUtility.SetDirty(importer);
			AssetDatabase.ImportAsset(iconPath);

			Debug.Log("File saved in: " + iconPath);

			targetCount++;
		}

		AssetDatabase.Refresh();
	}

	private void GetOverlayTextures()
	{
		overlayIcons.Clear();

		foreach (var overlay in overlays)
		{
			if (!overlay)
				continue;

			string overlayPath = AssetDatabase.GetAssetPath(overlay);
			byte[] fileData = File.ReadAllBytes(overlayPath);
			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(fileData);
			// If needed, rescale the overlay to 128x128 pixels.
			if (tex.height != 128)
			{
				TextureScale.Point(tex, 128, 128);
			}

			overlayIcons.Add(tex);
		}
	}

	private Texture2D GetFinalTexture(Texture2D texture, int id)
	{
		finalIcon = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);

		// Blend all overlay textures on top of the base icon.
		for (int i = 0; i < overlayIcons.Count; i++)
		{
			CombineTextures(finalIcon, i == 0 ? texture : finalIcon, overlayIcons[i]);
		}

		return finalIcon;
	}

	public void CombineTextures(Texture2D finalTex, Texture2D baseTex, Texture2D overlay)
	{
		Vector2 offset = new Vector2((finalTex.width - overlay.width) / 2, (finalTex.height - overlay.height) / 2);

		finalTex.SetPixels(baseTex.GetPixels());

		for (int y = 0; y < overlay.height; y++)
		{
			for (int x = 0; x < overlay.width; x++)
			{
				Color overlayPixel = overlay.GetPixel(x, y) * overlay.GetPixel(x, y).a;
				Color basePixel = finalTex.GetPixel(x + (int)offset.x, y + (int)offset.y) * (1 - overlayPixel.a);
				finalTex.SetPixel(x + (int)offset.x, y + (int)offset.y, basePixel + overlayPixel);
			}
		}

		finalTex.Apply();
	}

	[System.Serializable]
	public struct Target
	{
		[InfoBox("If the name value is empty, prefab name will be used as the filename!")]
		public string Name;

		[AssetsOnly] public GameObject Prefab;
	}
}