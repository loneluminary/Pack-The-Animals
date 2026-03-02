using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

public class Box : MonoBehaviour
{
    public AnimalType ID;
    [ReadOnly] public bool IsTarget;
    [SerializeField, AssetsOnly] GameObject packEffect;

    public List<Transform> AvailableSlots;
    [ReadOnly] public List<Animal> PackedAnimals;

    public void Pack(Animal animal)
    {
        animal.gameObject.SetActive(false);
        PackedAnimals.Add(animal);
        AvailableSlots.RemoveAt(0);
        
        if(AvailableSlots.Count <= 0) PackUpBox();
    }

    public void PackUpBox()
    {
        var seq = DOTween.Sequence().SetDelay(0.5f).OnComplete(() =>
        {
            GameManager.Instance.AddCoins(50);
            UIManager.Instance.CoinsAddingAnimation(transform.position + Vector3.up * 1f);
            SoundManager.Instance.PlaySound(SoundManager.Instance.CorrectSound);
            Instantiate(packEffect, transform.position, Quaternion.identity);
            GameManager.Instance.DelayedExecution(1f, () => { if (GameManager.Instance.IsCompleted()) UIManager.Instance.WinScreen(); });
            
            Destroy(gameObject);
        });
        
        seq.Join(transform.DOJump(transform.position, 1f, 1, 1f));
        seq.Join(transform.DOShakeScale(0.5f, 1.1f));

        seq.Play();
    }
    
    public Vector3 GetPackPosition() => AvailableSlots[0].position;
}