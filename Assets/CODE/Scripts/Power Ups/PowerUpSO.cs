using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

public abstract class PowerUpSO : ScriptableObject
    {
        [LabelWidth(50), OnValueChanged(nameof(RenameAsset))] public string Name;
        public int Cost;
        public Sprite Icon;

        public abstract void UsePowerUp();

        public void RenameAsset()
        {
            #if UNITY_EDITOR
            EditorUtilities.RenameAsset(this, Name);
            #endif
        }
    }