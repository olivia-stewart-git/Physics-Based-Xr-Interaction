using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class XRRayInteractionManager : MonoBehaviour
{
    private AudioManager a_Manager;

    //for ussing rays to interact with force pull and menus
    [Header("interaction settings")]
    [SerializeField] private GraphicRaycaster g_Raycaster;
    [SerializeField] private Camera gameCamera;
    PointerEventData m_PointerEventData;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] RectTransform canvasRect;

    [Space]
    public LayerMask fingerInteractionLayer;
    public float uiInteractionAngularTolerance = 5f;
    public float fingerRayDistance = 0.1f;
    public float fingerRayActivationDistance = 0.05f;

    [Header("Ui interaction settings")]
    public string buttonClickSoundTag;

    private void Start()
    {
        a_Manager = AudioManager.Instance;
    }

    public void UpdateRayInteraction(VrHandInputValues targetHand)
    {
        if (DoFingerInteraction(targetHand) == true)
        {
            UpdateFingerInteraction(targetHand);
        }
        else
        {
            //reset ui clicks
     
        }
    }

    bool DoFingerInteraction(VrHandInputValues targetHand)
    {
        if (targetHand.holdingItem)
        {
            return false;
        }

        if(targetHand.gripValue < 0.9f)
        {
            return false;
        }

        if(targetHand.triggerValue > 0.1f)
        {
            return false;
        }

        return true;
    }


    private bool didLeftUiClick = false;
    private bool didRightUiClick = false;

    private bool didFingerPressleft = false;
    private bool didFingerPressright = false;

    public void UpdateFingerInteraction(VrHandInputValues targetHand)
    {
        RaycastHit hit; //this is for world interactions
        Transform fromObject = targetHand.physicsHand.fingerInteractionPoint;
        if (Physics.Raycast(fromObject.position, fromObject.forward, out hit, fingerRayDistance, fingerInteractionLayer))
        {
            if (hit.transform.gameObject.GetComponent<FingerInteractable>() != null)
            {
                Debug.DrawLine(fromObject.position, hit.point, Color.red);
                //we hit
                float hitDist = Vector3.Distance(fromObject.position, hit.point);           
               
                if (hitDist < fingerRayActivationDistance)
                {
                    //check to perform click
                    switch (targetHand.handType)
                    {
                        case XRControlManager.HandType.right:
                            if (didFingerPressright == false)
                            {
                                CalculateFingerPress(hit.transform.gameObject);
                            }
                            didFingerPressright = true;
                            break;
                        case XRControlManager.HandType.left:
                            if (didFingerPressleft == false)
                            {
                                CalculateFingerPress(hit.transform.gameObject);
                            }
                            didFingerPressleft = true;
                            break;
                    }
                    return;
                }
                else
                {

                    if (hitDist > fingerRayActivationDistance + 0.03f)
                    {
                        switch (targetHand.handType)
                        {
                            case XRControlManager.HandType.right:
                                didFingerPressright = false;
                                break;
                            case XRControlManager.HandType.left:
                                didFingerPressleft = false;
                                break;
                        }
                    }
                }
            }
        }
        else
        {
            switch (targetHand.handType)
            {
                case XRControlManager.HandType.right:
                    didFingerPressright = false;
                    break;
                case XRControlManager.HandType.left:
                    didFingerPressleft = false;
                    break;
            }
        }

        #region ui events
        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(eventSystem);
        //Set the Pointer Event Position to that of the game object
        m_PointerEventData.position = gameCamera.WorldToScreenPoint(fromObject.transform.position);

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        g_Raycaster.Raycast(m_PointerEventData, results);

        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.GetComponent<InteractableUi>() != null)
                {
                    Vector3 between = results[i].gameObject.transform.position - fromObject.position;
                    float dot = Vector3.Dot(fromObject.forward, results[i].gameObject.transform.forward);
                    Vector3 projection = Vector3.Project(between, fromObject.forward);
                    float distanceBetween = projection.magnitude; //we project so that location is irrevelant
                    float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

                    float secondDot = Vector3.Dot(fromObject.forward, projection - fromObject.position);

               //     Debug.Log("second dot " + secondDot);

                    if (angle < uiInteractionAngularTolerance && distanceBetween < fingerRayDistance)
                    {
                        //Debug.Log("available ui object " + results[i].gameObject);
                        if (distanceBetween < fingerRayActivationDistance)
                        {
                            switch (targetHand.handType)
                            {
                                case XRControlManager.HandType.right:
                                    if (didRightUiClick == false)
                                    {
                                        CalculateUiClick(results[i].gameObject);
                                    }
                                    didRightUiClick = true;
                                    break;
                                case XRControlManager.HandType.left:
                                    if (didLeftUiClick == false)
                                    {
                                        CalculateUiClick(results[i].gameObject);
                                    }
                                    didLeftUiClick = true;
                                    break;
                            }
                            return;
                        }
                        else
                        {
                            if (distanceBetween > fingerRayDistance + 0.03f) //adds slight extra to prevent hand jitters causing constant clicks
                            {
                                switch (targetHand.handType)
                                {
                                    case XRControlManager.HandType.right:
                                        didRightUiClick = false;
                                        break;
                                    case XRControlManager.HandType.left:
                                        didLeftUiClick = false;
                                        break;
                                }
                            }
                            
                        }
                    }
                }
            }
            //Debug.Log("Hit " + results[0].gameObject.name);
        }
        else
        {
            Debug.Log("No results");
            switch (targetHand.handType)
            {
                case XRControlManager.HandType.right:
                    didRightUiClick = false;
                    break;
                case XRControlManager.HandType.left:
                    didLeftUiClick = false;
                    break;
            }
        }
        #endregion  

    }
    void CalculateUiClick(GameObject targetObject)
    {
        //we perform ui evenets
        if (targetObject.GetComponent<Button>() != null)
        {
            Button useButton = targetObject.GetComponent<Button>();
            useButton.onClick.Invoke();
            a_Manager.PlaySound(buttonClickSoundTag, 1f, 1f, 0f, targetObject.transform.position, 0f);
        }
        Debug.Log("Ui click");
    }

    void CalculateFingerPress(GameObject targetObject)
    {
        Debug.Log("Finger pressed");
        if(targetObject.GetComponent<IPressable>() != null)
        {
            targetObject.GetComponent<IPressable>().PressObject();
        }
    }

}
