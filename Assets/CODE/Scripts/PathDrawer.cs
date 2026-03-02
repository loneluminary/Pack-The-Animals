using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Extensions;

[System.Serializable]
public class PathDrawer
{
    [Title("Visual")]
    public LineRenderer PathRenderer;
    public LineRenderer HintPathRenderer;

    [Title("Events")]
    public UnityEvent<List<Vector3>> OnPathComplete;
    public UnityEvent OnPathCancelled;

    [ReadOnly] public bool IsDrawing;
    [ReadOnly, ShowInInspector] public readonly List<Vector3> Points = new();

    [HideInInspector] public Vector3Int LastCell;
    
    private static Grid Grid => LevelGenerator.Instance.Grid;

    public void OnStartDraw()
    {
        if (PathRenderer) PathRenderer.positionCount = 0;
        if (HintPathRenderer) HintPathRenderer.positionCount = 0;
        Points.Clear();
        IsDrawing = true;
        
        Vector3 world = RuntimeUtilities.GetPlaneWorldPoint3D(Camera.main);
        LastCell = Grid.WorldToCell(world);
        AddPoint(Grid.GetCellCenterWorld(LastCell).WithY(0.75f));
    }

    public void OnContinueDraw()
    {
        if (!IsDrawing) return;

        Vector3 world = RuntimeUtilities.GetPlaneWorldPoint3D(Camera.main);
        Vector3Int currentCell = Grid.WorldToCell(world);
        Vector3 center = Grid.GetCellCenterWorld(currentCell).WithY(0.75f);

        if (!LevelGenerator.Instance.IsCellInsideLevel(currentCell)) return;
        if (IsObstacleBetween(LastCell, currentCell)) { if (PathRenderer) PathRenderer.endColor = Color.red; return; }
        if (IsSkippingTiles(LastCell, currentCell)) return;

        if (PathRenderer) PathRenderer.endColor = Color.white;

        // backtrack: if this cell already exists in points, remove until that cell
        if (Points.Contains(center))
        {
            RemoveBackToCell(center);
            LastCell = currentCell;
            return;
        }

        // forward step
        if (currentCell != LastCell)
        {
            AddPoint(center);
            LastCell = currentCell;
        }
    }
    
    public void OnEndDraw()
    {
        IsDrawing = false;
        if (Points.Count > 1) OnPathComplete?.Invoke(Points);
        else OnPathCancelled?.Invoke();
        
        if (PathRenderer) PathRenderer.positionCount = 0;
    }

    private void AddPoint(Vector3 pos)
    {
        Points.Add(pos);

        if (PathRenderer)
        {
            PathRenderer.positionCount = Points.Count;
            PathRenderer.SetPosition(Points.Count - 1, pos.WithY(2f));
        }
    }

    private void RemoveBackToCell(Vector3 center)
    {
        // remove until the last point equals center (keeps center as last)
        while (Points.Count > 0 && Points[^1] != center) Points.RemoveAt(Points.Count - 1);

        if (PathRenderer)
        {
            PathRenderer.positionCount = Points.Count;
            for (int i = 0; i < Points.Count; i++) PathRenderer.SetPosition(i, Points[i].WithY(2f));
        }
    }

    #region Helper Method
    
    private static bool IsSkippingTiles(Vector3Int from, Vector3Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        int dz = Mathf.Abs(from.z - to.z);
    
        // if the total difference across all axes is more than 1, we're skipping a cell or more.
        return dx + dy + dz > 1;
    }

    private static bool IsObstacleBetween(Vector3Int from, Vector3Int to)
    {
        Vector3 fromPos = Grid.GetCellCenterWorld(from).WithY(0.3f);
        Vector3 toPos = Grid.GetCellCenterWorld(to).WithY(0.3f);
        if (Physics.Linecast(fromPos, toPos, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out Box box))
            {
                if (box.ID != SelectionManager.Instance.SelectedAnimal.ID) return true; // if we're hitting different color box, we can't draw.
            }
            else return true; // if we hit something that's not a box or passing through anything, we can't draw.
        }
        
        return false;
    }

    #endregion
}