using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour, IPressable
{
    private AudioSource a_Source;

    public XrMenuManager menuManager;
    public XRControlManager.HandType handType;
    public AudioClip[] clickSounds;


    private void Start()
    {
        a_Source = GetComponent<AudioSource>();
    }

    public void PressObject()
    {
        Debug.Log("Pressed menu button ");
        menuManager.OnMenuClick(handType);

        a_Source.PlayOneShot(clickSounds[Random.Range(0, clickSounds.Length)]);
    }
}
