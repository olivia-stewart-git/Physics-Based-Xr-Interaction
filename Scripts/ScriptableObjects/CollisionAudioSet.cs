using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CollisionAudioSet : ScriptableObject
{
    [Header("Collision sounds")]

    public AudioClip[] softSounds_metal;
    public AudioClip[] mediumSounds_metal;
    public AudioClip[] heavySounds_metal;
    [Space]
    public AudioClip[] softSounds_wood;
    public AudioClip[] mediumSounds_wood;
    public AudioClip[] heavySounds_wood;
    [Space]
    public AudioClip[] softSounds_concrete;
    public AudioClip[] mediumSounds_concrete;
    public AudioClip[] heavySounds_concrete;
    [Space]
    public AudioClip[] softSounds_dirt;
    public AudioClip[] mediumSounds_dirt;
    public AudioClip[] heavySounds_dirt;
    [Space]
    public AudioClip[] softSounds_water;
    public AudioClip[] mediumSounds_water;
    public AudioClip[] heavySounds_water;

    [Header("Slide sounds")]

    [Space]
    public AudioClip[] softSlideSounds_metal;
    public AudioClip[] hardSlideSounds_metal;
    [Space]
    public AudioClip[] softSlideSounds_wood;
    public AudioClip[] hardSlideSounds_wood;
    [Space]
    public AudioClip[] softSlideSounds_concrete;
    public AudioClip[] hardSlideSounds_concrete;
    [Space]
    public AudioClip[] softSlideSounds_dirt;
    public AudioClip[] hardSlideSounds_dirt;
    [Space]
    public AudioClip[] softSlideSounds_water;
    public AudioClip[] hardSlideSounds_water;
}
