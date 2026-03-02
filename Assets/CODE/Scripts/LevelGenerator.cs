using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Extensions;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Grid))]
public class LevelGenerator : Singleton<LevelGenerator>
{
    [Title("Level Grid")]
    [SerializeField] private Transform tilePrefab;
    [SerializeField] private Transform tilesContainer;
    [SerializeField] private bool centerGrid = true;
    public Vector2Int LevelSize = new(5, 5);
    [HideInInspector] public Grid Grid; 
    
    [Title("Items")]
    [SerializeField] Animal animalTemplate; 
    [SerializeField] Box boxTemplate; 
    [SerializeField] List<Sprite> animals, boxes; 
    [SerializeField] Vector3 animalOffset, boxOffset;
    [SerializeField] private Transform itemsContainer;

    [ReadOnly] public bool IsBusy;

    private void Awake()
    {
        int level = PlayerPrefs.GetInt(GameManager.LEVEL_PREFS_KEY, 1);
        LevelSize += new Vector2Int(level, level);
        
        GenerateGrid(LevelSize);
        GenerateItems(level + 3);
    }

    #region A* Path Finding

    public List<Vector3> FindPath(Vector3 worldStart, Vector3 worldEnd, bool drawDebug = false)
    {
        if (Grid == null) return new();

        Vector3Int start = ClampCellToLevel(Grid.WorldToCell(worldStart));
        Vector3Int goal = ClampCellToLevel(Grid.WorldToCell(worldEnd));
        if (start == goal) return new() { Grid.GetCellCenterWorld(start).WithAddY(0.25f) };

        var open = new List<Vector3Int> { start };
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };
        var closed = new HashSet<Vector3Int>();

        while (open.Count > 0)
        {
            // pick lowest fScore
            Vector3Int current = open[0];
            for (int i = 1; i < open.Count; i++) if (fScore(open[i]) < fScore(current)) current = open[i];

            if (current == goal) return ReconstructPath(current);

            open.Remove(current);
            closed.Add(current);

            foreach (var n in GetNeighbors4(current))
            {
                if (closed.Contains(n)) continue;

                if (n != goal && IsObstacleBetween(current, n)) continue;

                int tentative = gScore[current] + 1;
                if (!gScore.TryGetValue(n, out var ng) || tentative < ng)
                {
                    cameFrom[n] = current;
                    gScore[n] = tentative;
                    if (!open.Contains(n)) open.Add(n);
                }
            }
        }

        return new();

        int fScore(Vector3Int node) => gScore.GetValueOrDefault(node, int.MaxValue) + Heuristic(node, goal);
        int Heuristic(Vector3Int a, Vector3Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        List<Vector3> ReconstructPath(Vector3Int cur)
        {
            var pathCells = new List<Vector3Int> { cur };
            while (cameFrom.ContainsKey(cur))
            {
                cur = cameFrom[cur];
                pathCells.Add(cur);
            }

            pathCells.Reverse();

            var worldPath = pathCells.Select(cell => Grid.GetCellCenterWorld(cell).WithAddY(0.25f)).ToList();

            #if UNITY_EDITOR
            if (drawDebug)
            {
                EditorUtilities.DebugDrawWireSphere(worldPath[0], Grid.cellSize.magnitude / 2, Color.red, 10f);
                EditorUtilities.DebugDrawWireSphere(worldPath[^1], Grid.cellSize.magnitude / 2, Color.green, 10f);
                for (int i = 0; i < worldPath.Count - 1; i++) Debug.DrawLine(worldPath[i], worldPath[i + 1], Color.green, 5f);
            }
            #endif
            
            return worldPath;
        }
    }
    
    private bool IsObstacleBetween(Vector3Int from, Vector3Int to)
    {
        var a = Grid.GetCellCenterWorld(from).WithAddY(0.3f);
        var b = Grid.GetCellCenterWorld(to).WithAddY(0.3f);

        // any collider counts as obstacle
        return Physics.Linecast(a, b, out RaycastHit _);
    }

    public IEnumerable<Vector3Int> GetNeighbors4(Vector3Int cell)
    {
        var cand = new[]
        {
            new Vector3Int(cell.x + 1, cell.y, cell.z),
            new Vector3Int(cell.x - 1, cell.y, cell.z),
            new Vector3Int(cell.x, cell.y + 1, cell.z),
            new Vector3Int(cell.x, cell.y - 1, cell.z)
        };

        foreach (var c in cand)
        {
            var clamped = ClampCellToLevel(c);
            if (IsCellInsideLevel(clamped)) yield return clamped;
        }
    }

    private Vector3Int ClampCellToLevel(Vector3Int cell)
    {
        int x = Mathf.Clamp(cell.x, 0, Mathf.Max(0, LevelSize.x - 1));
        int y = Mathf.Clamp(cell.y, 0, Mathf.Max(0, LevelSize.y - 1));
        return new Vector3Int(x, y, cell.z);
    }

    public bool IsCellInsideLevel(Vector3Int cell)
    {
        return cell.x >= 0 && cell.x < LevelSize.x && cell.y >= 0 && cell.y < LevelSize.y;
    }

    #endregion

    #region Grid Generation

