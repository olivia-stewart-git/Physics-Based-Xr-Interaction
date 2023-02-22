using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(GrabPoint))]
public class GrabPointInspector : Editor
{
    private GameObject rootObject;
    private GrabPoint rootScript;

    public VisualTreeAsset visualTree;

    private VisualElement root;

    private VisualElement standardTypeHolder;
    private VisualElement lineTypeHolder;
    private VisualElement radialTypeHolder;

    void OnEnable()
    {
        rootScript = (GrabPoint)target;
        rootObject = rootScript.gameObject;

        //ini
    }
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        root = new VisualElement();

        visualTree.CloneTree(root);

        standardTypeHolder = root.Query<VisualElement>("StandardType");
        lineTypeHolder = root.Query<VisualElement>("LineType");
        radialTypeHolder = root.Query<VisualElement>("RadialType");

        RefreshContent(root);

        UpdateValues();

        // Return the finished inspector UI
        root.schedule.Execute(_ => RefreshContent(root)).Every(100);

        return root;
    }

    void RefreshContent(VisualElement root)
    {
        switch (rootScript.pointType)
        {
            case GrabPoint.GrabPointTransformType.standard:
                standardTypeHolder.style.display = DisplayStyle.Flex;
                lineTypeHolder.style.display = DisplayStyle.None;
                radialTypeHolder.style.display = DisplayStyle.None;
                break;
            case GrabPoint.GrabPointTransformType.line:
                standardTypeHolder.style.display = DisplayStyle.None;
                lineTypeHolder.style.display = DisplayStyle.Flex;
                radialTypeHolder.style.display = DisplayStyle.None;
                break;
            case GrabPoint.GrabPointTransformType.radial:
                standardTypeHolder.style.display = DisplayStyle.None;
                lineTypeHolder.style.display = DisplayStyle.None;
                radialTypeHolder.style.display = DisplayStyle.Flex;
                break;
        }
        UpdateValues();
    }

    
    void UpdateGrabPreviewAnimation()
    {

    }

    void UpdateValues()
    {
        rootScript.SetValues();
    }
}
