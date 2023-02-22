using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharpDamager : MonoBehaviour
{
    [Header("Damage Settings")]
    public Transform sharpStart;
    public Transform sharpEnd;
    public Transform bladeOrientator;

    [Header("Collision settings")]
   [Range(0f,1f)]  public float colliderFill;

    public BoxCollider sharpCollider;
    public float colliderHeight;
    public float colliderWidth;

    [Header("Raycast settings")]
    public int numberOfRaycasts = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetCollider(colliderFill);   
    }

    void SetCollider(float fill)
    {
        Vector3 centre = Vector3.Lerp(sharpStart.localPosition, sharpEnd.localPosition, colliderFill * 0.5f);
        sharpCollider.center = centre;
        Vector3 extents = new Vector3(colliderWidth / 2f, colliderHeight / 2f, Vector3.Distance(sharpStart.position, sharpEnd.position) * (colliderFill / 1f));
        sharpCollider.size = extents;
    }

    private void OnDrawGizmosSelected()
    {
        
        if (sharpEnd && sharpStart != null)
        {
            Gizmos.DrawLine(sharpStart.position, sharpEnd.position);

            if (sharpCollider != null)
            {
                SetCollider(colliderFill);
            }
        }
    }
}