    [Button(ButtonSizes.Large), HorizontalGroup("Grid")]
    public void GenerateGrid(Vector2Int gridSize)
    {
        if (!Grid) Grid = GetComponent<Grid>();
        if (!tilePrefab)
        {
            Debug.LogWarning("Assign tile prefab first.");
            return;
        }

        ClearGrid();
        
        if (centerGrid)
        {
            // world center of the first and last cell (respects gap & swizzle)
            Vector3Int minCell = new Vector3Int(0, 0, 0);
            Vector3Int maxCell = new Vector3Int(gridSize.x - 1, gridSize.y - 1, 0);

            Vector3 minWorld = Grid.GetCellCenterWorld(minCell);
            Vector3 maxWorld = Grid.GetCellCenterWorld(maxCell);

            Vector3 cellsCenter = (minWorld + maxWorld) * 0.5f;

            transform.position -= cellsCenter;
        }

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                var cell = new Vector3Int(x, y, 0);
                // use the true cell center and apply the computed offset
                Vector3 worldPos = Grid.GetCellCenterWorld(cell);

                Transform tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, tilesContainer);
                tile.name = $"{tile.name}{x}_{y}";
            }
        }

        Camera.main!.orthographicSize = gridSize.magnitude;
    }

    [Button(ButtonSizes.Large), HorizontalGroup("Grid")]
    public void ClearGrid()
    {
        for (int i = tilesContainer.childCount - 1; i >= 0; i--)
        {
            var child = tilesContainer.GetChild(i);
            #if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(child.gameObject);
            else
                #endif
                Destroy(child.gameObject);
        }
    }

    #endregion

    #region Item Generation

    [Button(ButtonSizes.Large), HorizontalGroup("Items")]
    public void GenerateItems(float chance)
    {
        if (!animalTemplate || !boxTemplate || animals == null || boxes == null)
        {
            Debug.LogWarning("Assign templates and sprite lists.");
            return;
        }

        if (!CollectAvailableCells(out var cells)) return;
        ClearItems();
        ShuffleCells(cells);

        var selected = new List<Vector3Int>();
        if (chance >= 1f)
        {
            int pairs = Mathf.Clamp(Mathf.RoundToInt(chance), 0, cells.Count / 2);
            if (pairs == 0) return;
            for (int i = 0; i < pairs * 2; i++) selected.Add(cells[i]);
        }
        else
        {
            selected.AddRange(cells.Where(_ => Random.value < Mathf.Clamp01(chance)));
            if (selected.Count % 2 == 1) selected.RemoveAt(Random.Range(0, selected.Count));
            if (selected.Count < 2) return;
            int maxPairs = cells.Count / 2;
            if (selected.Count / 2 > maxPairs) selected = selected.GetRange(0, maxPairs * 2);
        }

        int pairsCount = Mathf.Min(selected.Count / 2, cells.Count / 2);
        if (pairsCount == 0) return;

        // build valid types from sprite lists (index -> enum)
        int maxIndex = Mathf.Min(Mathf.Min(animals.Count, boxes.Count), Enum.GetValues(typeof(AnimalType)).Length);
        var validTypes = Enumerable.Range(0, maxIndex).Where(i => animals[i] && boxes[i]).Select(i => (AnimalType)i).ToArray();
        if (validTypes.Length == 0)
        {
            Debug.LogWarning("No valid sprite pairs.");
            return;
        }

        ShuffleCells(selected);
        
        var seq = DOTween.Sequence().OnComplete(() => IsBusy = false).SetDelay(0.25f);
        IsBusy = true;
        
        for (int i = 0; i < pairsCount; i++)
        {
            var animalCell = selected[i];
            var boxCell = selected[i + pairsCount];

            var type = validTypes[Random.Range(0, validTypes.Length)];
            var animalSprite = animals[(int)type];
            var boxSprite = boxes[(int)type];

            var animal = Instantiate(animalTemplate, Grid.GetCellCenterWorld(animalCell) + animalOffset, Quaternion.identity, itemsContainer);
            var box = Instantiate(boxTemplate, Grid.GetCellCenterWorld(boxCell) + boxOffset, Quaternion.identity, itemsContainer);

            SetSpriteOnInstance(animal.transform, animalSprite);
            SetSpriteOnInstance(box.transform, boxSprite);

            animal.ID = type;
            box.ID = type;
            
            animal.name = $"Animal_{type}_{i}";
            box.name = $"Box_{type}_{i}";
            
            animal.gameObject.SetActive(false);
            box.gameObject.SetActive(false);
            seq.Append(animal.transform.DOJump(animal.transform.position, 0.2f, 1, 0.5f).OnStart(() =>
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.PopSound); 
                box.gameObject.SetActive(true); 
                animal.gameObject.SetActive(true);
            })); 
            
            seq.Join(box.transform.DOJump(box.transform.position, 0.2f, 1, 0.5f));
        }
        
        seq.Play();
        
        static void SetSpriteOnInstance(Transform inst, Sprite sprite)
        {
            if (!inst || !sprite) return;
            var sr = inst.GetComponentInChildren<SpriteRenderer>();
            if (sr) { sr.sprite = sprite; return; }
            var img = inst.GetComponentInChildren<UnityEngine.UI.Image>();
            if (img) img.sprite = sprite;
        }
    }

    [Button(ButtonSizes.Large), HorizontalGroup("Items")]
    public void ClearItems()
    {
        for (int i = itemsContainer.childCount - 1; i >= 0; i--)
        {
            var child = itemsContainer.GetChild(i);
            #if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(child.gameObject);
            else
                #endif
                Destroy(child.gameObject);
        }
    }
    
    private static void ShuffleCells(List<Vector3Int> cells)
    {
        for (int i = 0; i < cells.Count - 1; i++)
        {
            int j = Random.Range(i, cells.Count);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }
    }
    
    private bool CollectAvailableCells(out List<Vector3Int> collectedCells)
    {
        // collect all available cells
        collectedCells = new List<Vector3Int>();
        for (int y = 0; y < LevelSize.y; y++)
        for (int x = 0; x < LevelSize.x; x++) collectedCells.Add(new Vector3Int(x, y, 0));

        if (collectedCells.Count < 2)
        {
            Debug.LogWarning("Not enough cells to spawn items.");
            return false;
        }

        return true;
    }

    #endregion
}