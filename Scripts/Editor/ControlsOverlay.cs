using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView),id: ID_OVERLAY, displayName: "Vr Control Manager")]

public class ControlsOverlay : Overlay
{
    private const string ID_OVERLAY = "vr-overlay";

    private float leftGripValue = 0f;
    private float rightGripValue = 0f;

    private float leftTriggerValue = 0f;
    private float rightTriggerValue = 0f;
    


    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();

        root.style.width = new StyleLength(new Length(300, LengthUnit.Pixel));

        var titleLabel = new Label(text: "Vr ControlManager");
        root.Add(titleLabel);

        GameObject playerBrain = GameObject.Find("PlayerBrain");

        //boolean
        if (playerBrain != null)
        {
            XRControlManager dmanager = playerBrain.GetComponent<XRControlManager>();

            bool doDebugMode = playerBrain.GetComponent<DebugManager>().doDebugMode;
            var debugLabel = new Label(text: "In Debug Mode " + doDebugMode.ToString());

            root.Add(debugLabel);


            //sliders
            #region leftGrip
            var leftGrip = new Slider(label: "Left Grip", start: 0f, end: 1f);
            leftGrip.style.flexGrow = 1f;

            leftGripValue = dmanager.leftHandValues.gripValue;
            leftGrip.value = leftGripValue;

            leftGrip.RegisterValueChangedCallback(ctx =>
            {
                leftGripValue = ctx.newValue;
                //register callback event here
                dmanager.leftHandValues.gripValue = leftGripValue;       
            });

            root.Add(leftGrip);
            #endregion
            #region right grip
            var rightGrip = new Slider(label: "Right Grip", start: 0f, end: 1f);
            rightGrip.style.flexGrow = 1f;

            rightGripValue = dmanager.rightHandValues.gripValue;
            rightGrip.value = rightGripValue;

            rightGrip.RegisterValueChangedCallback(ctx =>
            {
                rightGripValue = ctx.newValue;
                //register callback event here
                dmanager.rightHandValues.gripValue = rightGripValue;
                
            });

            root.Add(rightGrip);
            #endregion

            #region leftTrigger
            var leftTrigger = new Slider(label: "Left Trigger", start: 0f, end: 1f);
            leftTrigger.style.flexGrow = 1f;

            leftTriggerValue = dmanager.leftHandValues.triggerValue;
            leftTrigger.value = leftTriggerValue;

            leftTrigger.RegisterValueChangedCallback(ctx =>
            {
                leftTriggerValue = ctx.newValue;
                //register callback event here
                dmanager.leftHandValues.triggerValue = leftTriggerValue;             
            });

            root.Add(leftTrigger);
            #endregion

            #region rightTrigger
            var rightTrigger = new Slider(label: "Right Trigger", start: 0f, end: 1f);
            rightTrigger.style.flexGrow = 1f;

            rightTriggerValue = dmanager.rightHandValues.triggerValue;
            rightTrigger.value = rightTriggerValue;

            rightTrigger.RegisterValueChangedCallback(ctx =>
            {
                rightTriggerValue = ctx.newValue;
                //register callback event here
                dmanager.rightHandValues.triggerValue = rightTriggerValue;               
            });

            root.Add(rightTrigger);
            #endregion

            var leftHandMainButton = new Button();
            leftHandMainButton.text = "LeftMainButton";
            leftHandMainButton.clicked += OnMainLeftButtonClicked;
            root.Add(leftHandMainButton);

            var rightHandMainButton = new Button();
            rightHandMainButton.text = "RightMainButton";
            rightHandMainButton.clicked += OnMainRightButtonClicked;
            root.Add(rightHandMainButton);

            var leftHandSecondaryButton = new Button();
            leftHandSecondaryButton.text = "LeftSecondaryButton";
            leftHandSecondaryButton.clicked += OnSecondaryLeftButtonClicked;
            root.Add(leftHandSecondaryButton);

            var rightHandSecondaryButton = new Button();
            rightHandSecondaryButton.text = "RightSecondaryButton";
            rightHandSecondaryButton.clicked += OnSecondaryRightButtonClicked;
            root.Add(rightHandSecondaryButton);
        }

        return root;
    }

    void OnMainLeftButtonClicked()
    {    
        GameObject playerBrain = GameObject.Find("PlayerBrain");
        XRControlManager controlManager = playerBrain.GetComponent<XRControlManager>();
        controlManager.OnleftMainButton();
    }

    void OnMainRightButtonClicked()
    {
        GameObject playerBrain = GameObject.Find("PlayerBrain");
        XRControlManager controlManager = playerBrain.GetComponent<XRControlManager>();
        controlManager.OnRightMainButton();
    }

    void OnSecondaryLeftButtonClicked()
    {
        GameObject playerBrain = GameObject.Find("PlayerBrain");
        XRControlManager controlManager = playerBrain.GetComponent<XRControlManager>();
        controlManager.OnLeftSecondaryButton();
    }

    void OnSecondaryRightButtonClicked()
    {
        GameObject playerBrain = GameObject.Find("PlayerBrain");
        XRControlManager controlManager = playerBrain.GetComponent<XRControlManager>();
        controlManager.OnRightSecondaryButton();
    }
}
