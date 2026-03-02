using UnityEditor;
using UnityEngine;

public class ComponentEditor : MonoBehaviour
{
    [ComponentHeaderItem]
    private void DrawHeaderItem(Rect rect)
    {
        GUIStyle iconStyle = new GUIStyle(GUI.skin.GetStyle("IconButton"));
     
        GUIStyle labelStyle = new GUIStyle { normal = { textColor = Color.yellow } };
        if (GUI.Button(rect, new GUIContent("", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "It's a custom button"), iconStyle))
        {
            Undo.DestroyObjectImmediate(this);
        }
        
        rect.x -= 110;
        rect.width = 110;
        GUI.Label(rect, new GUIContent("Custom Button =>", "It's a custom label"), labelStyle);
    }
}