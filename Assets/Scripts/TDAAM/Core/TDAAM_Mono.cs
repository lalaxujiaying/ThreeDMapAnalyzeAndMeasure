using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDAAM_Mono<T> : MonoBehaviour where T : TDAAM_Mono<T>
{
    private static Transform root;
    private static Transform GetTDAAMRoot()
    {
        if (root != null) return root;

        root = new GameObject("[TDAAMRoot]").transform;
        DontDestroyOnLoad(root);
        return root;
    }
    private static Transform GetToolRoot(string toolTypeName)
    {
        Transform toolRoot = GetTDAAMRoot().Find(toolTypeName);
        if (toolRoot == null)
        {
            toolRoot = new GameObject(toolTypeName).transform;
            toolRoot.parent = GetTDAAMRoot();
        }
        return toolRoot;
    }
    public static T Create(string toolTypeName, string toolName, out GameObject go)
    {
        go = new GameObject(toolName);
        go.transform.parent = GetToolRoot(toolTypeName);
        return go.AddComponent<T>();
    }
    public static T Create(Transform toolTransfrom, string toolName, out GameObject go)
    {
        go = new GameObject(toolName);
        go.transform.parent = toolTransfrom;
        return go.AddComponent<T>();
    }
}
