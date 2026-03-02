using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public abstract class InspectorComponentHeaderItem
{
    private static readonly Dictionary<Type, MethodInfo> methodDict = new();

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.update += InitHeader;
    }

    private static void InitHeader()
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

        FieldInfo fieldInfo = typeof(EditorGUIUtility).GetField("s_EditorHeaderItemsMethods", flags);
        IList value = (IList)fieldInfo.GetValue(null);
        if (value == null) return;
        Type delegateType = value.GetType().GetGenericArguments()[0];

        Func<Rect, UnityEngine.Object[], bool> func = DrawHeaderItem;
        TypeCache.MethodCollection methods = TypeCache.GetMethodsWithAttribute<ComponentHeaderItemAttribute>();
        foreach (MethodInfo method in methods)
        {
            if (!method.IsStatic) return;
            if (!methodDict.TryAdd(method.ReflectedType, method)) return;
            value.Add(Delegate.CreateDelegate(delegateType, func.Method));
        }

        EditorApplication.update -= InitHeader;
    }

    private static bool DrawHeaderItem(Rect rect, UnityEngine.Object[] targets)
    {
        UnityEngine.Object target = targets[0];

        Type targetType = target.GetType();

        if (methodDict.ContainsKey(targetType))
        {
            //object instance = Activator.CreateInstance(target.GetType());
            methodDict.TryGetValue(targetType, out MethodInfo method);
            var parameters = method.GetParameters();
            var parametersType = parameters.Select(parameter => parameter.ParameterType).ToList();

            Type rectType = rect.GetType();

            if (parametersType.Count == 1 && parametersType.Contains(rectType))
            {
                method.Invoke(null, new object[] { rect });
            }
            else
            {
                GUIStyle errorStyle = new GUIStyle
                {
                    normal =
                    {
                        textColor = Color.red
                    },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 12
                };
                rect.width = 78;
                rect.x -= 63;

                const string errorToolTip = "Method Parameter Error: Please make sure the parameter is correct";
                GUI.Label(rect, new GUIContent(" Item Error!", EditorGUIUtility.IconContent("CollabError").image, errorToolTip), errorStyle);
            }
        }

        return false;
    }
}

#endif

/// Add an item to the component in the inspector window
/// Methods that use this Attribute must have [Rect] parameter
[AttributeUsage(AttributeTargets.Method)]
public class ComponentHeaderItemAttribute : Attribute
{
    public ComponentHeaderItemAttribute() { }
}