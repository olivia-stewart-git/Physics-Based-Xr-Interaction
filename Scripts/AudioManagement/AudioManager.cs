using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region singletonPatern
    public static AudioManager Instance { get; private set;  }
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

    private ObjectPooler objPooler;

    [Header("Game sounds settings")]
    public PlayableSound[] registeredSounds;

    private Dictionary<string, PlayableSound> playableSounds;

    private void Start()
    {
        objPooler = ObjectPooler.Instance;

        //initialise the sounds
        LoadSounds();
    }

    void LoadSounds()
    {
        playableSounds = new Dictionary<string, PlayableSound>();
        foreach (PlayableSound s in registeredSounds)
        {
            playableSounds.Add(s.nameKey, s);
        }
    }

    public void PlaySound(string tag, float range, float volumeScale, float pitchShift, Vector3 position, float duration)
    {
        if (tag == "") return;
        if (playableSounds.ContainsKey(tag))
        {
            float usePitch = Random.Range(1f - pitchShift, 1f + pitchShift);
            //get our audio to use
            GameObject audioObject = objPooler.SpawnFromPool("AudioObject", position, Quaternion.identity.normalized, null);

            AudioSource a_Source = audioObject.GetComponent<AudioSource>();

            a_Source.pitch = usePitch;

            AudioClip useClip = playableSounds[tag].clips[Random.Range(0, playableSounds[tag].clips.Length)];
            if (duration <= 0.01f)
            {
                a_Source.PlayOneShot(useClip, volumeScale);
            }
            else
            {
                //we play a repeating one
            }
        }
    }
}

[System.Serializable]
public class PlayableSound
{
    public string nameKey;

    public AudioClip[] clips;
}
