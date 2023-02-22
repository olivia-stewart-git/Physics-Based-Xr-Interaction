using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_AmmunitionFeed : MonoBehaviour
{

    [Header("Feed settings")]
 //   public bool requireInput;
    public bool ammunitionLoaded = false;

    private int currentAmmunitionPool;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private AmmunitionObject ammoObject;
    public void LoadInput(AmmunitionObject a_Object)
    {
        ammoObject = a_Object;
    }

    public void LoadAmmunition(int ammuntionAmount, bool isAdditive)
    {
        if (isAdditive)
        {
            currentAmmunitionPool += ammuntionAmount;
        }
        else
        {
            currentAmmunitionPool = ammuntionAmount;
        }

        ammunitionLoaded = true;
    }

    public void UnloadAmmunition()
    {
        if (ammoObject != null)
        {
            ammoObject.SetAmmunition(currentAmmunitionPool);
        }

        ammunitionLoaded = false;

        currentAmmunitionPool = 0;
    }

    public void OnAmmunitionEjected()
    {

    }

    public bool TakeAmmunition(int amount)
    {
        if(currentAmmunitionPool >= amount && currentAmmunitionPool > 0)
        {
            currentAmmunitionPool -= amount;

            if(ammoObject != null)
            {
                ammoObject.RemoveAmmuntion(amount);
            }
            return true;
        }
        return false;
    }

    public bool HasAmmunition()
    {
        if (currentAmmunitionPool > 0 && ammunitionLoaded)
        {
            return true;
        }
        return false;
    }
}
