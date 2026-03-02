using System.Linq;
using UnityEngine;
using Utilities.Extensions;

[CreateAssetMenu(fileName = "New Draw A Path")]
public class DrawAPath : PowerUpSO
{
    public float Duration = 10f;
    
    public override void UsePowerUp()
    {
        if (!GameManager.Instance.RemoveCoins(Cost)) return;
        
        var pair = GetRandomPair();
        var path = LevelGenerator.Instance.FindPath(pair.Item2.transform.position.WithY(0f), pair.Item1.transform.position.WithY(0f));
        var line = SelectionManager.Instance.PathDrawer.HintPathRenderer;

        if (line)
        {
            line.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++) line.SetPosition(i, path[i].WithY(2f));
            
            SelectionManager.Instance.DelayedExecution(Duration, () => line.positionCount = 0);
        }
    }

    public static (Box, Animal) GetRandomPair()
    {
        var boxes = FindObjectsByType<Box>(FindObjectsSortMode.None).Where(b => !b.IsTarget);
        var animals = FindObjectsByType<Animal>(FindObjectsSortMode.None).Where(a => !a.IsMoving);

        var boxGroups = boxes.GroupBy(b => b.ID).ToDictionary(g => g.Key, g => g.ToList());
        var animalGroups = animals.GroupBy(a => a.ID).ToDictionary(g => g.Key, g => g.ToList());

        var colors = boxGroups.Keys.Where(animalGroups.ContainsKey).ToList();
        if (colors.Count == 0) return (null, null);

        var c = colors[Random.Range(0, colors.Count)];

        if (boxGroups[c].Count == 0 || animalGroups[c].Count == 0) return (null, null);

        return (boxGroups[c][Random.Range(0, boxGroups[c].Count)], animalGroups[c][Random.Range(0, animalGroups[c].Count)]);
    }
}