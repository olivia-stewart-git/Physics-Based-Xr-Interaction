using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class XROffsetObject : MonoBehaviour
{
    private AudioManager a_Manger;

    [Header("values")]
   
    public GrabPoint g_Point;
    [Tooltip("So this will always be a secondary grab")]  public bool dePrioritiseGrab = true;
    [Space]
    public Transform start;
    public Transform end;

    public Transform toMove;

    [Tooltip("For if the rotation of the held item is affected by movement")]   public bool doOffsetDualDirection = false;
    [Space]
    public bool lockedReturn = false;
    public bool returnToStartByForce = false;
    public float returnToStartSpeed = 1f;
    [Space]
    public bool doOverShoot = false;
    public bool applyOverShootOnRelease = true;
    public bool releaseOnOvershoot = false;
    [Space]   
    private Vector3 lastToMovePosition = Vector3.zero;

    private bool atEndLast = false;
    private bool atStartLast = true;

    private float changeAmount;
    [Range(0f, 1f)]   public float currentOffsetValue = 0f; //this iss the current offsetvalue
    public float changeThresshold = 0.5f;
    public float endThreshold = 0.1f;
    public float offshootThresshold = 0.03f;
    [Space]
    public bool doSteppedMovement;
    public int movementSteps = 10;
    [Space]
    [Range(0f, 1f)] public float moveResistance = 1f; //decreasing this slows the movement down

    [Header("Rotation settings")]
    [Tooltip("uses rotation value aswell, ie for dials etc")] public bool requireRotationmatching = false;
    [Tooltip("for if you only want to calculate the rotation")] public bool excludePositionCalculations = false;
    [Tooltip("angle for reaching start and end")] public float anglularThreshold = 1f;

    [Header("Audio visual")]
    public string endAudioTag;
    public string startAudioTag;
    public string leaveStartAudioTag;
    public string offshootAudioTag;


    [Header("Events")]
    public UnityEvent OnHitStart;
    public UnityEvent OnLeaveStart;
    public UnityEvent OnHitEnd;
    public UnityEvent OnLeaveEnd;
    public UnityEvent OnOverShoot;
    public UnityEvent OnChangeThresholdbreached;

    public bool doOffssetEvents = true;



    private void Start()
    {
        
        a_Manger = AudioManager.Instance;
        AtStart();
    }

    public void SetEventBlock(bool value)
    {
        doOffssetEvents = value;
    }

    private void Update()
    {
        if (doOffssetEvents)
        {
            changeAmount = Vector3.Magnitude(lastToMovePosition - toMove.position);
            lastToMovePosition = toMove.position;

            if (changeAmount > changeThresshold)
            {
                OnChangeThresholdbreached.Invoke();
            }

            AtStart();
            AtEnd();
        }

        if (g_Point != null)
        {
            if (g_Point.beingHeld == false && returnToStartByForce)
            {
                //move the object back to start with force
                if (!isReturning && !lockedReturn && !atStartLast)
                {
                    if (returnToStartCoroutine != null)
                    {
                        StopCoroutine(returnToStartCoroutine);
                        isReturning = false;
                    }
                    float targetDuration = returnToStartSpeed * (Vector3.Distance(toMove.position, start.position) / Vector3.Distance(start.position, end.position));
                    returnToStartCoroutine = StartCoroutine(ReturnToStart(targetDuration));
                }
            }


            if (g_Point.beingHeld)
            {
                wasHeld = true;
                //move the object back to start with force
                if (returnToStartCoroutine != null)
                {
                    StopCoroutine(returnToStartCoroutine);
                    isReturning = false;
                }
            }
            else
            {
                if (wasHeld)
                {
                    AtStart();
                    AtEnd();
                    if (willDoOffshoot)
                    {
                        OverShoot();
                    }
                }
                wasHeld = false;
            }
        }
        if (lockedReturn && returnToStartByForce)
        {
            if (returnToStartCoroutine != null)
            {
                StopCoroutine(returnToStartCoroutine);
                isReturning = false;
            }
        }
    }


    private bool wasHeld = false;
    public bool AtStart()
    {
        float startDistance = Vector3.Distance(toMove.position, start.position);

        if (startDistance < endThreshold)
        {
            if (atStartLast == false)
            {
                OnStart();
            }
            atStartLast = true;
        }
        else
        {
            if (atStartLast)
            {
                if (atStartLast)
                {
                    OnLeftStart();
                }
            }
            atStartLast = false;
        }
        return atStartLast;
    }

    public bool AtEnd()
    {
        float endDistance = Vector3.Distance(toMove.position, end.position);

        if (endDistance < endThreshold)
        {
            if (atEndLast == false)
            {
                OnEnd();
            }
            atEndLast = true;
        }
        else
        {
            if (atEndLast)
            {
                LeaveEnd();
            }
            atEndLast = false;

        }

       
    
        return atEndLast;
    }

    private bool isReturning;
    Coroutine returnToStartCoroutine;
    IEnumerator ReturnToStart(float duration)
    {
        isReturning = true;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            toMove.position = Vector3.Lerp(end.position, start.position, timeElapsed / duration);

            //calculate rotation

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        toMove.position = start.position;
        
        OnStart();
        atStartLast = true;
        isReturning = false;
    }

    public Vector3 UpdatePosition(float inputValue)
    {    
        Vector3 targ = Vector3.Lerp(start.position, end.position, inputValue);
        return targ;
    }

    

    private void OnDrawGizmosSelected()
    {
        if (start != null && end != null)
        {
            Debug.DrawLine(start.position, end.position, Color.red);
           // Gizmos.DrawWireSphere(toMove.position, 0.02f);
        }
    }

    public void OnValueChanged()
    {

    }


    private bool willDoOffshoot = false;
    
    public bool WillOvershoot()
    {
        return willDoOffshoot;
    }

    public void OverShoot()
    {
        if (g_Point.beingHeld && applyOverShootOnRelease)
        {
            willDoOffshoot = true;
            return;
        }
        OnOverShoot.Invoke();
        a_Manger.PlaySound(offshootAudioTag, 1f, 1f, 0f, end.position, 0f);
        willDoOffshoot = false;
    }

    void OnEnd()
    {
        OnHitEnd.Invoke();
        Debug.Log("Hit end " + gameObject);
        a_Manger.PlaySound(endAudioTag, 1f, 1f, 0f, end.position, 0f);
    }

    void LeaveEnd()
    {
        OnLeaveEnd.Invoke();
    }
    void OnStart()
    {
        OnHitStart.Invoke();
        Debug.Log("Hit start " + gameObject);
        a_Manger.PlaySound(startAudioTag, 1f, 1f, 0f, start.position, 0f);
    }

    void OnLeftStart()
    {
        OnLeaveStart.Invoke();
        a_Manger.PlaySound(leaveStartAudioTag, 1f, 1f, 0f, start.position, 0f);
    }

    public void LockToBack()
    {
        lockedReturn = true;
        if (!atEndLast)
        {
            toMove.position = end.position;
        }

        AtStart(); //just update these
        AtEnd();
    }

    public void UnlockMove()
    {
        lockedReturn = false;
        AtStart(); //just update these
        AtEnd();
    }

    public bool IsLocked()
    {
        return lockedReturn;
    }

}
