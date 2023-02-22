using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(GrabbableObject))]
public class GrabbableObjectInspector : Editor
{
    public Object grabPointPrefab;

    private GameObject rootObject;
    private GrabbableObject rootScript;

    public VisualTreeAsset visualTree;

    private VisualElement root;
    void OnEnable()
    {
        rootScript = (GrabbableObject)target;
        rootObject = rootScript.gameObject;
    }

    private void InitializeEditor()
    {
        root.Query<Button>("AddGrabPoints").First().clicked += AddGrabPoint;
    }

    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        root = new VisualElement();

        visualTree.CloneTree(root);      

        InitializeEditor();

        // Return the finished inspector UI
        return root;
    }

    private void AddGrabPoint()
    {

        if(grabPointPrefab != null && rootObject != null)
        {
            Object grabInstance = PrefabUtility.InstantiatePrefab(grabPointPrefab);
            GameObject grabObjInstance = (GameObject)grabInstance;
            grabObjInstance.transform.position = rootObject.transform.position;
            grabObjInstance.transform.SetParent(rootObject.transform, true);
            rootScript.AddToGrabPointList(grabObjInstance);
        }
    }
}
