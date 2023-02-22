using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AmmunitionObject : MonoBehaviour
{
    [Header("AmmoSettingss")]
    public HeldObjectInputMale maleInput;
  [Tooltip("Only set in inspector if this is used directly from spawn")] [SerializeField] private Object_AmmunitionFeed ammunitionFeed;
    public int maximumAmmo;
    public bool isAdditive; //ie for shotgun shells
    private int curAmmo;

    // Start is called before the first frame update
    void Start()
    {
        curAmmo = maximumAmmo; 
    }

    private void OnDrawGizmosSelected()
    {
        Handles.Label(transform.position,"max " + maximumAmmo.ToString() + " cur " + curAmmo.ToString());
    }

    public void AttemptLoadAmmo()
    {
        if(maleInput != null)
        {
            GameObject femaleObject = maleInput.FemaleInput().gameObject;
            if(femaleObject != null)
            {
                Object_AmmunitionFeed aFeed = femaleObject.GetComponent<Object_AmmunitionFeed>();
                aFeed.LoadInput(this);
                aFeed.LoadAmmunition(curAmmo, isAdditive);
            }
        }
    }

    public void RemoveAmmuntion(int ammount)
    {
        int value = curAmmo -= ammount;
        if(value < 0)
        {
            value = 0;
        }

        curAmmo = value;
    }

    public void SetAmmunition(int amount)
    {
        curAmmo = amount;
    }
}
