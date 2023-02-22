using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class XrButton : MonoBehaviour
{
    [Header("XR Button settings")]
    public float angularAllowance = 0.01f;

    public float slowDownVelocity = 0.75f;
    public float resetCooldown = 0.1f;
    public float resetSpeed = 1f;
    public float distanceThreshold = 0.01f;

    public bool buttonActive = true;

    public UnityEvent OnButtonPressed = new UnityEvent();
    public UnityEvent OnButtonReleased = new UnityEvent();

    public Rigidbody mover;
    public Transform buttonStart;
    public Transform buttonEnd;

    [SerializeField]private bool atStart = true;
    [SerializeField]private bool atEnd = false;

    private AudioSource a_Source;

    public AudioClip pressedAudio;
    public AudioClip releasedAudio;
    private void Start()
    {
        a_Source = GetComponent<AudioSource>();


        if(OnButtonPressed == null)
        {
            OnButtonPressed = new UnityEvent();
        }
        if (OnButtonReleased == null)
        {
            OnButtonReleased = new UnityEvent();
        }
    }

    private float lastPress;

    private void Update()
    {
        //constratin the position to between the two pointss

        Vector3 between = buttonEnd.position - buttonStart.position;
        Vector3 fromStart = mover.position - buttonStart.position;

        float dot = Vector3.Dot(between, fromStart);
        float angle = Mathf.Acos(dot / (between.magnitude * fromStart.magnitude)) * Mathf.Rad2Deg;
        if(angle >= angularAllowance)
        {
            //project onto line and set position
            Vector3 targetPosition = buttonStart.position + ((dot / between.magnitude * between.magnitude) * between);
            mover.position = targetPosition;
        }


    }
    private void FixedUpdate()
    {

        float startDistance = Vector3.Distance(mover.position, buttonStart.position);
        float endDistance = Vector3.Distance(mover.position, buttonEnd.position);

        //perform projection

        float dot = Vector3.Dot(buttonStart.position - buttonEnd.position, mover.position - buttonEnd.position);
        if(dot <= 0f)
        {
            //mover.MovePosition(buttonEnd.position);
        }

        if(!atStart && Time.time > lastPress)
        {
              mover.velocity *= slowDownVelocity * startDistance / Vector3.Distance(buttonStart.position, buttonEnd.position);
  
            mover.AddForce((buttonStart.position - mover.position) * resetSpeed * Time.deltaTime, ForceMode.VelocityChange);
          //  mover.MovePosition(Vector3.Lerp(mover.position, buttonStart.position, resetSpeed * Time.deltaTime));
        }

        if(startDistance <= distanceThreshold)
        {
            if (!atStart)
            {
                ButtonReleased();
            }
            atStart = true;
            atEnd = false;
        }
        else
        {
            atStart = false;
            if(endDistance <= distanceThreshold)
            {
                if (!atEnd)
                {
                    lastPress = Time.time + resetCooldown;
                    ButtonPressed();
                }
                atEnd = true;
            }
            else
            {
                atEnd = false;
            }
        }
    }

    void ButtonReleased()
    {
        OnButtonReleased.Invoke();

        Debug.Log("button released " + gameObject);

        if(releasedAudio != null)
        {
            a_Source.PlayOneShot(releasedAudio);
        }
    }

    void ButtonPressed()
    {
        OnButtonPressed.Invoke();

        Debug.Log("button pressed " + gameObject);

        if (pressedAudio != null)
        {
            a_Source.PlayOneShot(pressedAudio);
        }
    }
}
