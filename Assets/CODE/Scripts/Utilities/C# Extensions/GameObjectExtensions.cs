using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class GameObjectExtensions
    {
        public static void SetLayerRecursively(this GameObject gameObject, string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in gameObject.transform) SetLayerRecursively(child.gameObject, layerName);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var attachedComponent)) attachedComponent = gameObject.AddComponent<T>();

            return attachedComponent;
        }

        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out _);
        }

        public static GameObject ToggleActive(this GameObject gameObject)
        {
            gameObject.SetActive(!gameObject.activeSelf);
            return gameObject;
        }

        public static GameObject DestroyAllChildren(this GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (Application.isPlaying) Object.Destroy(child.gameObject);
                else Object.DestroyImmediate(child.gameObject);
            }
            
            return gameObject;
        }

        public static GameObject AddComponentIfMissing<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.GetComponent<T>()) gameObject.AddComponent<T>();
            return gameObject;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInChildren<T>(includeInactive);
            return component;
        }

        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInParent<T>(includeInactive);
            return component;
        }

        #region Gizmos Icon

        public static void SetIcon(this GameObject gameObject, LabelIcon labelIcon)
        {
            SetIcon(gameObject, $"sv_label_{(int) labelIcon}");
        }
        
        public static void SetIcon(this GameObject gameObject, ShapeIcon shapeIcon)
        {
            SetIcon(gameObject, $"sv_icon_dot{(int) shapeIcon}_pix16_gizmo");
        }
        
        public static void RemoveIcon(this GameObject gameObject)
        {
            #if UNITY_EDITOR
            EditorGUIUtility.SetIconForObject(gameObject, null);
            #endif
        }

        private static void SetIcon(GameObject gameObject, string contentName)
        {
            #if UNITY_EDITOR
            GUIContent iconContent = EditorGUIUtility.IconContent(contentName);
            EditorGUIUtility.SetIconForObject(gameObject, (Texture2D)iconContent.image);
            #endif
        }
        
        public enum LabelIcon
        {
            Gray,
            Blue,
            Teal,
            Green,
            Yellow,
            Orange,
            Red,
            Purple
        }
        
        public enum ShapeIcon
        {
            CircleGray,
            CircleBlue,
            CircleTeal,
            CircleGreen,
            CircleYellow,
            CircleOrange,
            CircleRed,
            CirclePurple,
            DiamondGray,
            DiamondBlue,
            DiamondTeal,
            DiamondGreen,
            DiamondYellow,
            DiamondOrange,
            DiamondRed,
            DiamondPurple
        }

        #endregion
    }
}