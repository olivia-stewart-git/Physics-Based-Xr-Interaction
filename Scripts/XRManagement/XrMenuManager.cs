using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XrMenuManager : MonoBehaviour
{
    [Header("Menu settings")]
    private XrCalibration calibrationManager;
    public GameObject xrMenuObject;
    public Transform menuTargetLeftHand;
    public Transform menuTargetRightHand;
    public Transform playerHead;
    public float menuFollowSpeed = 5f;
    public float menuCloseDistance = 2f;


    [Header("Ui elements")]
    public Button calibrateHeightButton;

    // Start is called before the first frame update
    void Start()
    {
        calibrationManager = XrCalibration.Instance;


        calibrateHeightButton.onClick.AddListener(CallibratePlayerHeight); //subscriibes to button event+
        xrMenuObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (menuOpen)
        {
            //we track the positon
            TrackMenuPositon();
            if (ShouldCloseMenu())
            {
                CloseMenu();
            }
        }
    }

    bool ShouldCloseMenu()
    {
        float distanceBetween = Vector3.Distance(playerHead.transform.position, targetedMenuTransform.position);
        if(distanceBetween > menuCloseDistance)
        {
            return true;
        }
        return false;
    }

    void TrackMenuPositon()
    {
        xrMenuObject.transform.position = Vector3.Lerp(xrMenuObject.transform.position, targetedMenuTransform.position, Time.deltaTime * menuFollowSpeed);
    }

    private bool menuOpen = false;

    private Transform targetMenuFollower;
    public void OnMenuClick(XRControlManager.HandType handType)
    {
        if (menuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu(handType);
        }
    }

    void CloseMenu()
    {
        menuOpen = false;
        xrMenuObject.SetActive(false);
    }

    private Transform targetedMenuTransform;
    void OpenMenu(XRControlManager.HandType handType)
    {
        switch (handType)
        {
            case XRControlManager.HandType.right:
                targetedMenuTransform = menuTargetRightHand;
                break;
            case XRControlManager.HandType.left:
                targetedMenuTransform = menuTargetLeftHand;
                break;
        }
        menuOpen = true;
        xrMenuObject.SetActive(true);
        xrMenuObject.transform.position = targetedMenuTransform.position;
        xrMenuObject.transform.LookAt(playerHead);
    }

    public void CallibratePlayerHeight()
    {
        calibrationManager.CallibrateHeight();
    }
}
