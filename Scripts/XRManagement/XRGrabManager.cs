using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class XRGrabManager : MonoBehaviour
{
    private GameLogger g_Logger;
    private AudioManager a_Manager;

    private XrHandAnimationManager animationManager;

    [Header("Grab settings")]
    [SerializeField] private Vector3 grabCheckSize = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private LayerMask grabMask;
    [Space]
   

    [Header("StandardMove settings")]
    [Range(0, 1)] public float slowDownVelocity = 0.75f;
    [Range(0, 1)] public float slowDownAngularVelocity = 0.75f;

    [Range(0, 100)] public float maxPositionChange = 75.0f;
    [Range(0, 100)] public float maxRotationChange = 75.0f;

    public float anglularCuttoff = 5f;

    public float handMoveMultiplier = 2f;
    public float handRotateMultiplier = 3f;

    private Quaternion targetRotation = Quaternion.identity;
    private Vector3 targetPosition = Vector3.zero;

    public float torqueCompensationMultiplier = 1f;

    [Header("Double Handed Move settings")]
    [Range(0f, 1f)] public float secondaryHandTiltStrength = 0.3f;
    [Range(0, 1)] public float slowDownVelocityDual = 0.75f;
    [Range(0, 1)] public float slowDownAngularVelocityDual = 0.75f;

    [Header("Weight settings")]
    public float gravityStrength = 9.8f;
    [Tooltip("The weight where the object will move 1 to one")] public float miniumMoveWeight = 1f;
    [Tooltip("The weight where all highter weights will be normalized to")]public float maximumMoveWeight = 20f;
    public float moveWeightInfluenceMultiplier = 1f;
    [Space]
    public float rotationWeightBouncinessMultiplier = 1f;

    private void Start()
    {
        a_Manager = AudioManager.Instance;
        g_Logger = GetComponent<GameLogger>();
        animationManager = GetComponent<XrHandAnimationManager>();
    }

    

    public void AttemptGrab(VrHandInputValues targetHand)
    {
        g_Logger.LogNotice("attempted grab");
        //we check to see if there is an applicable grab point
        if(targetHand.holdingItem == false)
        {
            //we check to find something to grab
            Collider[] grabOverlaps = Physics.OverlapBox(targetHand.physicsHand.grabCentre.position, grabCheckSize, Quaternion.identity, grabMask);

            if(grabOverlaps == null || grabOverlaps.Length == 0)
            {
                return;
            }
            Debug.Log(grabOverlaps);
            //create a list of possible points
            List<GrabPoint> possibleGrabs = new List<GrabPoint>();
            foreach (var item in grabOverlaps)
            {
                GrabPoint test = item.gameObject.GetComponent<GrabPoint>();
                if (test.AllowGrab())
                {
                    possibleGrabs.Add(test);
                    
                }
            }
            Debug.Log(possibleGrabs);

            if (possibleGrabs != null && possibleGrabs.Count > 0)
            {
                if(possibleGrabs.Count > 1) //we sort by closest if list is larger and dot
                {
                    possibleGrabs = possibleGrabs.OrderBy((d) => (1f / ((d.transform.position - targetHand.physicsHand.grabCentre.position).magnitude) * Vector3.Dot(d.GetComponent<GrabPoint>().GetGrabRotation(targetHand.physicsHand.transform, targetHand.handType) * Vector3.one, targetHand.trackedTransform.forward))).ToList<GrabPoint>();
                }

                //set the use target

                GrabPoint gPoint = possibleGrabs[0];

                if(gPoint.grabParent.isBeingHeld == false)
                {
                    if ((targetHand.handType == XRControlManager.HandType.right && gPoint.useRightHand == true) ||(targetHand.handType == XRControlManager.HandType.left && gPoint.useLeftHand == true)) 
                    {
                        if (gPoint.isOffsetGrab)
                        {
                            StartOffsetGrab(targetHand, gPoint);
                        }
                        else
                        {
                            PerformGrab(targetHand, gPoint);
                        }
                    }
                }
                else
                {
                    if (gPoint.beingHeld == true && gPoint.CanSecondaryGrab())
                    {
                        PerformGrabToSecondaryGrab(targetHand, gPoint);
                    }
                    else
                    {

                        if ((targetHand.handType == XRControlManager.HandType.right && gPoint.useRightHand == true) || (targetHand.handType == XRControlManager.HandType.left && gPoint.useLeftHand == true))
                        {
                            if (gPoint.isOffsetGrab)
                            {
                                StartOffsetGrab(targetHand, gPoint);
                            }
                            else
                            {
                                //we see for doing multi hold
                                PerformGrabToPriorHeld(targetHand, gPoint);
                            }
                        }
                    }
                }
            }
        }
    }

    void PerformGrabToPriorHeld(VrHandInputValues targetHand, GrabPoint targetGrab)
    {
        if (resetHandCoroutine != null)
        {
            StopCoroutine(resetHandCoroutine);
        }

        targetGrab.StartedGrab(targetHand.trackedTransform, targetHand.handType);

        g_Logger.LogNotice("secondary hold " + targetGrab.grabParent.name);
        Debug.Log("secondary hold " + targetGrab.grabParent.name);

        targetHand.physicsHand.GetComponent<Rigidbody>().isKinematic = true;
        targetHand.physicsHand.gameObject.layer = 8;
        targetHand.curGrabPoint = targetGrab;
        targetGrab.beingHeld = true;
        Debug.Log("curgrab point " + targetHand.curGrabPoint.transform.parent.name);

        GrabbableObject targetObject = targetGrab.grabParent;
        targetHand.physicsHand.SetGrabbedObject(targetObject);

        //we activate secondary movement
        if (targetObject.PrimaryHeld().curGrabPoint.isOffsetGrab == true)
        {
            if (targetObject.PrimaryHeld().curGrabPoint.OffsetObject().dePrioritiseGrab)
            {
                targetObject.SetHeldBy(targetHand, targetObject.PrimaryHeld(), this); //switches so the primary hand is not the offset object
            }
            else
            {
                targetObject.SetHeldBy(targetObject.PrimaryHeld(), targetHand, this); //justs adjust primary hold
            }
        }
        else
        {
            targetObject.SetHeldBy(targetObject.PrimaryHeld(), targetHand, this); //justs adjust primary hold
        }

        targetHand.holdingItem = true;

        animationManager.SwitchHandPose(targetGrab.GetAnimationPose(), targetHand.handType);

        a_Manager.PlaySound("VrGrab", 1f, 1f, 0.1f, targetGrab.GetGrabPosition(targetHand.physicsHand.transform, targetHand.handType), 0f);
    }

    public void PerformGrab(VrHandInputValues targetHand, GrabPoint targetGrab)
    {
        if (resetHandCoroutine != null)
        {
            StopCoroutine(resetHandCoroutine);
        }


        g_Logger.LogNotice("grabbed object " + targetGrab.grabParent.name);
        Debug.Log("grabbed object " + targetGrab.grabParent.name);

        targetGrab.StartedGrab(targetHand.trackedTransform, targetHand.handType);
        
        targetHand.physicsHand.GetComponent<Rigidbody>().isKinematic = true;

        targetHand.curGrabPoint = targetGrab;
        Debug.Log("curgrab point " + targetHand.curGrabPoint.transform.parent.name);

        GrabbableObject targetObject = targetGrab.grabParent;
        targetObject.rb.useGravity = false;

        targetObject.isBeingHeld = true;
        targetObject.SetHeldBy(targetHand, null, this);


        if(targetObject.GetComponent<HeldObjectInputMale>() != null)
        {
            HeldObjectInputMale mInput = targetObject.GetComponent<HeldObjectInputMale>();
            if (mInput.IsSocketed())
            {
                StartModifiedOffset(mInput.FemaleInput(), mInput, targetHand);
            }
        }
        targetGrab.beingHeld = true;

        targetHand.physicsHand.SetGrabbedObject(targetObject);
        targetObject.ObjectGrabbed();

        //snap to the position

        //we set the layer of the target object to 8 : held item
       // targetObject.SetCollisionObjectLayers(8);
        targetHand.physicsHand.gameObject.layer = 8;
        SnapObjectToHand(targetHand, targetGrab);
        
        //to connect the rigidbody to the hands
      //  targetHand.physicsHand.BindObjectToRigidBody(targetObject.gameObject);
        targetHand.holdingItem = true;

        animationManager.SwitchHandPose(targetGrab.GetAnimationPose(), targetHand.handType);

        a_Manager.PlaySound("VrGrab", 1f, 1f, 0.1f, targetGrab.GetGrabPosition(targetHand.physicsHand.transform, targetHand.handType), 0f);

    }

    void PerformGrabToSecondaryGrab(VrHandInputValues targetHand, GrabPoint targetGrab) //ie pistol grip
    {
        if (resetHandCoroutine != null)
        {
            StopCoroutine(resetHandCoroutine);
        }
        Debug.Log("Performing secondary grab");

        targetHand.physicsHand.GetComponent<Rigidbody>().isKinematic = true;
        targetHand.curGrabPoint = targetGrab;

        GrabbableObject targetObject = targetGrab.grabParent;
        targetObject.rb.useGravity = false;

        targetObject.isBeingHeld = true;
        targetObject.SetHeldBy(targetGrab.grabParent.PrimaryHeld(), targetHand, this);

        targetGrab.StartSecondaryGrab(targetHand.trackedTransform.position, targetHand.trackedTransform.forward, targetHand.handType);

        targetGrab.beingHeld = true;

        targetHand.physicsHand.SetGrabbedObject(targetObject);

        //snap to the position

        //we set the layer of the target object to 8 : held item
        // targetObject.SetCollisionObjectLayers(8);
        targetHand.physicsHand.gameObject.layer = 8;
        SnapObjectToHand(targetHand, targetGrab);

        //to connect the rigidbody to the hands
        //  targetHand.physicsHand.BindObjectToRigidBody(targetObject.gameObject);
        targetHand.holdingItem = true;

        animationManager.SwitchHandPose(targetGrab.SecondaryGrabPoseKey(), targetHand.handType);

        a_Manager.PlaySound("VrGrab", 1f, 1f, 0.1f, targetGrab.GetGrabPosition(targetHand.physicsHand.transform, targetHand.handType), 0f);
    }

    void StartOffsetGrab(VrHandInputValues targetHand, GrabPoint targetGrab)
    {
        if(resetHandCoroutine != null)
        {
            StopCoroutine(resetHandCoroutine);
        }

        g_Logger.LogNotice("grabbed object " + targetGrab.grabParent.name);
        Debug.Log("grabbed object " + targetGrab.grabParent.name);

        targetGrab.StartedGrab(targetHand.trackedTransform, targetHand.handType);

        targetHand.physicsHand.GetComponent<Rigidbody>().isKinematic = true;
        targetHand.physicsHand.gameObject.layer = 8;
        targetHand.curGrabPoint = targetGrab;
        Debug.Log("curgrab point " + targetHand.curGrabPoint.transform.parent.name);

        targetGrab.beingHeld = true;

        GrabbableObject targetObject = targetGrab.grabParent;
        targetObject.isBeingHeld = true;
        if(targetObject.PrimaryHeld() != null) //sets the primary holder etc
        {
            targetObject.SetHeldBy(targetObject.PrimaryHeld(), targetHand, this);
        }
        else
        {
            targetObject.SetHeldBy(targetHand, null, this);
            targetGrab.grabParent.ObjectGrabbed();
        }

        targetObject.rb.useGravity = false;

        targetHand.physicsHand.SetGrabbedObject(targetObject);

        targetHand.holdingItem = true;

        CalculateGrabPointOffset(targetHand, targetHand.curGrabPoint.OffsetObject());

      //  MoveHandToMatchObject(targetGrab, targetHand);

        animationManager.SwitchHandPose(targetGrab.GetAnimationPose(), targetHand.handType);

        a_Manager.PlaySound("VrGrab", 1f, 1f, 0.1f, targetGrab.GetGrabPosition(targetHand.physicsHand.transform, targetHand.handType), 0f);
    }
    public void ReleaseGrip(VrHandInputValues targetHand)
    {
        g_Logger.LogNotice("released grip");
        Debug.Log("released grip");
        if (targetHand.holdingItem == true)
        {
            DropObject(targetHand);
        }


    }

    void DropObject(VrHandInputValues targetHand)
    {
        if (targetHand.physicsHand.HeldObject() != null)
        {
            GrabbableObject currentHeld = targetHand.physicsHand.HeldObject();

            targetHand.curGrabPoint.beingHeld = false;

            currentHeld.ObjectDropped();
            //if we are grabing secondary
            if (currentHeld.SecondaryHeld() != null)
            {
                if (currentHeld.SecondaryHeld().handType == targetHand.handType)
                {
                    currentHeld.SetHeldBy(currentHeld.PrimaryHeld(), null, this); //switches the primary hand
                }
                else
                {
                    currentHeld.SetHeldBy(currentHeld.SecondaryHeld(), null, this); //switches the primary hand
                }
                g_Logger.LogNotice("Dropped secondary" + currentHeld.gameObject.name);
                Debug.Log("Dropped secondary" + currentHeld.gameObject.name);
            }
            else
            {
                //if we are dropping solitatry
                currentHeld.SetHeldBy(null, null, this);

                currentHeld.rb.useGravity = true;
                currentHeld.rb.ResetCenterOfMass();
                CalculateObjectThrow(targetHand, currentHeld.rb);

                //     currentHeld.SetCollisionObjectLayers(10);
                //      currentHeld.gameObject.layer = 10; //setss layer to interactable object
                currentHeld.isBeingHeld = false;
                g_Logger.LogNotice("Dropped " + currentHeld.gameObject.name);
                Debug.Log("Dropped " + currentHeld.gameObject.name);
            }
            targetHand.curGrabPoint.OnGripReleased(targetHand.handType);
            targetHand.curGrabPoint = null;

        }

        resetHandCoroutine = StartCoroutine(ResetHandLayer(targetHand));

        if (doModifiedOffset)
        {
            EndModifiedOffset();
        }

        targetHand.holdingItem = false;

        animationManager.SwitchHandPose("", targetHand.handType);

        targetHand.physicsHand.GetComponent<Rigidbody>().isKinematic = false;
        targetHand.physicsHand.HeldObjectDropped();

        didOvershoot = false;
    }

    void CalculateObjectThrow(VrHandInputValues targetHand, Rigidbody targetRb)
    {

    }

    private Coroutine resetHandCoroutine;
    IEnumerator ResetHandLayer(VrHandInputValues targetHand)
    {
        yield return new WaitForSeconds(0.3f);
        //re-enable hand collision
        targetHand.physicsHand.gameObject.layer = 6;
    }

    void SnapObjectToHand(VrHandInputValues targetHand, GrabPoint targetPoint)
    {
        Vector3 grabPosition = targetPoint.GetGrabPosition(targetHand.trackedTransform, targetHand.handType);
        Quaternion grabRotation = targetPoint.GetGrabRotation(targetHand.trackedTransform, targetHand.handType);

        Quaternion difference = targetHand.trackedTransform.rotation * Quaternion.Inverse(grabRotation);
        targetPoint.grabParent.rb.MoveRotation(targetPoint.grabParent.rb.rotation * difference);

        Vector3 vectorDifference = grabPosition - targetHand.trackedTransform.position;
        targetPoint.grabParent.rb.MovePosition(targetPoint.grabParent.rb.position - vectorDifference);

        MoveHandToMatchObject(targetPoint, targetHand);
    }

    public void PerformGrabMovement(VrHandInputValues targetHand)
    {
        if (doModifiedOffset)
        {
            if(targetHand.handType == modifiedOffsetHandType)
            {
                DoModifiedOffsetMove(targetHand);
                return;
            }
        }

        if (targetHand.curGrabPoint.isOffsetGrab == true) 
        {
            MoveOffset(targetHand, targetHand.curGrabPoint.OffsetObject());
        }

        if(targetHand.curGrabPoint.grabParent.SecondaryHeld() != null) //prevents double calculations through both handss
        {
            Debug.Log("not null");
            if (targetHand.curGrabPoint.grabParent.SecondaryHeld().curGrabPoint != null)
            {
                if (targetHand.curGrabPoint.DoingSecondaryGrab())
                {
                    //for if doing ie pistol grip 
                    if (targetHand.handType == targetHand.curGrabPoint.grabParent.SecondaryHeld().handType)
                    {
                        DoFixedMove(targetHand);
                        return; //if its the primary hand then it just continues the loop normally
                    }
                }
                else
                {
                    if (targetHand != targetHand.curGrabPoint.grabParent.SecondaryHeld())
                    {
                        bool doDoDouble = true;

                        if (targetHand.curGrabPoint.grabParent.grabType == GrabbableObject.GrabType.physics)
                        {
                            doDoDouble = false;
                        }

                        if (targetHand.curGrabPoint.isOffsetGrab && !targetHand.curGrabPoint.onOffsetChangeTranform)
                        {
                            doDoDouble = false;
                        }
                        else
                        {
                            if (targetHand.curGrabPoint.grabParent.SecondaryHeld().curGrabPoint.isOffsetGrab && !targetHand.curGrabPoint.grabParent.SecondaryHeld().curGrabPoint.onOffsetChangeTranform)
                            {
                                doDoDouble = false;
                            }
                        }

                        if (doDoDouble)
                        {
                            MoveDoubleHanded(targetHand, targetHand.curGrabPoint.grabParent.SecondaryHeld());
                            return;
                        }
                    }
                    else
                    {
                        //targetHand.curGrabPoint.grabParent.SetHeldBy(targetHand.curGrabPoint.grabParent.PrimaryHeld(), null, this);
                    }
                }
            }           
        }

        switch (targetHand.physicsHand.HeldObject().grabType)
        {
            case GrabbableObject.GrabType.standard:
                if (targetHand.curGrabPoint.grabParent.SecondaryHeld() != targetHand)
                {
                    MoveStandard(targetHand);
                }
                break;
            case GrabbableObject.GrabType.melee:

                break;
            case GrabbableObject.GrabType.gun:

                break;
            case GrabbableObject.GrabType.fixedGrab:
                DoFixedMove(targetHand);
                break;
            case GrabbableObject.GrabType.physics:
                MovePhysics(targetHand);
                break;
            default:
                break;
        }
    }

    public void UpdateHandMovementLoop(VrHandInputValues targetHand)
    {
        if (!targetHand.holdingItem) return;

        GrabPoint curPoint = targetHand.curGrabPoint;
        MoveHandToMatchObject(curPoint, targetHand);
    }

    #region standardMovement
    Vector3 GetTargetPosition(VrHandInputValues targetHand, GrabPoint targetPoint)
    {
        Debug.Log("targ point + " + targetPoint.transform.parent.name);
        Vector3 grabPosition = targetPoint.GetGrabPosition(targetHand.physicsHand.transform, targetHand.handType);

        Vector3 vectorDifference = grabPosition - targetHand.physicsHand.trackedTransform.position;

        Vector3 rawTarget = targetPoint.grabParent.rb.position - vectorDifference;

        //adjust raw target for different object weights etc

        return rawTarget;   
    }

    Quaternion GetTargetRotation(VrHandInputValues targetHand, GrabPoint targetPoint) 
    {
        Quaternion grabRotation = targetPoint.GetGrabRotation(targetHand.physicsHand.transform, targetHand.handType);
        Quaternion difference = targetHand.physicsHand.trackedTransform.rotation * Quaternion.Inverse(grabRotation);

        Debug.DrawLine(targetHand.physicsHand.trackedTransform.position, targetHand.physicsHand.trackedTransform.position + targetHand.physicsHand.trackedTransform.forward, Color.grey);

        return difference * targetPoint.grabParent.transform.rotation;
    }
    void MoveStandard(VrHandInputValues targetHand)
    {
       
        //we will move the object to where the hand would be. The hand itself will follow the items grab positionb
    //    Debug.Log("target hand to move " + targetHand.handType);


        Quaternion targetRotation = GetTargetRotation(targetHand, targetHand.curGrabPoint);
        Vector3 targetPosition = GetTargetPosition(targetHand, targetHand.curGrabPoint);

        Vector3 curGrabPos = targetHand.curGrabPoint.GetGrabPosition(targetHand.curGrabPoint.grabParent.transform, targetHand.handType);
        MovePhysicsRotation(targetHand.curGrabPoint.grabParent.rb, targetRotation, curGrabPos, targetPosition);

        MovePhysicsPosition(targetHand.curGrabPoint.grabParent.rb, targetPosition);

       // MoveHandToMatchObject(targetHand.curGrabPoint, targetHand);
    }

    public void MovePhysicsRotation(Rigidbody body, Quaternion trackedTransform, Vector3 heldAt, Vector3 targetPosition)
    {
    
         body.centerOfMass = body.transform.InverseTransformPoint(heldAt); //must have scale of 1 
         body.angularVelocity *= slowDownAngularVelocity;

        Debug.DrawLine(body.position + (trackedTransform * body.transform.forward) * 0.2f, body.position, Color.red, Time.deltaTime);

        Vector3 angularVelocity = FindNewAngularVelocity(body, trackedTransform);
        float angluarTest = Quaternion.Angle(body.rotation, trackedTransform);

        if (angluarTest < anglularCuttoff && angluarTest > 1f)
        {
            angularVelocity = angularVelocity * (angluarTest / anglularCuttoff) * (angluarTest / anglularCuttoff);
        }

        if (angluarTest < 1f)
        {
            body.MoveRotation(Quaternion.Slerp(body.rotation, trackedTransform, 10f * Time.deltaTime));
        }
        else
        {

            if (IsValidVelocity(angularVelocity.x))
            {
                float maxChange = maxRotationChange * Time.deltaTime * handRotateMultiplier;
              body.angularVelocity = Vector3.MoveTowards(body.angularVelocity, angularVelocity, maxChange);
            
                Vector3 useVel = (angularVelocity - body.angularVelocity) * Time.deltaTime * handRotateMultiplier;

                Debug.DrawLine(body.position, body.position + useVel, Color.blue);

                Vector3 dif = heldAt - targetPosition;
                Vector3 toTarget = heldAt - body.position;
                toTarget += dif;

                Vector3 magnitudeAdjust = toTarget.normalized * useVel.magnitude * torqueCompensationMultiplier;

                body.AddForce(magnitudeAdjust * Time.deltaTime, ForceMode.VelocityChange);
                Debug.DrawLine(body.position, body.position + (magnitudeAdjust * 0.3f), Color.cyan);
                body.AddTorque(useVel, ForceMode.VelocityChange);
            }
        }
    }
    public void MovePhysicsPosition(Rigidbody body, Vector3 trackedTransform)
    {
        body.velocity *= slowDownVelocity;

        Vector3 velocity = FindNewVelocity(body, trackedTransform);

        if (IsValidVelocity(velocity.x))
        {
           // body.velocity = Vector3.MoveTowards(body.velocity, velocity, maxChange);
           body.AddForce((velocity - body.velocity) * Time.deltaTime * handMoveMultiplier, ForceMode.VelocityChange);
        }
    }

    bool IsValidVelocity(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    Vector3 FindNewVelocity(Rigidbody body, Vector3 trackedTransform)
    {
        Vector3 worldPosition = trackedTransform;
        Vector3 difference = worldPosition - body.position;
        return difference / Time.deltaTime;
    }
    private Vector3 FindNewAngularVelocity(Rigidbody body, Quaternion trackedTransform)
    {
        Quaternion worldRotation = trackedTransform;
        //modify the world rotation to fit gravitational influence
        Quaternion difference = worldRotation * Quaternion.Inverse(body.rotation);
        difference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f)
        {
            angleInDegrees -= 360f;
        }       

        return (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.deltaTime;
    }

    void MoveHandToMatchObject(GrabPoint targetPoint, VrHandInputValues handValues)
    {

        Quaternion targetRotation = targetPoint.GetGrabRotation(handValues.physicsHand.transform, handValues.handType);
        Vector3 targetPosition = targetPoint.GetGrabPosition(handValues.physicsHand.transform, handValues.handType);

        if (targetPoint.DoingSecondaryGrab()) //accoutn for difference in secondary grab
        {
            if(targetPoint.grabParent.SecondaryHeld().handType == handValues.handType)
            {
                targetRotation = targetPoint.GetSecondaryGrabTransform().rotation;
                targetPosition = targetPoint.GetSecondaryGrabTransform().position;
            }
        }

        handValues.physicsHand.body.MoveRotation(targetRotation);
        handValues.physicsHand.body.MovePosition(targetPosition);
    }

    #endregion

    #region doubled handed movement
    void MoveDoubleHanded(VrHandInputValues primaryHand, VrHandInputValues secondaryHand)
    {
        Transform mainHandReference = primaryHand.trackedTransform;
        Transform secondaryHandReference = secondaryHand.trackedTransform;
        Debug.Log("mainhand" + mainHandReference.gameObject.name + " secondary hand " + secondaryHandReference.gameObject.name);

        Rigidbody mainHolder = primaryHand.physicsHand.HeldObject().rb;
        
        Vector3 grabP1 = primaryHand.curGrabPoint.GetGrabPosition(mainHandReference, primaryHand.handType); 
        Vector3 grabP2 = secondaryHand.curGrabPoint.GetGrabPosition(secondaryHandReference, secondaryHand.handType);
        Transform mainGrabTransform = primaryHand.curGrabPoint.GetCurrentOrientationTransform();

        //move double handed
        Debug.DrawLine(secondaryHandReference.position, mainHandReference.position, Color.green, Time.deltaTime);
        //primaryHand.curGrabPoint.GetGrabPosition(mainHandReference, primaryHand.handType)    

        Vector3 vectorDifference = grabP1 - mainHandReference.position;
        Vector3 doPosition = mainHolder.position - vectorDifference;

        Vector3 currentPosition = mainHolder.position;

        mainHolder.velocity *= slowDownVelocityDual;

        Vector3 worldPosition = doPosition;
        Vector3 difference = worldPosition - currentPosition;
        Vector3 targetVelocity = difference / Time.deltaTime;
        if (IsValidVelocity(targetVelocity.x))
        {
            // mainHolder.velocity = Vector3.MoveTowards(mainHolder.velocity, targetVelocity, maxChange);
            mainHolder.AddForce((targetVelocity - mainHolder.velocity) * Time.deltaTime * handMoveMultiplier, ForceMode.VelocityChange);
        }

        //calculate rotation (the hard part)
        mainHolder.centerOfMass = mainHolder.transform.InverseTransformPoint(grabP1); //must have scale 1

        Vector3 rawDirection = grabP2 - grabP1; // the current direction of the aim barrel
        Debug.DrawLine(grabP2, grabP1, Color.red, Time.deltaTime);

        //get base values 
        float lengthMagnitude = Vector3.Magnitude(rawDirection);
        Vector3 aimDirection = lengthMagnitude * (secondaryHandReference.position - mainHandReference.position).normalized; // this is the base direction the new rotation will face    
        Debug.DrawLine(mainHandReference.position, mainHandReference.position + aimDirection, Color.green, Time.deltaTime);

        //we form the two rotations of the object
        Quaternion currentAimRotation = Quaternion.LookRotation(rawDirection, mainGrabTransform.up); //we use the primary hand upwards direction
        Quaternion targetAimRotation = Quaternion.LookRotation(aimDirection, mainHandReference.up);

        Quaternion rotationalDifference = targetAimRotation * Quaternion.Inverse(currentAimRotation);

        Quaternion targetRot = rotationalDifference * mainHolder.rotation;

        mainHolder.MoveRotation(targetRot);

        mainHolder.angularVelocity *= slowDownAngularVelocityDual;
        float angluarTest = Quaternion.Angle(mainHolder.rotation, targetRot);

        //for offset modifications
        if (secondaryHand.curGrabPoint.isOffsetGrab == true)
        {
            XROffsetObject offsetObj = secondaryHand.curGrabPoint.OffsetObject();
            if (!offsetObj.doOffsetDualDirection)
            {
                //we make it so the object mainstains direction of primary hand insead
                targetRot = GetTargetRotation(primaryHand, primaryHand.curGrabPoint); //convertss to be like a normal
            }
        }
        Vector3 targetAnglularVelocity = FindNewAngularVelocity(mainHolder, targetRot);

        if (angluarTest < anglularCuttoff)
        {
            targetAnglularVelocity = targetAnglularVelocity * (angluarTest / anglularCuttoff);
        }

        if (IsValidVelocity(targetAnglularVelocity.x))
        {
            Vector3 useVel = (targetAnglularVelocity - mainHolder.angularVelocity) * Time.deltaTime * handRotateMultiplier;
           // mainHolder.AddTorque(useVel, ForceMode.VelocityChange);
        }
    }

    #endregion

    #region offsetMove
    private Vector3 handGrabOffset;
    private bool didOvershoot = false;
    private Vector3 overShootStartPosition;
    void CalculateGrabPointOffset(VrHandInputValues targetHand, XROffsetObject targetObject)
    {
        overShootStartPosition = targetHand.trackedTransform.position;
        handGrabOffset = targetHand.curGrabPoint.transform.position - targetObject.UpdatePosition(targetObject.currentOffsetValue);
        targetHand.curGrabPoint.transform.position = targetObject.UpdatePosition(targetObject.currentOffsetValue) + handGrabOffset;
    }
    void MoveOffset(VrHandInputValues targetHand, XROffsetObject targetObject)
    {

        //if we want to apply the grab point to move as well
        Transform toMove = targetObject.toMove;
        Vector3 checkOffset = targetHand.curGrabPoint.GetGrabPosition(targetHand.physicsHand.transform, targetHand.handType) - targetHand.curGrabPoint.transform.position;
        Vector3 moveReference = targetHand.trackedTransform.position - checkOffset;
        Vector3 offsetA = targetObject.start.position;
        Vector3 offsetB = targetObject.end.position;    
 
        Vector3 referencePosition = moveReference;
        Vector3 betweenOffset = offsetB - offsetA;

        if (!targetObject.excludePositionCalculations) { 

            float dot = Vector3.Dot(betweenOffset, referencePosition - offsetA);

            if (dot > 0f)
            {
                Vector3 projected = (Vector3.Project(referencePosition - offsetA, betweenOffset));// / Vector3.SqrMagnitude(betweenOffset)) * betweenOffset; //projection in case unalligned

                float currentBetween = Mathf.Clamp01((toMove.position - offsetA).magnitude / betweenOffset.magnitude);

                float projMagnitude = projected.magnitude;
                Vector3 endPos = targetObject.end.position;
                float betweenDist = betweenOffset.magnitude;
                if (projMagnitude > betweenDist)
                {
                    if (targetObject.doOverShoot)
                    {
                        if (targetObject.WillOvershoot())
                        {
                            //we adjust 
                            endPos = Vector3.Lerp(endPos, targetObject.start.position + (betweenOffset.normalized * (betweenDist + targetObject.offshootThresshold)), Time.deltaTime * 10f);
                        }
                        if (targetObject.offshootThresshold < (projMagnitude - betweenOffset.magnitude) && Vector3.Distance(overShootStartPosition, targetHand.trackedTransform.position) > targetObject.offshootThresshold) //the second part is to stop offshot from occuring instantly when grabing the behind of object
                        {
                            //we detect over shoot //useful for releaseing slides of gun back etc
                            if (!didOvershoot)
                            {
                                targetObject.OverShoot();

                                if (targetObject.releaseOnOvershoot)
                                {
                                    DropObject(targetHand);
                                }
                                return;

                            }
                            didOvershoot = true;
                        }
                        else
                        {
                            didOvershoot = false;
                        }
                    }
                    projMagnitude = betweenDist;
                }
                else
                {
                    didOvershoot = false;
                }

                float targetBetween = (projMagnitude / betweenOffset.magnitude);
                //  Debug.Log(currentBetween + "current between");
                float toSpeed = Mathf.Abs(1f / dot) * Vector3.Distance(toMove.position, referencePosition);
                float lerpedTo = Mathf.Lerp(currentBetween, targetBetween, toSpeed);
                lerpedTo = Mathf.Clamp01(lerpedTo);

                if (targetObject.doSteppedMovement) //for stepped shifting
                {
                    int intervals = targetObject.movementSteps;
                    float intervalSize = 1f / intervals;


                }

                Vector3 newPosition = Vector3.Lerp(targetObject.start.position, endPos, lerpedTo);
                toMove.position = newPosition;

                //to do resisted movement

                //targetHand.curGrabPoint.transform.position = newPosition;
                targetObject.currentOffsetValue = lerpedTo;
            }

            if (targetObject.requireRotationmatching)
            {
                //rotation component
                Quaternion rotA = targetObject.start.rotation;
                Quaternion rotB = targetObject.end.rotation;

            }

            targetObject.OnValueChanged();
        }
        else
        {

        }
    }
    #endregion

    #region physicsMove

    void MovePhysics(VrHandInputValues targetHand)
    {
        //we add forces to atttempt to match object to hand, suitable for larger and heavier objects

        GrabPoint targetPoint = targetHand.curGrabPoint;
        Transform trackedTarget = targetHand.trackedTransform;
        Vector3 trackingVelocity = targetHand.trackedVelocity;



    }

    #endregion//not yet implemented

    #region fixed move
    private void DoFixedMove(VrHandInputValues targetHand)
    {
        GrabPoint gPoint = targetHand.curGrabPoint;
       // MoveHandToMatchObject(gPoint, targetHand); //sticks the hand to the fixed point
    }
    #endregion

    #region modified offests

    private bool doModifiedOffset = false;
    private HeldObjectInputFemale femaleInputTarget;

    private XRControlManager.HandType modifiedOffsetHandType;
    private VrHandInputValues modOffsetHand;
    public void StartModifiedOffset(HeldObjectInputFemale targetInput, HeldObjectInputMale maleInput, VrHandInputValues targetHand)
    {
      
        femaleInputTarget = targetInput;
        modifiedOffsetHandType = targetHand.handType;
        modOffsetHand = targetHand;
        doModifiedOffset = true;

        Debug.Log("started modified offset");
        targetInput.OnStartInput(maleInput, this);
    }
    public void EndModifiedOffset()
    {
        doModifiedOffset = false;

        femaleInputTarget.OnEndInput();
    }
    public void CancelModifiedOffset()
    {
        doModifiedOffset = false;
        DropObject(modOffsetHand);
    }

    public bool ModifiedInputStartRequest()
    {
        if(femaleInputTarget == null)
        {
            return false;
        }
        if(doModifiedOffset == true || !femaleInputTarget.CanBeAccessed(femaleInputTarget.CurrentInput()))
        {
            return false;
        }
        return true;
    }

    void DoModifiedOffsetMove(VrHandInputValues targetHand)
    {
        Transform start = femaleInputTarget.positionalStart;
        Transform end = femaleInputTarget.positionalEnd;
        Rigidbody toMove = targetHand.curGrabPoint.grabParent.GetComponent<Rigidbody>();

        Vector3 betweenOffset = end.position - start.position;
        Vector3 referencePosition = toMove.position + (targetHand.trackedTransform.position - targetHand.physicsHand.body.position);

        Debug.DrawLine(start.position, referencePosition, Color.magenta, Time.deltaTime); //reference line

        float dot = Vector3.Dot(betweenOffset, referencePosition - start.position);
        if(dot < 0f)
        {
            if ((referencePosition - start.position).magnitude > 0.1f)
            {
                //we eject
                EndModifiedOffset();
                femaleInputTarget.EjectObject();
                return;
            }
            dot = 0f;

            referencePosition = start.position;
        }

        

        Vector3 projected = Vector3.Project(referencePosition - start.position, betweenOffset);// (Vector3.Dot(betweenOffset, referencePosition - start.position) / Vector3.SqrMagnitude(betweenOffset)) * betweenOffset; //projection in case unalligned

        float currentBetween = (toMove.position - start.position).magnitude / betweenOffset.magnitude;

        float projMagnitude = projected.magnitude;
        if (projMagnitude > betweenOffset.magnitude)
        {
            projMagnitude = betweenOffset.magnitude;
        }
        float targetBetween = (projMagnitude / betweenOffset.magnitude);
        //  Debug.Log(currentBetween + "current between");

        float toSpeed = Mathf.Abs(1f / dot) * Vector3.Distance(toMove.position, referencePosition) * Time.deltaTime * 5f;
        float lerpedTo = Mathf.Lerp(currentBetween, targetBetween, toSpeed);
        lerpedTo = Mathf.Clamp01(lerpedTo);
        Debug.Log("doing modified offsset movement");

        Vector3 newPosition = Vector3.Lerp(start.position, end.position, lerpedTo);
        toMove.MovePosition(newPosition);

        bool reached = femaleInputTarget.CheckReached();
        if (reached && femaleInputTarget.releaseGripOnReached)
        {
            Debug.Log("reached end and dropped");
            DropObject(targetHand); //we drop the object automatically when we reach end
        }

        //constrain rotation
        float distanceFromStart = Vector3.Distance(start.position, toMove.position);
        float distanceBetweenPoints = Vector3.Distance(start.position, end.position);
        float lerpAmount = distanceFromStart / distanceBetweenPoints;

        Quaternion targRot = Quaternion.Lerp(start.rotation, end.rotation, lerpAmount);

       toMove.MoveRotation(targRot);
       // toMove.velocity = Vector3.zero;
       // targetHand.physicsHand.body.velocity = Vector3.zero; // stop weird physics shit
    }

    #endregion

    #region recieve input
    public void SendTriggerUp(VrHandInputValues targetHand)
    {
        GrabPoint targetPoint = targetHand.curGrabPoint;
        targetPoint.TriggerInputUp();
        targetPoint.grabParent.TriggerUp();
    }

    public void SendTriggerDown(VrHandInputValues targetHand)
    {
        GrabPoint targetPoint = targetHand.curGrabPoint;
        targetPoint.TriggerInputDown();
        targetPoint.grabParent.TriggerDown();
    }

    public void SendMainButton(VrHandInputValues targetHand)
    {
        if (targetHand.holdingItem)
        {
            GrabPoint targetPoint = targetHand.curGrabPoint;
            targetPoint.MainButtonInput();
            targetPoint.grabParent.MainButtonPressed();
        }
    }

    public void SendSecondaryButton(VrHandInputValues targetHand)
    {
        
    }


    #endregion
}
