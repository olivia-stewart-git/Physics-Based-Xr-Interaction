using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GrabbableObject))]
public class Object_Gun : MonoBehaviour
{
    private AudioManager a_Manager;
    private ObjectPooler objPooler;
    private GrabbableObject grabObject;

    private float lastShot;

    [Header("Gun settings")]
    public bool infiniteAmmo = false;
        [Space]
    public float damage;
    public float bulletLifetime = 3f;
    public float rateOfFire = 0.1f;
    public float isShootingDuration = 0.1f;
    public bool automatic = false;
    public int projectilesPerShot = 1;
    public int ammunitionConsumedPerShot = 1;
    public float accuracy;
    [Space]
    public float range = 10f;
    public float projectileSpeed = 10f;
    public bool doRaycastProjectileCollision = true;
    [Space]
    public bool ejectRoundOnShot = true;
    public bool loadRoundAfterShot = true;
    public bool lockSlideOnlast = true;

    public LayerMask collisionMask;
    [Header("Shooting requirement")]
    public Rigidbody gunRigidBody;
    public Transform recoilReference;
    public Transform barrelEnd;
    public string projectileName = "defaultBullet";

    [Header("Recoil settings")]
    public float recoilMultipler = 1f;
    public float verticalRecoilAngle = 3f;
    public float horiztonalRecoilDrift;
    public float positionalRecoil = 10f;
   

    [Header("Av settings")]
    public string shootSoundTag;
    public string magazineEjectSound;
    [SerializeField] private string emptyShotSoundtag;
    public ParticleSystem shootParticles;
    public ParticleSystem shellEjectParticles;
    public ParticleSystem unDepletedRoundEjectParticles;
    [Space]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private string shootAnimationName;

    [Header("Ammunition settings")]
    public HeldObjectInputFemale magazineInput;
    public Object_AmmunitionFeed ammunitionFeed;
   [Tooltip("can also be a shotgun slide etc")] public XROffsetObject gunSlide; 

    [Header("Restrictions")]
    public GrabPoint[] blockFiringWhenGrabbed;
    public GrabPoint[] blockGrabWhenFiring;
    [Space]
 [Tooltip("these must be at start to shoot")]   public XROffsetObject[] offsetStartBlockers;
    [Tooltip("block their events while sshooting")] public XROffsetObject[] offsetShootEventBlockers;
    private void Start()
    {
        grabObject = GetComponent<GrabbableObject>();

        objPooler = ObjectPooler.Instance;
        a_Manager = AudioManager.Instance;       
    }

    private void Update()
    {
        //handle automatic firing
        if(triggerDepressed && automatic && Time.time > lastShot)
        {
            AttemptGunShot();
        }   

        if(isShooting && Time.time > lastDidShooting)
        {
            isShooting = false;
            SetBlockOfGrabs(false);
        }
    }
    private float lastDidShooting;
    void SetBlockOfGrabs(bool value)
    {
        if(blockGrabWhenFiring.Length > 0)
        {
            foreach (GrabPoint g in blockGrabWhenFiring)
            {
                g.SetGrabBlock(value);
            }
        }

        if(offsetShootEventBlockers.Length > 0)
        {
            foreach(XROffsetObject offset in offsetShootEventBlockers)
            {
                offset.SetEventBlock(value);
            }
        }
    }

    public void AttemptGunShot()
    {
        if (!RoundLoaded())
        {
            a_Manager.PlaySound(emptyShotSoundtag, 1f, 1f, 0f, transform.position, 0f);
            return;
        }

        if(Time.time > lastShot && AllowFiring())
        {
            //make projectiles{
                PerformShot();
            DoShootVisuals();
        }
    }
    
    void PlayEmptySound()
    {

    }

