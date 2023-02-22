using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(GrabbableObject))]
[RequireComponent(typeof(Rigidbody))]
public class HeldObjectInputMale : MonoBehaviour
{
    [HideInInspector]public Rigidbody rb;
    [HideInInspector]  public GrabbableObject g_Object;


    [Header("Input settings")]
    public LayerMask checkLayer;
    public string inputName = "default";

    public Transform inputOrientator;
    public float inputCheckSize = 0.03f;

    public GameObject dynamicColliderHolder;
    [Header("Events")]
    public UnityEvent OnReachEnd = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        g_Object = GetComponent<GrabbableObject>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private bool washeld;
    void Update()
    {
        if (g_Object.isBeingHeld)
        {
            if (!washeld)
            {
                OnObjectGrabbed();
            }
            washeld = true;
            if (!isInSocket)
            {
                CheckForSocket();
            }
            else
            {
                if (femaleInput != null)
                {
                    TryNotifyStatus();
                }
            }
        }
        else
        {
            washeld = false;
        }
    }


    void OnObjectGrabbed()
    {
        if (isInSocket)
        {
            femaleInput.OnInputGrabbed();
        }
    }

    void TryNotifyStatus()
    {
        bool shouldDo = g_Object.heldManager.ModifiedInputStartRequest();
        if (shouldDo)
        {
            g_Object.heldManager.StartModifiedOffset(femaleInput, this, g_Object.PrimaryHeld());
        }
    }

    void CheckForSocket()
    {
        Debug.DrawLine(inputOrientator.position, inputOrientator.position + (inputOrientator.forward * 0.01f), Color.red);

        Collider[] foundTransmitters = Physics.OverlapSphere(inputOrientator.position, inputCheckSize, checkLayer);

        if (foundTransmitters == null || foundTransmitters.Length == 0)
        {
            return;
        }

        Debug.Log("Found socket  " + foundTransmitters[0].gameObject);

        Debug.DrawLine(inputOrientator.position, foundTransmitters[0].transform.position, Color.green);

        HeldObjectInputFemale femaleUse = foundTransmitters[0].gameObject.GetComponent<HeldObjectInputTransmitter>().femaleInput; //gets the firsts input

        bool checkSocket = femaleUse.CanBeAccessed(this);

        if (checkSocket)
        {
            g_Object.heldManager.StartModifiedOffset(femaleUse, this, g_Object.PrimaryHeld());
        }
    }

    private HeldObjectInputFemale femaleInput;

    public HeldObjectInputFemale FemaleInput()
    {
        return femaleInput;
    }

    private bool isInSocket = false;
    public void SetSocketed(bool value, HeldObjectInputFemale toUseFemaleInput)
    {
        isInSocket = value;
        femaleInput = toUseFemaleInput;
    }

    public void DeactivateCollider()
    {
        dynamicColliderHolder.SetActive(false);
    }

    public void ReactivateCollider()
    {
        dynamicColliderHolder.SetActive(true);
    }

    public bool IsSocketed()
    {
        return isInSocket;
    }
    private void OnDrawGizmosSelected()
    {
        if (inputOrientator != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(inputOrientator.position, 0.01f);
            Gizmos.DrawLine(inputOrientator.position, inputOrientator.position + (inputOrientator.forward * 0.03f));
        }
    }

    public void OnEndReached()
    {
        OnReachEnd.Invoke();
    }
}
