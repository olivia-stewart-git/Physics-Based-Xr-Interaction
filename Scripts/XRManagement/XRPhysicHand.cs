using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRPhysicHand : MonoBehaviour
{
    [Header("HandInputs")]
    public Animator handanimator;

    public Transform trackedTransform = null;
    public Transform rayInteractionPoint;
    public Transform fingerInteractionPoint;
    [HideInInspector] public Rigidbody body = null;

    [Header("Grabbing Items")]
    public Transform grabCentre;

    private bool isGrabbing = false;
    private GrabbableObject currentGrabbed;

    [Header("Moving hands")]
    public float maxHandRange = 1f;
    public  float physicsRange = 0.1f;
    public LayerMask physicsMask = 0;

    [Range(0, 1)] public float slowDownVelocity = 0.75f;
    [Range(0, 1)] public float slowDownAngularVelocity = 0.75f;

    [Range(0, 100)] public float maxPositionChange = 75.0f;
    [Range(0, 100)] public float maxRotationChange = 75.0f;

    public float anglularCuttoff = 5f;

    public float handMoveMultiplier = 2f;
    public float handRotateMultiplier = 3f;

    private Quaternion targetRotation = Quaternion.identity;
    private Vector3 targetPosition = Vector3.zero;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        InitialiseValues();
    }

    void InitialiseValues()
    {
        MoveWithoutPhysics();
    }

    public bool IsGrabbing()
    {
        return isGrabbing;
    }

    public void ReleaseGrip()
    {

    }

    #region movingHand
    //this should always be called from a fixed update loop
    public void UpdateHandPosition() //this is only called by control manager
    {
        //we play catcup
        if (Vector3.Distance(trackedTransform.position, transform.position) > maxHandRange)
        {
            body.transform.position = trackedTransform.position;
        }

        if (ShouldMoveWithPhysics())
        {


            MovePhysicsPosition();
            MovePhysicsRotation();
        }
        else
        {
            MoveWithoutPhysics();
        }
    }

    public void MovePhysicsRotation()
    {
        body.angularVelocity *= slowDownAngularVelocity;

        Vector3 angularVelocity = FindNewAngularVelocity();
        float angluarTest = Quaternion.Angle(body.rotation, trackedTransform.rotation);

        if (angluarTest < anglularCuttoff)
        {
            angularVelocity = angularVelocity * (angluarTest / anglularCuttoff);
        }

        if (IsValidVelocity(angularVelocity.x))
        {
            float maxChange = maxRotationChange * Time.deltaTime * handRotateMultiplier;
            body.angularVelocity = Vector3.MoveTowards(body.angularVelocity, angularVelocity, maxChange);
        }
    }
    public void MovePhysicsPosition()
    {
        body.velocity *= slowDownVelocity;
       
        Vector3 velocity = FindNewVelocity();

        if (IsValidVelocity(velocity.x))
        {
            float maxChange = maxPositionChange * Time.deltaTime * handMoveMultiplier;
            //body.velocity = Vector3.MoveTowards(body.velocity, velocity, maxChange);

            // body.velocity = Vector3.MoveTowards(body.velocity, velocity, maxChange);
            body.AddForce((velocity - body.velocity) * Time.deltaTime * handMoveMultiplier, ForceMode.VelocityChange);
        }
    }

   bool IsValidVelocity(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    Vector3 FindNewVelocity()
    {
        Vector3 worldPosition = trackedTransform.position;
        Vector3 difference = worldPosition - body.position;
        return difference / Time.deltaTime;
    }
    private Vector3 FindNewAngularVelocity()
    {
        Quaternion worldRotation = trackedTransform.rotation;
        Quaternion difference = worldRotation * Quaternion.Inverse(body.rotation);
        difference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if(angleInDegrees > 180f)
        {
            angleInDegrees -= 360f;
        }

        return (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.deltaTime;
    }

    public bool ShouldMoveWithPhysics()
    {
        //  if (isGrabbing)
        // {
        //    return true;
        //}

        return Physics.CheckSphere(transform.position, physicsRange, physicsMask, QueryTriggerInteraction.Ignore);
    }


    public void MoveWithoutPhysics()
    {
        body.velocity = Vector3.zero;

        body.MovePosition(trackedTransform.position);
        body.MoveRotation(trackedTransform.rotation);
    }
    #endregion

    public void SetGrabbedObject(GrabbableObject g_object)
    {
        currentGrabbed = g_object;
        isGrabbing = true;
   
    }

    public void BindObjectToRigidBody(GameObject targetToBind)
    {
        //we connect it with a joint



    }
    public void HeldObjectDropped()
    {


        currentGrabbed = null;

        isGrabbing = false;
    }
    public GrabbableObject HeldObject()
    {
        return currentGrabbed;
    }

    public void BindHandToRigidBody()
    {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, physicsRange);
        if(rayInteractionPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayInteractionPoint.position, rayInteractionPoint.forward);
        }
    }
}
