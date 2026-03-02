using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Sirenix.OdinInspector.Editor;
using Utilities.Extensions;

[InitializeOnLoad]
public static class BoundsHandleSceneHook
{
    static BoundsHandleSceneHook()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        var drawer = BoundsHandlesAttributeDrawer.CurrentDrawer;
        if (drawer != null)
        {
            if (drawer.Attribute.drawOnSelected)
            {
                Component component = drawer.Property.Tree.WeakTargets[0] as Component;
                if (component && component.gameObject == Selection.activeGameObject) drawer.DrawHandles();
            }
            else drawer.DrawHandles();
        }
    }
}

public class BoundsHandlesAttributeDrawer : OdinAttributeDrawer<BoundsHandleAttribute, Bounds>
{
    public static BoundsHandlesAttributeDrawer CurrentDrawer { get; private set; }

    private readonly BoxBoundsHandle boundsHandle = new();

    protected override void DrawPropertyLayout(GUIContent label)
    {
        CurrentDrawer = this;
        CallNextDrawer(label);
    }

    public void DrawHandles()
    {
        if (Property.Tree.WeakTargets.IsNullOrEmpty()) return;
        var comp = Property.Tree.WeakTargets[0] as Component;
        if (!comp) return;

        var b = ValueEntry.SmartValue;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        
        using (new Handles.DrawingScope(comp.transform.localToWorldMatrix))
        {
            boundsHandle.center = b.center;
            boundsHandle.size = b.size;

            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(comp, "Changed Bounds size via scene view");
                ValueEntry.SmartValue = new Bounds(boundsHandle.center, boundsHandle.size);
                GUI.changed = true;
            }
        }

        if (Attribute.drawSolidCube) DrawSolidCube(comp.transform.localToWorldMatrix, b);
    }

    public static void DrawSolidCube(Matrix4x4 matrix, Bounds bounds)
    {
        using (new Handles.DrawingScope(matrix))
        {
            Color faceColor = new Color(0f, 1f, 0f, 0.15f);
            var corners = GetBoxCorners();

            // Draw all six faces of the bounding box
            Handles.DrawSolidRectangleWithOutline(new[] { corners[0], corners[1], corners[2], corners[3] }, faceColor, Color.green); // Bottom
            Handles.DrawSolidRectangleWithOutline(new[] { corners[4], corners[5], corners[6], corners[7] }, faceColor, Color.green); // Top
            Handles.DrawSolidRectangleWithOutline(new[] { corners[0], corners[3], corners[7], corners[4] }, faceColor, Color.green); // Left
            Handles.DrawSolidRectangleWithOutline(new[] { corners[1], corners[2], corners[6], corners[5] }, faceColor, Color.green); // Right
            Handles.DrawSolidRectangleWithOutline(new[] { corners[3], corners[2], corners[6], corners[7] }, faceColor, Color.green); // Front
            Handles.DrawSolidRectangleWithOutline(new[] { corners[0], corners[1], corners[5], corners[4] }, faceColor, Color.green); // Back
        }

        Vector3[] GetBoxCorners()
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            return new[]
            {
                center + new Vector3(-extents.x, -extents.y, -extents.z), // 0 Bottom-Left-Back
                center + new Vector3(extents.x, -extents.y, -extents.z), // 1 Bottom-Right-Back
                center + new Vector3(extents.x, -extents.y, extents.z), // 2 Bottom-Right-Front
                center + new Vector3(-extents.x, -extents.y, extents.z), // 3 Bottom-Left-Front

                center + new Vector3(-extents.x, extents.y, -extents.z), // 4 Top-Left-Back
                center + new Vector3(extents.x, extents.y, -extents.z), // 5 Top-Right-Back
                center + new Vector3(extents.x, extents.y, extents.z), // 6 Top-Right-Front
                center + new Vector3(-extents.x, extents.y, extents.z) // 7 Top-Left-Front
            };
        }
    }
}

#endif

[AttributeUsage(AttributeTargets.Field)]
public class BoundsHandleAttribute : Attribute
{
    public readonly bool drawOnSelected;
    public readonly bool drawSolidCube;
    
    public BoundsHandleAttribute(bool DrawOnSelected = true, bool DrawSolidCube = false)
    {
        drawSolidCube = DrawSolidCube;
        drawOnSelected = DrawOnSelected;
    }
}