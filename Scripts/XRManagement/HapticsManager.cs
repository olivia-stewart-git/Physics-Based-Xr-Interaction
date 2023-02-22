using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticsManager : MonoBehaviour
{
    #region singletonPatern
    public static HapticsManager Instance { get; private set; }
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
    [Header("haptic setttings")]
    public bool useHaptics = true;
    [Range(0f, 1f)]  public float hapticMultiplier = 1f;

    public ActionBasedController leftController;
    public ActionBasedController rightController;

    public void SendHapticCommand(float amplitude, float duration, XRControlManager.HandType targetHand)
    {
        if (!useHaptics) return;
        switch (targetHand)
        {
            case XRControlManager.HandType.right:
                rightController.SendHapticImpulse(amplitude * hapticMultiplier, duration);
                break;
            case XRControlManager.HandType.left:
                leftController.SendHapticImpulse(amplitude * hapticMultiplier, duration);
                break;
        }
    }
}