    bool AllowFiring()
    {
        if(blockFiringWhenGrabbed.Length > 0)
        {
            foreach (GrabPoint g in blockFiringWhenGrabbed)
            {
                if (g.beingHeld)
                {
                    return false;
                }
            }
        }

        if(offsetStartBlockers.Length > 0)
        {
            foreach (XROffsetObject offset in offsetStartBlockers)
            {
                if(offset.AtStart() == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool roundIsLoaded = false;
    bool RoundLoaded()
    {
        if (infiniteAmmo)
        {
            return true;
        }
        return roundIsLoaded;
    }

    private bool isShooting;
    private bool roundDepleted;
    
    void PerformShot()
    {
        //get shoot direction
        for (int i = 0; i < projectilesPerShot; i++)
        {

            Vector3 baseDirection = barrelEnd.forward;
            float useAngleX = Random.Range(-accuracy, accuracy);
            float useAngleY = Random.Range(-accuracy, accuracy);

            Quaternion upwardsValue = Quaternion.AngleAxis(useAngleY, Vector3.right);
            Quaternion horizontalValue = Quaternion.AngleAxis(useAngleX, Vector3.up);

            Vector3 shootDirection = (upwardsValue * horizontalValue) * baseDirection;

            //creat the projectile
            GameObject projectileInstance = objPooler.SpawnFromPool(projectileName, barrelEnd.position, Quaternion.LookRotation(shootDirection), null);
            ProjectileScript projectile = projectileInstance.GetComponent<ProjectileScript>();

            projectile.InitialiseProjectile(damage, shootDirection, false, range, bulletLifetime, projectileSpeed, doRaycastProjectileCollision, collisionMask);
            //add to the event


            if (doRaycastProjectileCollision)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, shootDirection, out hit, range, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    projectile.OnCollsion(hit.point, hit.normal, hit.transform.gameObject, shootDirection);
                    projectile.UpdateRange(Vector3.Distance(barrelEnd.position, hit.point));
                }
            }

        }
        roundDepleted = true;
        isShooting = true;
        SetBlockOfGrabs(true);

        //play shoot animation
        if(gunAnimator != null)
        {
            gunAnimator.Play(shootAnimationName, 0, 0f); 
        }

        //update one time values
        lastShot = Time.time + rateOfFire;
        lastDidShooting = Time.time + isShootingDuration;

        if (ejectRoundOnShot)
        {
            AttemptEjectRound();
        }

        if (loadRoundAfterShot)
        {
            AttemptLoadRound();
        }

        if(gunSlide != null && lockSlideOnlast)
        {
            CheckToLockSlide();
        }

        CalculateRecoilVector();
    }    
    void DoShootVisuals()
    {
        if(shootParticles != null)
        {
            shootParticles.Play();
        }

        if(shootSoundTag != null)
        {
            a_Manager.PlaySound(shootSoundTag, 1f, 1f, 0.1f, barrelEnd.position, 0f);
        }
    }

    private bool triggerDepressed = false;
    public void GunTriggerDown()
    {
        triggerDepressed = true;
            AttemptGunShot();
        
    }

    public void GunTriggerUp()
    {
        triggerDepressed = false;
    }

    public void EjectMagazine()
    {
        if (isShooting) return;
        Debug.Log("attempt eject magazine ");
        if(magazineInput != null)
        {
            if (!magazineInput.IsHeldInput() && magazineInput.EndFilled()) //check the play is not holding the magazine
            {
                a_Manager.PlaySound(magazineEjectSound, 1f, 1f, 0.1f, magazineInput.positionalEnd.position, 0f);
                magazineInput.ReleaseHeld();
            }
        }

        if(ammunitionFeed != null)
        {
            ammunitionFeed.UnloadAmmunition();
        }
    }

    public void AttemptEjectRound()
    {
        Debug.Log("attemp eject round");
        if (RoundLoaded())
        {
            //play particles
            if (roundDepleted)
            {
                if(unDepletedRoundEjectParticles != null)
                {
                    unDepletedRoundEjectParticles.Play();
                }
            }
            else
            {
                if (shellEjectParticles != null)
                {
                    shellEjectParticles.Play();
                }
            }

            roundDepleted = false;

            //play sound

            //we eject round

            roundIsLoaded = false;
        }
    }

    void AttemptLoadRound()
    {
        if (roundIsLoaded) return;
        if (infiniteAmmo)
        {
            roundIsLoaded = true;
            return;
        }
        if(ammunitionFeed != null)
        {
            if (ammunitionFeed.TakeAmmunition(ammunitionConsumedPerShot))
            {
                roundIsLoaded = true;
                Debug.Log("loaded round ");
            }
            roundDepleted = false;
        }
    }

    public void AttemptUnlockSlide()
    {
        Debug.Log("Did overshooot wow");
        if(gunSlide != null)
        {
            if (gunSlide.IsLocked())
            {
                gunSlide.UnlockMove();
            }
        }

        AttemptLoadRound();
    }

    public void LockSlideBack()
    {
        Debug.Log("Locked slide " + gameObject);
        if(gunSlide != null)
        {
            gunSlide.LockToBack();
        }
    } 

    void CheckToLockSlide()
    {
        if (infiniteAmmo) return;
        if(ammunitionFeed != null)
        {
            if (!ammunitionFeed.HasAmmunition())
            {
                LockSlideBack();
            }
        }
    }

    public void CalculateRecoilVector()
    {
        float recoilMultipler = grabObject.CalculateWeightDistributionModifier();

        //this will be the new direction the gun faces
        Quaternion right = Quaternion.AngleAxis(Random.Range(-horiztonalRecoilDrift * recoilMultipler, horiztonalRecoilDrift * recoilMultipler), recoilReference.up);
        Quaternion up = Quaternion.AngleAxis(verticalRecoilAngle, recoilReference.right);
        Vector3 recoilDirection = (right * up) * recoilReference.forward;
        Quaternion converted = Quaternion.LookRotation(recoilDirection, recoilReference.up);
        Quaternion rdifference = recoilReference.rotation * Quaternion.Inverse(converted);
        Quaternion newTarg = rdifference * gunRigidBody.rotation;

        Quaternion difference = newTarg * Quaternion.Inverse(gunRigidBody.rotation);
        difference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f)
        {
            angleInDegrees -= 360f;
        }

        Vector3 useTorque = (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.deltaTime;
        
        if(!float.IsNaN(useTorque.x) && !float.IsInfinity(useTorque.x))
        {
            //add our recoil
            gunRigidBody.AddTorque(useTorque * recoilMultipler, ForceMode.Impulse);
            Debug.Log("Adding recoil");
        }

        //do positonal recoil

        Vector3 backDir = -recoilReference.transform.forward;
        gunRigidBody.AddForce(backDir * positionalRecoil, ForceMode.Impulse);
    }

    public void ChooseSlideReleaseOrMag() //for if you want to either release the mag or slide
    {
        if(gunSlide != null)
        {
            if (gunSlide.IsLocked())
            {
                AttemptUnlockSlide();
            }
            else
            {
                EjectMagazine();
            }
        }
    }
}
