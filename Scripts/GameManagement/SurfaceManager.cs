using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : MonoBehaviour
{
    #region singletonPatern
    public static SurfaceManager Instance { get; private set; }
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

    public enum SurfaceImpactType { blunt, bullet, stab, slice}

    [Header("Set surface values")]
    [SerializeField] private float mediumStrengthThresshold;
    [SerializeField] private float strongStrengthThresshold;

    [Header("Particle settings")]
    [SerializeField] private int numberOfInstancesToPool = 5;
    [SerializeField] private Transform impactParticleHolder;
    [SerializeField] private ImpactParticleCollection defaultImpactCollection;


    private void Start()
    {
        InitialiseImpactParticles(defaultImpactCollection);
    }

    public string GetSurfaceCollisionAudio(WorldSurface inputSurface, WorldSurface collisionSurface, float impulseValue)
    {
        string toReturn = "null";
        if (collisionSurface.overrideSurface)
        {

        }
        else
        {
            toReturn = GetSetSurfaceTypeAudio(collisionSurface.surfaceType, inputSurface.surfaceType, impulseValue);
        }
        return toReturn;
    }

    public string GetImpactCollisionAudioOfType(WorldSurface collisionSurface, SurfaceImpactType impactType)
    {
        string toReturn = "null";
        switch (impactType)
        {
            case SurfaceImpactType.blunt:
                break;
            case SurfaceImpactType.bullet:
                toReturn = collisionSurface.surfaceType.ToString() + "BulletImpact";
                break;
            case SurfaceImpactType.stab:
                break;
            case SurfaceImpactType.slice:
                break;
        }
        return toReturn;
    }

    //must follow strict naming conventions
    private string GetSetSurfaceTypeAudio(WorldSurface.SurfaceType inputType, WorldSurface.SurfaceType comparator, float strength)
    {
        string strengthKey = "low";
       // if(strength > mediumStrengthThresshold)
       // {
          //  if(strength < strongStrengthThresshold)
           // {
           //     strengthKey = "medium";
          //  }
          //  else
          //  {
             //   strengthKey = "high";
           // }
       // }

        string toReturn = inputType.ToString() + "HitBy" + comparator.ToString() + strengthKey;
        return toReturn;
    }

    //for creating particles at collision
    public void CreateSurfaceHitParticles(Vector3 point, Vector3 direction, WorldSurface hitSurface, SurfaceImpactType impactType)
    {
        //not yet implemented
        if (hitSurface.overrideSurface) return;

        //for default 
        SpawnParticleFromCollection(point, direction, defaultImpactCollection);

        switch (impactType)
        {
            case SurfaceImpactType.blunt:

                break;
            case SurfaceImpactType.bullet:

                break;
            case SurfaceImpactType.stab:

                break;
            case SurfaceImpactType.slice:

                break;
            default:
                break;
        }
    }

    private void SpawnParticleFromCollection(Vector3 point, Vector3 direction, ImpactParticleCollection particleCollection)
    {
        GameObject toSpawn = particleCollection.instanceQueue.Dequeue();
        toSpawn.transform.position = point;
        toSpawn.transform.rotation = Quaternion.LookRotation(direction);

        toSpawn.GetComponent<ParticleSystem>().Play();

        particleCollection.instanceQueue.Enqueue(toSpawn);
    }

    private void InitialiseImpactParticles(ImpactParticleCollection collecitonToInit)
    {
        collecitonToInit.instanceQueue = new Queue<GameObject>();

        for (int i = 0; i < numberOfInstancesToPool; i++)
        {
            foreach (GameObject g in collecitonToInit.spawnableParticles)
            {
                GameObject instance = Instantiate(g, impactParticleHolder);
                collecitonToInit.instanceQueue.Enqueue(instance);
            }
        }
    }
}

[System.Serializable]
public class ImpactParticleCollection
{
    public GameObject[] spawnableParticles;
    public Queue<GameObject> instanceQueue;
}
