using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HeldObjectInputFemale : MonoBehaviour
{

    private XRGrabManager currentGrabManager;

    //the female input deos not require a grab object
    [Header("InputSettings")]
    [SerializeField] private Rigidbody objectRigidBody;


    public string allowedNameInput;
    public string endReachedAudioTag;
    public string ejectSoundTag;

    public float reachTolerance = 0.01f;
    public float inputDistanceTolerance = 0.03f;

    public Transform socket;
    public float angularTolerance = 5f;
    public float directionalAngleTolerance = 5f; //for the angle on the horizontal

    public bool inputOpen = true;
    public bool releaseGripOnReached = false;

    [Header("simple inputs")]
    public bool requireDepth = true;
    public bool destroyObjectOnEnd = false;

    public float inputDuration = 0.2f;

   [Tooltip("Ensure matches the start orientation of the input")]  public Transform positionalStart;
    [Tooltip("Ensure matches the end orientation of the input")] public Transform positionalEnd;

    [Header("Events")]
    public UnityEvent OnEndReached = new UnityEvent();
    public UnityEvent OnEndLost = new UnityEvent();

    private void Start()
    {
        distanceBetweenPoints = Vector3.Distance(positionalEnd.position, positionalStart.position);
    }

    private bool wasEnd;
    private void Update()
    {
        if(inputFilled && !heldFilled && !endReached && maleInput != null)
        {
            ConstrainSocketedMovement();
        }
        else
        {
            if(moveFromEnd == true)
            {
                ConstrainSocketedMovement();
            }
        }
        if (inputFilled)
        {
            bool fallout = CheckFallOut();
            if (maleInput != null)
            {
                if (fallout)
                {
                    didEject = false;
                }
                if (heldFilled)
                {
                    if (fallout && !didEject)
                    {
                        maleInput.g_Object.heldManager.EndModifiedOffset(); //end the offset movement
                        EjectObject();
                    }
                }
                else
                {
                    if (fallout && !didEject && !atStartOfInput)
                    {
                         EjectObject();
                    }
                }
            }
            else
            {
                if (moveFromEnd)
                {
                    if (fallout)
                    {
                        if (fallout && !didEject)
                        {
                            EjectObject();
                        }
                    }
                }
            }
        }

        if(Time.time > lastCheckAccess && didEject) //prevents for automatic re-input on ejection
        {
            didEject = false;
        }

        if (inputFilled)
        {
            CheckToPlaySlideSound();
        }

        if (atStartOfInput)
        {
            bool outStart = OutOfStartInput();
            if (outStart)
            {
                atStartOfInput = false;
            }
        }

        if(wasEnd && !endReached)
        {
            OnEndLost.Invoke();
        }
        wasEnd = endReached;
    }

    void CheckToPlaySlideSound()
    {

    }


    bool OutOfStartInput()
    {
        //we do dot
        Vector3 between = positionalEnd.position - positionalStart.position;
        Vector3 fromStart = maleInput.transform.position - positionalStart.position;

        float dot = Vector3.Dot(between, fromStart);
        if(dot > 0f && fromStart.magnitude > reachTolerance)
        {
            return true;
        }
        return false;
    }

    private float distanceBetweenPoints;
    void ConstrainSocketedMovement()
    {

        //constrain movement
        float distanceFromStart = Vector3.Distance(positionalStart.position, maleInput.transform.position);
        float lerpAmount = distanceFromStart / distanceBetweenPoints;

        Quaternion targRot = Quaternion.Lerp(positionalStart.rotation, positionalEnd.rotation, lerpAmount);

        Vector3 startToInput = maleInput.transform.position - positionalStart.position;
        Vector3 betweenBothVector = positionalEnd.position - positionalStart.position;

        Vector3 proj = Vector3.Project(startToInput, betweenBothVector.normalized);
        Vector3 targPosition = positionalStart.position + proj;

        maleInput.transform.position = targPosition;
        maleInput.transform.rotation = targRot;

        Debug.Log("Constraining movement");
    }

    public void OnInputGrabbed()
    {
        if(endReached && inputFilled)
        {
            ReleaseHeld();
        }
    }

    void ResetInput()
    {
        heldFilled = false;
        inputFilled = false;
        moveFromEnd = false;
        atStartOfInput = false;
    }
    bool CheckFallOut()
    {
        if (atStartOfInput) return false;
        if(heldFilled)
        {
           
           // return false;
        }

        //we do dot
        Vector3 between = positionalEnd.position - positionalStart.position;
        Vector3 fromStart = maleInput.transform.position - positionalStart.position;

        float dot = Vector3.Dot(between, fromStart);

      //  float dist = Vector3.Distance(positionalStart.position, maleInput.transform.position);
        if (dot <= 0f) //use larger than reach tolerance
        {
            return true;
        }

        return false;
    }

    private float lastCheckAccess;
    private float canCheckCooldown = 0.1f;
    public bool CanBeAccessed(HeldObjectInputMale toCheck)
    {
        lastCheckAccess = Time.time + canCheckCooldown;
        if (!inputOpen || didEject) return false;

        if (lastInput > Time.time) return false;

        if (allowedNameInput != toCheck.inputName) return false;

        if (inputFilled || heldFilled || endReached)
        {
            return false;
        }


        //check for matching vectores
        float distance = Vector3.Distance(socket.position, toCheck.inputOrientator.position);
        if (distance > inputDistanceTolerance) return false;

        float angleVert = Mathf.Acos(Vector3.Dot(socket.forward, toCheck.inputOrientator.forward)) * Mathf.Rad2Deg;
        if (angleVert> angularTolerance) return false;

        float angleHori = Mathf.Acos(Vector3.Dot(socket.right, toCheck.inputOrientator.right)) * Mathf.Rad2Deg;
        if (angleHori > directionalAngleTolerance) return false;

        return true;
    }

    private bool heldFilled = false;
    private bool inputFilled = false;

    private bool endReached = false;

    private HeldObjectInputMale maleInput;

    public HeldObjectInputMale CurrentInput()
    {
        return maleInput;
    }
    private bool atStartOfInput = false;
   public void OnStartInput(HeldObjectInputMale inputObject, XRGrabManager gManager)
    {
        currentGrabManager = gManager;

        atStartOfInput = true;

        maleInput = inputObject;
        maleInput.SetSocketed(true, this);

        maleInput.DeactivateCollider();

        inputFilled = true;
        heldFilled = true;

        Debug.Log("StartedSocketInput");

        if(requireDepth == false)
        {
            atStartOfInput = false;

            maleInput.transform.rotation = positionalEnd.rotation;
            if (inputDuration > 0.05f)
            {
                lerpCoroutine = StartCoroutine(LerpInput(maleInput.transform, positionalStart.position, positionalEnd.position, inputDuration));
            }
            else
            {
                EndReached();
            }
            
        }
    }

    Coroutine lerpCoroutine;
    IEnumerator LerpInput(Transform toMove,Vector3 start, Vector3 end, float duration)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            toMove.position = Vector3.Lerp(start, end, timeElapsed / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toMove.position = end;
        EndReached();
    }

    public void OnEndInput()
    {
        heldFilled = false;

        if (!endReached)
        {
            ReleaseHeld();
        }
    }

    private bool moveFromEnd = false;
    public bool CheckReached()
    {
        float dist = Vector3.Distance(positionalEnd.position, maleInput.transform.position);
        if (dist > reachTolerance)
        {
            moveFromEnd = false;
            return false;
        }
        if (maleInput == null || Time.time < lastCheckedEnd || moveFromEnd == true) return false;

       if (!endReached)
       {
            EndReached();           
       }

       
        return true;
    }


    private FixedJoint endFixedJoint;
    private void EndReached()
    {
        if(lerpCoroutine != null)
        {
            StopCoroutine(lerpCoroutine);
        }

        endReached = true;
        atStartOfInput = false;

        AudioManager.Instance.PlaySound(endReachedAudioTag, 1f, 1f, 0.1f, positionalEnd.position, 0f);

        Debug.Log("Reached end of " + gameObject);

        OnEndReached.Invoke();
      
        if (destroyObjectOnEnd)
        {
            currentGrabManager.CancelModifiedOffset();
            Destroy(maleInput.gameObject);
            ResetInput();
            return;
        }

        //add a fixed joint
        maleInput.rb.MovePosition(positionalEnd.position);
        maleInput.rb.MoveRotation(positionalEnd.rotation);

        FixedJoint fJoint = maleInput.gameObject.AddComponent<FixedJoint>();
        Vector3 anchorPos = maleInput.transform.InverseTransformPoint(positionalEnd.position);
        fJoint.connectedAnchor = anchorPos;
        fJoint.connectedMassScale = 0.1f;
        fJoint.connectedBody = objectRigidBody;
        endFixedJoint = fJoint;

        maleInput.OnEndReached();
    }

    public void ReleaseHeld()
    {
        if(inputFilled)
        {       
            moveFromEnd = true;
            endReached = false;
            
           // maleInput.rb.isKinematic = false;
            if(endFixedJoint != null)
            {
                Destroy(endFixedJoint);
            }
            maleInput.rb.velocity = Vector3.zero;
            //EjectObject();
            Debug.Log("Pulledfrom end " + gameObject);
        }
    }

    private float lastCheckedEnd;
    private float checkcooldown = 0.2f;

    private float inputCooldown = 0.2f;
    private float lastInput;

    private bool didEject = false;
    public void EjectObject()
    {
        if (maleInput != null)
        {
            //eject
            heldFilled = false;
        }
        if (endFixedJoint != null)
        {
            Destroy(endFixedJoint);
        }

        maleInput.SetSocketed(false, this);
        maleInput.ReactivateCollider();

        endReached = false;
        lastCheckedEnd = Time.time + checkcooldown;
        lastInput = Time.time + inputCooldown;

        didEject = true;

        AudioManager.Instance.PlaySound(ejectSoundTag, 1f, 1f, 0.1f, positionalEnd.position, 0f);

        Debug.Log("EjectedObjct " + gameObject);

        moveFromEnd = false;

        inputFilled = false;
    }

    public bool IsHeldInput()
    {
        if(inputFilled && heldFilled)
        {
            return true;
        }
        return false;
    }

    public bool EndFilled()
    {
        if(inputFilled && endReached)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (socket != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(socket.position, 0.01f);
            Gizmos.DrawLine(socket.position, socket.position + (socket.forward * 0.03f));
        }

        if(requireDepth && positionalStart != null && positionalEnd != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(positionalStart.position, positionalEnd.position);
        }
    }
}
