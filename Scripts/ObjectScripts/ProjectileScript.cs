using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileScript : MonoBehaviour, IPooledObject
{
    private SurfaceManager s_Manager;
    private AudioManager a_Manager;

    [Header("Projectile settings")]
    public bool raycastCollision = true;

    [SerializeField]private SurfaceManager.SurfaceImpactType impactType;

    public GameObject visualRepresentor;
    public ParticleSystem collisionParticles;
    public TrailRenderer trailR;

    public float collisionBuffer = 0.05f;

    public UnityEvent onCollided = new UnityEvent();

    private bool initialised = false;
    private bool doInstantaneousCollision;

    private float useDamage;
    private float useRange;
    private float deactivateTime;
    private float useSpeed;

    private bool doUsePhysics;

    private Vector3 startPosition;
    private Vector3 moveDirection;

    private LayerMask collisionLayer;

    // Start is called before the first frame update
    public void InitialiseProjectile(float damage, Vector3 direction, bool usePhysics, float range, float lifetime, float projectileSpeed, bool instantaneousCollision, LayerMask collisionMask)
    {
        hasCollided = false;

        a_Manager = AudioManager.Instance;
        s_Manager = SurfaceManager.Instance;

        collisionLayer = collisionMask;

        deactivateTime = Time.time + lifetime;
        useDamage = damage;
        useSpeed = projectileSpeed;

        doInstantaneousCollision = instantaneousCollision;

        doUsePhysics = usePhysics;
        useRange = range;

        moveDirection = direction;

        startPosition = transform.position;

        if (doUsePhysics)
        {

        }

        initialised = true;
    }

    private Vector3 lastPosition;
    // Update is called once per frame
    void Update()
    {
        if(initialised)
        {
            if (raycastCollision && !doInstantaneousCollision)
            {
                Vector3 between = transform.position - lastPosition;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, between, out hit, between.magnitude + collisionBuffer, collisionLayer, QueryTriggerInteraction.Ignore))
                {
                    //collided
                    OnCollsion(hit.point, hit.normal, hit.transform.gameObject, between);
                    DisableProjectile();
                }
            }

            float distance = Vector3.Distance(transform.position, startPosition);
            if(distance > useRange)
            {
                DisableProjectile();
            }

            if(Time.time > deactivateTime)
            {
                DisableProjectile();

            }

            if (!doUsePhysics && !hasCollided) //move the projectile
            {
                transform.position = transform.position + (moveDirection * useSpeed * Time.deltaTime);
            }
        }


    }

    public void UpdateRange(float newRange)
    {
        useRange = newRange;
    }

    private bool hasCollided = false;
    public void OnCollsion(Vector3 point, Vector3 normal, GameObject hitObject, Vector3 direction)
    {
        hasCollided = true;
        //do other stuff
        if(hitObject.GetComponent<IDamageable>() != null)
        {
            hitObject.GetComponent<IDamageable>().OnTakeDamage(useDamage, point, direction, useDamage);
        }

        //do particles
        collisionParticles.transform.position = point;
        collisionParticles.transform.rotation = Quaternion.LookRotation(normal);
        collisionParticles.Play();

        if (hitObject.GetComponent<WorldSurface>() != null)
        {
            WorldSurface collisionSurface = hitObject.GetComponent<WorldSurface>();
            s_Manager.CreateSurfaceHitParticles(point, normal, collisionSurface, impactType);

            string audioTag = s_Manager.GetImpactCollisionAudioOfType(collisionSurface, impactType);
            a_Manager.PlaySound(audioTag, 1f, 1f, 0f, point, 0f);
        }

        onCollided.Invoke();
    }

    void DisableProjectile()
    {
        initialised = false;

        visualRepresentor.SetActive(false);
    }

    public void OnObjectSpawn()
    {
        initialised = false;

        visualRepresentor.SetActive(true);

        if(trailR != null)
        {
            trailR.Clear();
        }
    }
}
