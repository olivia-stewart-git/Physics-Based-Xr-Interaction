using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluntDamager : MonoBehaviour
{
    private Rigidbody rb;

    public Transform centreOfMass;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        //we get the force generate at collision

    }

    private void OnDrawGizmosSelected()
    {
        
    }
}
