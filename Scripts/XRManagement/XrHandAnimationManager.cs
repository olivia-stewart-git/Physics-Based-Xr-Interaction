using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XrHandAnimationManager : MonoBehaviour
{
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    public string defaultStateName;

    private void Start()
    {
        SwitchHandPose("", XRControlManager.HandType.right);
        SwitchHandPose("", XRControlManager.HandType.left);
    }

    public void RecieveHandAnimationValues(VrHandInputValues targetHand)
    {
        Animator targetAnimator = leftHandAnimator;
        if(targetHand.handType == XRControlManager.HandType.right)
        {
            targetAnimator = rightHandAnimator;
        }

        targetAnimator.SetFloat("Grip", targetHand.gripValue);
        targetAnimator.SetFloat("Trigger", targetHand.triggerValue);
    }

    public void SwitchHandPose(string key, XRControlManager.HandType handType)
    {
        if (key != ""  && key != "default") 
        {
            switch (handType)
            {
                case XRControlManager.HandType.right:
                    int stateId = Animator.StringToHash(key);
                    bool hasState = rightHandAnimator.HasState(0, stateId);
                    if (hasState)
                    {
                        rightHandAnimator.Play(key, 0, 0f);
                    }
                    break;
                case XRControlManager.HandType.left:
                    int stateIdL = Animator.StringToHash(key);
                    bool hasStateL = leftHandAnimator.HasState(0, stateIdL);
                    if (hasStateL)
                    {
                        leftHandAnimator.Play(key, 0, 0f);
                    }
                    break;
            }
        }
        else
        {
            switch (handType)
            {
                case XRControlManager.HandType.right:
                    rightHandAnimator.Play(defaultStateName, 0, 0f);
                    break;
                case XRControlManager.HandType.left:
                    leftHandAnimator.Play(defaultStateName, 0, 0f);
                    break;
            }
        }
    }
}
