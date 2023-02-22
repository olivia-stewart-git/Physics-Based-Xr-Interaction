using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xr_IkManager : MonoBehaviour
{
    [Header("Body ik")]
    public Transform trackedHead;
    public Transform playerBase;

    public CapsuleCollider playerCollider;
    public float headColliderOffset = 0.1f;
    public float neckHeight = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCollider();
    }

    void UpdateCollider()
    {
        float height = Vector3.Distance(trackedHead.position, playerBase.position);
        Vector3 collidercentre = new Vector3(0f, (height + headColliderOffset) * 0.5f, 0f);
        playerCollider.center = collidercentre;
        playerCollider.height = height + headColliderOffset;
    }

    #region shoulders and neck
    void CalculateShoulderIk()
    {

    }

    Vector3 GetShoulderOffset()
    {
        return new Vector3(0f, neckHeight, 0f);
    }

    #endregion

    private void OnDrawGizmos()
    {
        if(trackedHead != null)
        {

        Gizmos.DrawLine(trackedHead.position, trackedHead.position - GetShoulderOffset());
        }
    }
}
