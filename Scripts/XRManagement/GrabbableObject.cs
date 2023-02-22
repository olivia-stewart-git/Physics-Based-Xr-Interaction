using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour
{
    [HideInInspector]public XRGrabManager heldManager;

    public string objectName;
    public enum GrabType {standard, melee, gun, fixedGrab, physics}
    public GrabType grabType;

    public List<GrabPoint> grabbablePoints = new List<GrabPoint>();
    public List<GameObject> collisionObjects = new List<GameObject>();

    [HideInInspector] public Rigidbody rb;

    public bool isBeingHeld = false;

    public UnityEvent OnTriggerDown = new UnityEvent();
    public UnityEvent OnTriggerUp = new UnityEvent();
    public UnityEvent OnMainButton = new UnityEvent();

    public UnityEvent OnObjectGrabbed = new UnityEvent();
    public UnityEvent OnObjectDropped = new UnityEvent();

    public float weight = 1f;

    public Transform centreOfMass;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();


        if(centreOfMass == null)
        {         
            centreOfMass = transform;
        }

         rb.ResetCenterOfMass();

        SetGrabbableParent();
    }


    public void SetGrabbableParent()
    {
        foreach (GrabPoint point in grabbablePoints)
        {
            point.grabParent = this;
        }
    }

    

    public void AddToGrabPointList(GameObject target)
    {
        if(target.GetComponent<GrabPoint>() != null)
        {
            GrabPoint gPoint = target.GetComponent<GrabPoint>();
            gPoint.SetValues();
            gPoint.grabParent = this;
            grabbablePoints.Add(gPoint);
        }
    }

    public void SetCollisionObjectLayers(int layer)
    {
        foreach (GameObject colObj in collisionObjects)
        {
            colObj.layer = layer;
        }
    }

    //events

    public void RecieveGripValue(float value)
    {

    }

    public void RecieveTriggerValue(float value)
    {

    }

    public void TriggerDown()
    {
        OnTriggerDown.Invoke();
    }

    public void TriggerUp()
    {
        OnTriggerUp.Invoke();
    }

    public void MainButtonPressed()
    {
        OnMainButton.Invoke();
    }

    public void ObjectGrabbed()
    {
        OnObjectGrabbed.Invoke();
    }

    public void ObjectDropped()
    {
        OnObjectDropped.Invoke();

        //reset centre of mass
        rb.ResetCenterOfMass();
    }

    #region dual handed
    private VrHandInputValues primaryHolder;
    private VrHandInputValues secondaryHolder;
    public void SetHeldBy(VrHandInputValues primaryHeld, VrHandInputValues secondaryHeld, XRGrabManager g_Manager)
    {
        primaryHolder = primaryHeld;
        secondaryHolder = secondaryHeld;

        heldManager = g_Manager;
    }

    public VrHandInputValues PrimaryHeld()
    {
        return primaryHolder;
    }


    public VrHandInputValues SecondaryHeld()
    {
        return secondaryHolder;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        foreach (GrabPoint g in grabbablePoints)
        {
            if(g != null)
            {
                
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawSphere(g.transform.position, 0.01f);
            }
        }

        if(centreOfMass == null)
        {
            Handles.color = Color.green;
            
            Handles.Label(transform.position, "COM");
        }
        else
        {
            Handles.color = Color.yellow;
            Handles.Label(transform.position, "COM");
        }

        if(rb != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + rb.centerOfMass);
        }
    }

    public float CalculateWeightDistributionModifier()// we can use this to calculate the proper recoil will be range 0f to 1f
    {
        if(secondaryHolder != null)
        {
            return 0.5f;
        }

        return 1f;
    }
}
