using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class GrabPoint : MonoBehaviour
{
    
    public enum GrabPointTransformType {standard, line, radial}

    public GrabPointTransformType pointType;

    public GrabbableObject grabParent;
    public Vector3 grabPointArea = new Vector3(0.05f, 0.05f, 0.05f);

    public string animationPoseKey = "Grip";

    public bool allowLeftHand = true;
    public bool allowRightHand = true;

    public Transform orientationReference;
    public Transform mainOrientator;
    public Transform secondaryOrientator;

    public GameObject leftHandRepresentor;
    public GameObject rightHandRepresentor;

    //offsest settingss
    public bool useRightHand = true;
    public bool useLeftHand = true;
    public bool offsetPoint;

    private XROffsetObject offsetObject;

    public XROffsetObject OffsetObject() { return offsetObject; }

    //standard grab settings

    //radial settings
    public float grabRadius;

    //line settings

    //offset setting
    public bool isOffsetGrab = false;
    public bool onOffsetChangeTranform = false; //if true we do dual handed movement best for shotugn pump etc
    private float curOffsetValue = 0.5f; //must be between 0, 1

    //trigger events

    public UnityEvent OnTriggerDown = new UnityEvent();
    public UnityEvent OnTriggerup = new UnityEvent();
    public UnityEvent OnMainButton = new UnityEvent();

    public bool beingHeld = false;

    //for extra settings

    public bool requirePrior;
    public bool priorGrabIsSuperficial = true;
    public GrabPoint priorRequirement;

    public List<AdditionalOrientator> additionalOrientators = new List<AdditionalOrientator>();

    private void Start()
    {
        DisableRepresentors();
        usePoseKey = animationPoseKey;
        if (isOffsetGrab)
        {
            offsetObject = GetComponent<XROffsetObject>();
        }
    }


    void DisableRepresentors()
    {
        leftHandRepresentor.SetActive(false);
        rightHandRepresentor.SetActive(false);

        if(additionalOrientators.Count > 0) //disables additional orientators
        {
            foreach (AdditionalOrientator orient in additionalOrientators)
            {
                orient.orientationInstance.SetActive(false);
            }
        }
    }
    private bool blockingGrab = false;
    public void SetGrabBlock(bool value)
    {
        blockingGrab = value;
    }
    public bool AllowGrab()
    {
        if (blockingGrab)
        {
            return false;
        }

        if (beingHeld && !CanSecondaryGrab())
        {
            return false;
        }

        return true;
    }
    public void SetValues()
    {
     //   GetComponent<BoxCollider>().size = grabPointArea;
        //sets layer to interactable object layer
        if (!Application.isPlaying)
        {
            leftHandRepresentor.SetActive(useLeftHand);
            rightHandRepresentor.SetActive(useRightHand);
        }
    }


    private Vector3 startedGrabOffset = Vector3.zero;

    public void StartedGrab(Transform input, XRControlManager.HandType handType)
    {
        startedGrabOffset = transform.InverseTransformPoint(input.position); //takess the offset so we can save the value for complex grab behaviors

        CalculateTargetRepresentor(input.position, input.forward, handType);
    }

    Transform CalculateTargetRepresentor(Vector3 position, Vector3 direction, XRControlManager.HandType handType)
    {
        switch (handType)
        {
            case XRControlManager.HandType.right:
                useRepresentor = rightHandRepresentor.transform;
                break;
            case XRControlManager.HandType.left:
                useRepresentor = leftHandRepresentor.transform;
                break;
        }

        Transform baseCheck = useRepresentor;
        float dot = Vector3.Dot(baseCheck.forward, direction);
        float distance = Vector3.Distance(baseCheck.position, position);

        float targetCheck = (1f / distance) * dot;

        if(additionalOrientators.Count > 0) //compare angles products to find closest
        {
            for (int i = 0; i < additionalOrientators.Count; i++)
            {
                if (additionalOrientators[i].gripType == handType && additionalOrientators[i].isSecondaryGrab == false)
                {
                    float newDot = Vector3.Dot(additionalOrientators[i].orientationInstance.transform.forward, direction);
                    float newDis = Vector3.Distance(additionalOrientators[i].orientationInstance.transform.position, position);
                    float newCheck = (1f / newDis) * newDot;
                    if (targetCheck < newCheck)
                    {
                        targetCheck = newCheck;
                        useRepresentor = additionalOrientators[i].orientationInstance.transform;
                        usePoseKey = additionalOrientators[i].animationPose;
                    }
                }
            }
        }
        else
        {
            usePoseKey = animationPoseKey;
        }

        return useRepresentor;
    }

    public Vector3 GetGrabPosition(Transform input, XRControlManager.HandType handType)
    {
        Vector3 targetPosition = Vector3.zero;
        if (useRepresentor == null)
        {
            useRepresentor = CalculateTargetRepresentor(input.position, input.forward, handType);
        }
        switch (pointType)
        {
            case GrabPointTransformType.standard:
                targetPosition = useRepresentor.position;
                break;
            case GrabPointTransformType.line:
                Vector3 offset = orientationReference.position - secondaryOrientator.position;


                Vector3 between = secondaryOrientator.position - mainOrientator.position;
                Vector3 betweenHand = input.position - mainOrientator.position;
                float dot = Vector3.Dot(between, betweenHand);
                if(dot <= 0f)
                {
                    targetPosition = mainOrientator.position;
                    break;
                }

                //project the vector upon
                float squareMagnitude = Vector3.SqrMagnitude(between);
                Vector3 projection = (dot / squareMagnitude) * between;
                targetPosition = mainOrientator.position + projection;

                break;
            case GrabPointTransformType.radial:
                Vector3 normalized = (input.position - mainOrientator.position).normalized;
                targetPosition = mainOrientator.position + (normalized * grabRadius);
                break;
        }
        return targetPosition;
    }

    private Transform useRepresentor;
    private string usePoseKey;

    public Transform GetCurrentOrientationTransform()
    {
        return useRepresentor;
    }

    public Quaternion GetGrabRotation(Transform input, XRControlManager.HandType handType)
    {
        Quaternion targetRotation = Quaternion.identity;

        if(useRepresentor == null)
        {
            useRepresentor = CalculateTargetRepresentor(input.position, input.forward, handType);
        }
        switch (pointType)
        {
            case GrabPointTransformType.standard:

                    targetRotation = useRepresentor.transform.rotation;
                break;
            case GrabPointTransformType.line:
                Quaternion baseRotation = mainOrientator.rotation;

                //we determine how much there has been a rotation
                Vector3 baseDirection = orientationReference.position - secondaryOrientator.position;
                Vector3 lineDirection = secondaryOrientator.position - mainOrientator.position;

                break;
            case GrabPointTransformType.radial:
                break;
        }
        return targetRotation;
    }

    private void OnDrawGizmosSelected()
    {
        switch (pointType)
        {
            case GrabPointTransformType.standard:
                break;
            case GrabPointTransformType.line:
                Gizmos.DrawLine(mainOrientator.position, secondaryOrientator.position);
                break;
            case GrabPointTransformType.radial:
                Gizmos.DrawWireSphere(mainOrientator.position, grabRadius);
                break;
        }

    }
    #region secondaryGrab
    private bool isDoingSecondaryGrab;
    public bool DoingSecondaryGrab()
    {
        return isDoingSecondaryGrab;
    }

    public bool CanSecondaryGrab()
    {
        if (!grabParent.isBeingHeld) return false;

        if(grabParent.SecondaryHeld() == null)
        {
            foreach (AdditionalOrientator orient in additionalOrientators)
            {
                if (orient.isSecondaryGrab)
                {
                    return true;
                }
            }
        }


        return false;
    }

    private AdditionalOrientator targetSecondaryOrientator;
    private Transform targetSecondaryGrab;
    private string useSecondaryPoseKey;
    public string SecondaryGrabPoseKey()
    {
        return useSecondaryPoseKey;
    }

    public void CalculateSecondaryGrabTransform(Vector3 position, Vector3 direction, XRControlManager.HandType handType)
    {
        Transform baseCheck = additionalOrientators[0].orientationInstance.transform;
        int orientatorId = 0;
        float dot = Vector3.Dot(baseCheck.forward, direction);
        float distance = Vector3.Distance(baseCheck.position, position);

        float targetCheck = (1f / distance) * dot;

        if (additionalOrientators.Count > 0) //compare angles products to find closest
        {
            for (int i = 0; i < additionalOrientators.Count; i++)
            {
                if (additionalOrientators[i].gripType == handType && additionalOrientators[i].isSecondaryGrab == true)
                {
                    float newDot = Vector3.Dot(additionalOrientators[i].orientationInstance.transform.forward, direction);
                    float newDis = Vector3.Distance(additionalOrientators[i].orientationInstance.transform.position, position);
                    float newCheck = (1f / newDis) * newDot;
                    if (targetCheck < newCheck)
                    {
                        targetCheck = newCheck;
                        baseCheck = additionalOrientators[i].orientationInstance.transform;
                        usePoseKey = additionalOrientators[i].animationPose;
                        orientatorId = i;
                    }
                }
            }
        }

        targetSecondaryOrientator = additionalOrientators[orientatorId];
        targetSecondaryGrab = baseCheck;
    }

    public void StartSecondaryGrab(Vector3 position, Vector3 direction, XRControlManager.HandType handType)
    {
        secondaryGrabHeldBy = handType;
        isDoingSecondaryGrab = true;
        CalculateSecondaryGrabTransform(position, direction, handType);
    }

    public void EndSecondaryGrab()
    {
        isDoingSecondaryGrab = false;
        Debug.Log("ended secondary Grab");
    }

    private XRControlManager.HandType secondaryGrabHeldBy;
    void TransferSecondaryGrab()
    {
        if(secondaryGrabHeldBy == grabParent.PrimaryHeld().handType) //this assumes that we have switched the primary hand to the secondary hand
        {
            usePoseKey = targetSecondaryOrientator.transferPoseKey;
            useRepresentor = targetSecondaryOrientator.transferToTransform; //sets the primary positon and poses to the secondary
        }

        EndSecondaryGrab();
    }

    public Transform GetSecondaryGrabTransform()
    {
        return targetSecondaryGrab;
    }



    #endregion
    public void ApplyHandPose()
    {
        //we do pose of
    }

    public string GetAnimationPose()
    {
        return usePoseKey;
    }

    public void TriggerInputDown()
    {
        OnTriggerDown.Invoke();
    }

    public void TriggerInputUp()
    {
        OnTriggerup.Invoke();
    }

    public void MainButtonInput()
    {
        OnMainButton.Invoke();
    }

    public void OnGripReleased(XRControlManager.HandType type)
    {
        if (isDoingSecondaryGrab)
        {
            TransferSecondaryGrab();
        }
    }
}

[System.Serializable]
public class AdditionalOrientator
{
    public string animationPose = "Grip";
    public XRControlManager.HandType gripType;
    public GameObject orientationInstance;

    public bool isSecondaryGrab = false;
    public Transform transferToTransform;
    public string transferPoseKey = "Grip"; //these are for when we release the grip and transfer where the gun is held

}