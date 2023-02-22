using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WorldSurface))]
public class PhysicsObject : MonoBehaviour
{
    private WorldSurface thisSurface;
    private SurfaceManager s_Manager;
    private AudioManager a_Manager;
    private Rigidbody rb;

    [Header("Object settings")]
    [Tooltip("In kilograms")] public float mass = 1f;


    // Start is called before the first frame update
    void Start()
    {
        s_Manager = SurfaceManager.Instance;
        a_Manager = AudioManager.Instance;

        lastPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        thisSurface = GetComponent<WorldSurface>();
    }


    private float bodyAcceleration;
    private Vector3 lastPosition;
    private float lastDistanceMoved;


    private Vector3 curVelocity;
    private Vector3 lastVelocity;

    private void FixedUpdate()
    {
        //calculate accelration
        Vector3 curPosition = transform.position;
        float distance = Vector3.Distance(curPosition, lastPosition);
        float distMove = distance * Time.deltaTime;
        bodyAcceleration = lastDistanceMoved - distMove;
        lastDistanceMoved = distMove;

        lastPosition = transform.position;
       
    }

    private void Update()
    {
        curVelocity = rb.velocity;

        //calculate instantaneous accelaration
        
        lastVelocity = curVelocity;
    }

    float ObjectForce()
    {
        //f = ma       
        float raw = mass * bodyAcceleration;  
        return  Mathf.Abs(raw);
    }

    private float curForce = 0f;
    #region collisison
    private void OnCollisionEnter(Collision collision)
    {
        //do impact

        Vector3 closestPoint = collision.collider.ClosestPoint(rb.position);

        ContactPoint closest = collision.contacts[0];

        float smallestDist = Vector3.Distance(closest.point, closestPoint);

        for (int i = 0; i < collision.contactCount; i++) //get the closest point
        {
            float checkDistance = Vector3.Distance(collision.contacts[i].point, closestPoint);
            if(checkDistance < smallestDist)
            {
                smallestDist = checkDistance;
                closest = collision.contacts[i];
            }
        }

        DoCollisionImpact(closest.point, closest.normal, collision.impulse, collision.gameObject);

    }
    //for later
    //If you just want a measurement of how strong the hit was (like, for example for damage calculations),
    //the dot product of collision normal and collision velocity (ie the velocity of the two bodies relative to each other),
    //times the mass of the other collider should give you useful values.
    void DoCollisionImpact(Vector3 impactPoint, Vector3 impactNormal, Vector3 impulse, GameObject hitObject)
    {
        //calculate the total force
        float curForce = ObjectForce();
        Vector3 curVel = rb.velocity;

        Vector3 collisionForce = impulse / Time.fixedDeltaTime;

        float forceRaw = collisionForce.magnitude;

        if (collisionForce.magnitude > 20f)
        {
            Debug.Log(gameObject + " Collided with " + hitObject + " force " + collisionForce.magnitude);
            //handle audio
            if (hitObject.GetComponent<WorldSurface>() != null)
            {
                WorldSurface hitSurface = hitObject.GetComponent<WorldSurface>();
                string collisionAudioTag = s_Manager.GetSurfaceCollisionAudio(thisSurface, hitSurface, forceRaw);
                a_Manager.PlaySound(collisionAudioTag, 1f, 1f, 0f, impactPoint, 0f);
            }
        }

        //updated collision impulse
    }

    private void OnCollisionExit(Collision collision)
    {
        
    }

    private void OnCollisionStay(Collision collision)
    {
        
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Handles.Label(transform.position, text: "objectForce " + ObjectForce());
    }
}
