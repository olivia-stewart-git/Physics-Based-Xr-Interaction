using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDualHandScript : MonoBehaviour
{
    public Transform mainHandReference;
    public Transform secondaryHandReference;

    public Rigidbody mainHolder;

    public Transform grabP1;
    public Transform grabP2;

    public Transform toRotate;

    [Space]
    [Range(0, 1)] public float slowDownVelocity = 0.75f;
    [Range(0, 1)] public float slowDownAngularVelocity = 0.75f;


    [Range(0, 100)] public float maxPositionChange = 75.0f;
    [Range(0, 100)] public float maxRotationChange = 75.0f;
    public float rotationSpeedMultiplier = 1.5f;
    public float velocityMultiplier = 2f;

    public float anglularCuttoff = 5f;

    [Range(0f, 1f)]
    public float secondaryHandTiltStrength = 0.3f;
    [Header("Offset settings")]
    public bool affectOffsetObject = true;
    public Rigidbody offsetRigidBody;

    public Transform offsetA;
    public Transform offsetB;
    public Transform offsetCurrentPosition;
    // Start is called before the first frame update
    void Start()
    {
        InitialiseHandle(offsetRigidBody, secondaryHandReference);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateDoublePosition();   
    }

    void UpdateDoublePosition()
    {

        Debug.DrawLine(secondaryHandReference.position, mainHandReference.position, Color.green, Time.deltaTime);


        //move to target
        if (affectOffsetObject)
        {
            Vector3 pos = CalculateOffsetMove(offsetRigidBody, secondaryHandReference);
            grabP2.position = pos + offsetHandleValue;

            //stop the whole thing from movings
            
        }


        Vector3 vectorDifference = grabP1.position - mainHandReference.position;
        Vector3 doPosition = mainHolder.position - vectorDifference;

        Vector3 currentPosition = mainHolder.position;

        mainHolder.velocity *= slowDownVelocity;

        Vector3 worldPosition = doPosition;
        Vector3 difference = worldPosition - currentPosition;
        Vector3 targetVelocity = difference / Time.deltaTime;
        if (IsValidVelocity(targetVelocity.x))
        {
            float maxChange = maxPositionChange * Time.deltaTime * velocityMultiplier;
            mainHolder.velocity = Vector3.MoveTowards(mainHolder.velocity, targetVelocity, maxChange);
        }

        //calculate rotation (the hard part)

        Vector3 rawDirection = grabP2.position - grabP1.position;
        Debug.DrawLine(grabP2.position, grabP1.position, Color.red, Time.deltaTime);
        Debug.DrawLine(mainHolder.position, mainHolder.position + mainHolder.transform.forward * 0.7f, Color.blue, Time.deltaTime);

        //get base values
        float lengthMagnitude = Vector3.Magnitude(grabP2.position - grabP1.position);
        
        Vector3 aimOffset = mainHolder.transform.forward * lengthMagnitude;
        Vector3 offsetDifference = aimOffset - rawDirection;
        Vector3 aimDirection = lengthMagnitude * ((secondaryHandReference.position - mainHandReference.position) + offsetDifference).normalized;


        //calculate target upwards direction
        Vector3 updwardsDirection = GetTargetUpwardsDirection(aimDirection);
        
        Quaternion baseRotation = Quaternion.LookRotation(aimDirection, updwardsDirection);//thiss is the base target of potation
   

        Debug.DrawLine(mainHandReference.position, mainHandReference.position + (updwardsDirection * 0.3f), Color.green, Time.deltaTime);
        mainHolder.angularVelocity *= slowDownAngularVelocity;
        float angluarTest = Quaternion.Angle(mainHolder.rotation, baseRotation);
        Vector3 targetAnglularVelocity = FindNewAngularVelocity(baseRotation);

        if(angluarTest < anglularCuttoff)
        {
            targetAnglularVelocity = targetAnglularVelocity * (angluarTest / anglularCuttoff);
        }

            if (IsValidVelocity(targetAnglularVelocity.x))
            {
                float maxChange = maxRotationChange * Time.deltaTime * rotationSpeedMultiplier;
                mainHolder.angularVelocity = Vector3.MoveTowards(mainHolder.angularVelocity, targetAnglularVelocity, maxChange);
            }
        
    }

    private Vector3 FindNewAngularVelocity(Quaternion inputRotation)
    {
        Quaternion worldRotation = inputRotation;
        Quaternion difference = worldRotation * Quaternion.Inverse(mainHolder.rotation);
        difference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f)
        {
            angleInDegrees -= 360f;
        }

        return (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.deltaTime;
    }

    Vector3 GetTargetUpwardsDirection(Vector3 aimDirection)
    {      
        Vector3 cross = Vector3.Cross(aimDirection, mainHandReference.right);
        Vector3 secondaryCross = Vector3.Cross(aimDirection, secondaryHandReference.right);


        Vector3 combined = Vector3.Lerp(cross, secondaryCross, secondaryHandTiltStrength);
        
        return combined.normalized;
    }
    bool IsValidVelocity(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private Vector3 offsetHandleValue;

    void InitialiseHandle(Rigidbody toMove, Transform grabPos)
    {
        offsetHandleValue = grabPos.position - toMove.position;
    }
    Vector3 CalculateOffsetMove(Rigidbody toMove, Transform moveReference)
    {
        Vector3 referencePosition = moveReference.position - offsetHandleValue;
        Vector3 betweenOffset = offsetB.position - offsetA.position;

        float dot =  Vector3.Dot(betweenOffset, referencePosition - offsetA.position);

        if (dot > 0f)
        {

            Vector3 projected = (Vector3.Dot(betweenOffset, referencePosition - offsetA.position) / Vector3.SqrMagnitude(betweenOffset)) * betweenOffset; //projection in case unalligned

            float currentBetween = Mathf.Clamp01((toMove.position - offsetA.position).magnitude / betweenOffset.magnitude);

            float projMagnitude = projected.magnitude;
            if (projMagnitude > betweenOffset.magnitude)
            {
                projMagnitude = betweenOffset.magnitude;
            }

            float targetBetween = Mathf.Clamp01(projMagnitude / betweenOffset.magnitude);
            Debug.Log(currentBetween + "current between");

            float toSpeed = Mathf.Abs(1f / dot) * Vector3.Distance(toMove.position, referencePosition);
            float lerpedTo = Mathf.Lerp(currentBetween, targetBetween, toSpeed);

            Vector3 newPosition = Vector3.Lerp(offsetA.position, offsetB.position, lerpedTo);
            toMove.transform.position = newPosition;
            return newPosition;
        }
        return toMove.position;
    }
}

