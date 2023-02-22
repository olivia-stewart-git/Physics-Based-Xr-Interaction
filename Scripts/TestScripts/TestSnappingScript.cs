using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSnappingScript : MonoBehaviour
{
    public Transform snapTo;
    public Transform snapObject;
    public Transform snapPoint;

    public GrabPoint.GrabPointTransformType pointType;

    public Transform orientationReference;
    public Transform mainOrientator;
    public Transform secondaryOrientator;

    public float grabRadius;
    public void DoSnap()
    {
        Quaternion difference = snapTo.rotation * Quaternion.Inverse(snapPoint.rotation);
        snapObject.rotation = snapObject.rotation * difference;

        Vector3 vectorDifference = snapPoint.position - snapTo.position;
        snapObject.position = snapObject.position - vectorDifference;
    }


    public Vector3 GetGrabPosition(Transform input)
    {
        Vector3 targetPosition = Vector3.zero;
        switch (pointType)
        {
            case GrabPoint.GrabPointTransformType.standard:
                targetPosition = orientationReference.position;
                break;
            case GrabPoint.GrabPointTransformType.line:
                Vector3 offset = orientationReference.position - secondaryOrientator.position;

                Vector3 between = secondaryOrientator.position - mainOrientator.position;
                Vector3 betweenHand = input.position - mainOrientator.position;
                float dot = Vector3.Dot(between, betweenHand);
                if (dot <= 0f)
                {
                    targetPosition = mainOrientator.position;
                    break;
                }

                //project the vector upon
                float squareMagnitude = Vector3.SqrMagnitude(between);
                Vector3 projection = (dot / squareMagnitude) * between;
                targetPosition = mainOrientator.position + projection;

                break;
            case GrabPoint.GrabPointTransformType.radial:
                Vector3 normalized = (input.position - mainOrientator.position).normalized;
                targetPosition = mainOrientator.position + (normalized * grabRadius);
                break;
        }
        return targetPosition;
    }

}
