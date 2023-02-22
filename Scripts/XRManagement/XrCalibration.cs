using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XrCalibration : MonoBehaviour
{
    #region singletonPatern
    public static XrCalibration Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion
    //this scripts serves for calibrating player height and player arm length to fit to the world
    //ensure worlds scale is set to one

    [Header("Settings")]
    public Transform xrPlayerBase;
    public Transform xrCameraReference; //this is what we take the current height from
    public Transform playerBody;
    [Space]
    public Transform leftPhysicsHand;
    public Transform rightPhysicsHand;

    public float defaultHeight = 1.8f;

    //this is for changing the player body specifically
    public void CallibrateHeight()
    {
        float heightDifference = xrCameraReference.transform.position.y - xrPlayerBase.position.y;
        float ratio = defaultHeight / heightDifference;
        float inverseRatio = heightDifference / defaultHeight; //use the inverse to keep consistant size of hand
        Vector3 desiredScale = Vector3.one * ratio;
        Vector3 desiredHandScale = Vector3.one * inverseRatio;
        playerBody.transform.localScale = desiredScale;

        leftPhysicsHand.localScale = desiredHandScale;
        rightPhysicsHand.localScale = desiredHandScale;
    }

    public void RecenterCamera()
    {

    }
}
