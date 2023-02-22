using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SurfaceMap : ScriptableObject
{
    [Header("Surface collision settings")]
    [Range(0f, 1f)] public float surfaceHardness = 1f;

    [Header("Surface audio setttings")] //these are the sounds we play when colliding with something
    public string collisionSoundLightTag;
    public string collisionSoundMediumTag;
    public string collisionSoundHeavyTag;

    [Header("Collision effects")]
    public GameObject[] lightCollisionVisualEffect;
    public GameObject[] mediumCollisionVisualEffect;
    public GameObject[] heavyCollisionVisualEffect;
}
