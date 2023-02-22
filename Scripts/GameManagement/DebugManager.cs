using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour
{


    public XRControlManager xr_Controller;

    public bool doDebugMode = false;

    public ActionBasedController leftController;
    public ActionBasedController rightController;

    public TrackedPoseDriver headTracker;
    // Start is called before the first frame update
    void Start()
    {
        if (doDebugMode)
        {
           
            StartDebugMode();
        }
    }

    private void Update()
    {
        if (doDebugMode)
        {
           // SetInputValues();
            PerformCameraLook();
        }
    }

    void StartDebugMode()
    {
        rightController.enableInputTracking = false;
        leftController.enableInputTracking = false;

        headTracker.enabled = false;
    }

    void PerformCameraLook()
    {

    }
}
