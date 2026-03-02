using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

public class Animal : MonoBehaviour
{
    public AnimalType ID;
    [SerializeField] private float tileMoveDuration = 1f;
    [SerializeField] private float turnDuration = 0.5f;

    [ReadOnly] public bool IsMoving;
    
    /// <summary>
    /// Follow a world-space path (list of positions). Calls onComplete when finished.
    /// No diagonal/straight-line constraints required here path is a sequence of world points.
    /// </summary>
    public void FollowPath(IList<Vector3> path, System.Action onComplete = null)
    {
        if (path.IsNullOrEmpty())
        {
            onComplete?.Invoke();
            return;
        }
        
        Sequence seq = DOTween.Sequence().OnComplete(() =>
        {
            IsMoving = false;
            onComplete?.Invoke();
        });
        
        for (int i = 1; i < path.Count -1; i++)
        {
            var point = path[i];
            // Look only if there’s a valid direction
            if ((point - transform.position).sqrMagnitude > 0.01f) seq.Append(transform.DOLookAt(point, turnDuration).SetEase(Ease.InOutSine));
            seq.Append(transform.DOMove(point, tileMoveDuration).SetEase(Ease.Linear));
        }
        
        if ((path[^1] - transform.position).sqrMagnitude > 0.01f) seq.Append(transform.DOLookAt(path[^1], turnDuration).SetEase(Ease.InOutSine));
        
        IsMoving = true;
        
        transform.DOKill();
        seq.Play();
    }

    public void JumpIntoBox(Box box)
    {
        transform.DOJump(box.GetPackPosition(), 1f, 1, 1f).OnComplete(() => box.Pack(this));
    }
}

public enum AnimalType
{
    Bear,
    Buffalo,
    Chick,
    Chiken,
    Cow,
    Crocodile,
    Dog,
    Duck,
    Elephant,
    Frog,
    Giaffe,
    Goat,
    Gorilla,
    Hippo,
    Horse,
    Monkey,
    Moose,
    Narwhal,
    Owl,
    Panda,
    Parrot,
    Penguin,
    Pig,
    Rabbit,
    Rhino,
    Sloth,
    Snake,
    Walrus,
    Whale,
    Zebra
}