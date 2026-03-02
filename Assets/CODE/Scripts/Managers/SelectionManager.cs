using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using Utilities.Extensions;

public class SelectionManager : Singleton<SelectionManager>
{
    [HideLabel] public PathDrawer PathDrawer = new PathDrawer();
    [ReadOnly] public Animal SelectedAnimal;

    private void Awake()
    {
        PathDrawer.OnPathComplete.AddListener(OnPathDrawn);
    }

    private void Update()
    {
        if (LevelGenerator.Instance.IsBusy) return;
        
        if (PathDrawer.IsDrawing) // While drawing continue while pressed, otherwise try place & finish
        {
            if (Pointer.current.press.isPressed)
            {
                PathDrawer.OnContinueDraw();
            }
            else if (Pointer.current.press.wasReleasedThisFrame)
            {
                PathDrawer.OnEndDraw();
                SelectedAnimal = null;
            }
           
            return;
        }

        // When not drawing, start draw when pressing on an idle animal.
        if (Pointer.current.press.wasPressedThisFrame && Physics.Raycast(Camera.main!.ScreenPointToRay(Pointer.current.position.ReadValue()), out var hit2, 100f) && hit2.collider.TryGetComponent<Animal>(out var animal) && !animal.IsMoving)
        {
            SelectedAnimal = animal;
            PathDrawer.OnStartDraw();
        }
    }

    private void OnPathDrawn(List<Vector3> path)
    {
        if (Physics.Raycast(Camera.main!.ScreenPointToRay(Pointer.current.position.ReadValue()), out var hit, 100f) && hit.collider.TryGetComponent<Box>(out var box) && SelectedAnimal)
        {
            bool isPathCompleting = PathDrawer.LastCell == LevelGenerator.Instance.Grid.WorldToCell(box.transform.position.WithY(0));
            if (box.ID == SelectedAnimal.ID && isPathCompleting)
            {
                var animalRef = SelectedAnimal; // capture it or it will be null on complete.
                SelectedAnimal.FollowPath(path, () => animalRef.JumpIntoBox(box));
            }
            else
            {
                SelectedAnimal.transform.DOShakePosition(0.5f, new Vector3(0.1f, 0f, 0.1f));
                SelectedAnimal.transform.DOShakeScale(0.1f, 1.1f);
                box.transform.DOShakePosition(0.5f, new Vector3(0.1f, 0f, 0.1f));
                box.transform.DOShakeScale(0.1f, 1.1f);
                SoundManager.Instance.PlaySound(SoundManager.Instance.InCorrectSound);
            }
        }
    }
}