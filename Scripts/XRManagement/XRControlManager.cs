using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class XRControlManager : MonoBehaviour
{
    private DebugManager debugManager;
    private XrHandAnimationManager xrAnimator;

    private XRRayInteractionManager xrRayInteractor;

    public enum HandType { right, left}

    [Header("ScriptsReferences")]
    [SerializeField] private XrMovementManager movementManager;
    [SerializeField] private XRGrabManager grabManager;

    [Header("InteractionSettings")]
    [SerializeField] private bool updateHandPositions = true;
    [SerializeField] private bool allowMovement = true;

    [Header("PlayerComponents")]
    [SerializeField] private XRPhysicHand xr_LeftHand;
    [SerializeField] private XRPhysicHand xr_RightHand;
    [Space]
    public Transform leftHandTrackedRaw;
    public Transform rightHandTrackedRaw;

    [Header("Grabbing")]
    public float grabInputThreshold = 0.1f; //this is how far we must press to activiate grab

    [Header("SnapTurnSettings")]
    public Transform snapTarget;
    public float snapTurnThreshhold = 0.1f;
    public float snapTurnAmount = 45f;

    private bool didSnapTurn = false;
    private bool rightTriggerDown = false;
    private bool leftTriggerDown = false;

    //structs
    public VrHandInputValues leftHandValues = new VrHandInputValues();
     public VrHandInputValues rightHandValues = new VrHandInputValues();

    // Start is called before the first frame update
    void Start()
    {
        //intiiatialse the hands
        debugManager = GetComponent<DebugManager>();
        xrAnimator = GetComponent<XrHandAnimationManager>();
        xrRayInteractor = GetComponent<XRRayInteractionManager>();
        InitialiseHands();
    }
    #region update

    // Update is called once per frame
    void Update()
    {
        CheckInputForEvents();
        BaseHandAnimation();
        HandleRayInteraction();

        //this completely overridess all stick movement, to be used with care
        if(allowMovement == true)
        {
            movementManager.UpdateMoveDelta(leftHandValues.stickDelta);
            movementManager.UpdateMovement();
        }

        if (leftHandValues.holdingItem)
        {
            grabManager.UpdateHandMovementLoop(leftHandValues);
        }

        if (rightHandValues.holdingItem)
        {
            grabManager.UpdateHandMovementLoop(rightHandValues);
        }
    }

    void HandleRayInteraction()
    {
        if (!rightHandValues.holdingItem)
        {
            xrRayInteractor.UpdateRayInteraction(rightHandValues);
        }

        if (!leftHandValues.holdingItem)
        {
            xrRayInteractor.UpdateRayInteraction(leftHandValues);
        }
    }

    void BaseHandAnimation()
    {
        xrAnimator.RecieveHandAnimationValues(leftHandValues);
        xrAnimator.RecieveHandAnimationValues(rightHandValues);      
    }

    void CheckInputForEvents()
    {
        //left
        if (leftHandValues.gripValue > (1f - grabInputThreshold))
        {
            //grip pressed
            if (leftHandValues.lastGripped == false)
            {
                leftHandValues.lastGripped = true;
                GripPresssed(leftHandValues);
                Debug.Log("Grip threshold left");
            }
        }
        else
        {
            if (leftHandValues.lastGripped == true && leftHandValues.gripValue < 0.1f)
            {
                ReleasesGrip(leftHandValues);
                leftHandValues.lastGripped = false;
            }
        }

        //right
        if (rightHandValues.gripValue > (1f - grabInputThreshold))
        {
            //grip pressed
            if (rightHandValues.lastGripped == false)
            {
                rightHandValues.lastGripped = true;
                GripPresssed(rightHandValues);
                Debug.Log("Grip threshold right");
            }
        }
        else
        {
            if (rightHandValues.lastGripped == true && rightHandValues.gripValue < 0.1f)
            {

                ReleasesGrip(rightHandValues);

                rightHandValues.lastGripped = false;
            }

        }

        //check for snapturn
        float snapValue = rightHandValues.stickDelta.x;
        if(snapValue > 1f - snapTurnThreshhold || snapValue < -1f - snapTurnThreshhold)
        {
            if (!didSnapTurn)
            {
                didSnapTurn = true;
                SnapTurn(Mathf.Round(snapValue));
            }
        }
        else
        {
            if (didSnapTurn)
            {
                didSnapTurn = false;
            }
        }

        //trigger events
        if(rightHandValues.triggerValue > 0.8f)
        {
            if (!rightTriggerDown)
            {
                TriggerDown(rightHandValues);
            }

            rightTriggerDown = true;
        }
        else
        {
            if (rightTriggerDown)
            {
                TriggerUp(rightHandValues);
            }
            rightTriggerDown = false;
        }

        if (leftHandValues.triggerValue > 0.8f)
        {
            if (!leftTriggerDown)
            {
                TriggerDown(leftHandValues);
            }

            leftTriggerDown = true;
        }
        else
        {
            if (leftTriggerDown)
            {
                TriggerUp(leftHandValues);
            }
            leftTriggerDown = false;
        }
    }
    
    void SnapTurn(float multiplier)
    {
        float amount = snapTurnAmount * multiplier;
        Quaternion targetRot = snapTarget.rotation * Quaternion.AngleAxis(amount, Vector3.up);
        snapTarget.GetComponent<Rigidbody>().MoveRotation(targetRot);
    }

    void GripPresssed(VrHandInputValues targetHand)
    {
        targetHand.lastGripped = true;
        grabManager.AttemptGrab(targetHand);
    }

    void ReleasesGrip(VrHandInputValues targetHand) 
    {
        rightHandValues.lastGripped = false;
        grabManager.ReleaseGrip(targetHand);
    }

    private void FixedUpdate()
    {
        //see if we should update the position of the vr hands
        if (updateHandPositions)
        {
            UpdateHandPositions();


        }
    }


    #endregion

    private Vector3 lastRightHandPosition = Vector3.zero;
    private Vector3 lastLeftHandPosition = Vector3.zero;

    #region managing hands
    void UpdateHandPositions()
    {
        //check which method to use for hand movement
        if (xr_LeftHand.IsGrabbing())
        {
            grabManager.PerformGrabMovement(leftHandValues);
        }
        else
        {
            xr_LeftHand.UpdateHandPosition();
        }

        if (xr_RightHand.IsGrabbing())
        {
            grabManager.PerformGrabMovement(rightHandValues);
        }
        else
        {
            xr_RightHand.UpdateHandPosition();
        }

        //calculate velocity
        rightHandValues.trackedVelocity = (rightHandValues.trackedTransform.position - lastRightHandPosition) * Time.deltaTime;
        lastRightHandPosition = rightHandValues.trackedTransform.position;

        leftHandValues.trackedVelocity = (leftHandValues.trackedTransform.position - lastLeftHandPosition) * Time.deltaTime;
        lastLeftHandPosition = leftHandValues.trackedTransform.position;

    }

    void InitialiseHands()
    {
        leftHandValues.lastGripped = false;
        leftHandValues.holdingItem = false;
        leftHandValues.physicsHand = xr_LeftHand;
        leftHandValues.handType = HandType.left;
        leftHandValues.trackedTransform = leftHandTrackedRaw;

        rightHandValues.lastGripped = false;
        rightHandValues.holdingItem = false;
        rightHandValues.physicsHand = xr_RightHand;
        rightHandValues.handType = HandType.right;
        rightHandValues.trackedTransform = rightHandTrackedRaw;

        lastRightHandPosition = rightHandValues.trackedTransform.position;
        lastLeftHandPosition = leftHandValues.trackedTransform.position;
    }


    #endregion

    #region getting input

    public void GetLeftStickInput(InputAction.CallbackContext context)
    {
        
        Vector2 value = context.ReadValue<Vector2>();
        leftHandValues.stickDelta = value;
       // Debug.Log("Left stick " + value);
    }

    public void GetRightStickInput(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();

            rightHandValues.stickDelta = value;
        
    }
    public void GetLeftGripInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
            leftHandValues.gripValue = value;
       
    }
    public void GetRightGripInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();

            rightHandValues.gripValue = value;
       
    }

    public void GetRightTriggerInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        rightHandValues.triggerValue = value;
    }

    public void GetLeftTriggerInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        leftHandValues.triggerValue = value;      
    }

    private void TriggerDown(VrHandInputValues targetValues)
    {
        if (targetValues.holdingItem)
        {
            grabManager.SendTriggerDown(targetValues);
        }
    }

    private void TriggerUp(VrHandInputValues targetValues)
    {
        if (targetValues.holdingItem)
        {
            grabManager.SendTriggerUp(targetValues);
        }
    }

    public void GetRightMainButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnRightMainButton();
        }
    }

    public void OnRightMainButton()
    {
        Debug.Log("right button clicked");
        grabManager.SendMainButton(rightHandValues);
    }

    public void GetLeftMainButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnleftMainButton();
        }
    }

    public void OnleftMainButton()
    {
        Debug.Log("Leftbutton clicked");
        grabManager.SendMainButton(leftHandValues);
    }
    public void GetRightSecondaryButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnRightSecondaryButton();
        }
    }

    public void OnRightSecondaryButton()
    {
        Debug.Log("rightSecondaryButton clicked");
        grabManager.SendSecondaryButton(rightHandValues);
    }

    public void GetLeftSecondaryButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnLeftSecondaryButton();
        }
    }

    public void OnLeftSecondaryButton()
    {
        Debug.Log("leftSecondaryButton clicked");
        grabManager.SendSecondaryButton(leftHandValues);
    }
    #endregion
}

[System.Serializable]
public class VrHandInputValues
{
    public XRControlManager.HandType handType;
    public XRPhysicHand physicsHand;
    public float gripValue;
    public bool lastGripped;
    public bool holdingItem;
    public float triggerValue;
    public Vector2 stickDelta;

    public Vector3 trackedVelocity;

    public Transform trackedTransform;

    public GrabPoint curGrabPoint;
}
