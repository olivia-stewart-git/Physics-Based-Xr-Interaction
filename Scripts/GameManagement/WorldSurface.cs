using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSurface : MonoBehaviour
{
    public enum SurfaceType {wood, metal, concrete, dirt,  water, flesh}
    public SurfaceType surfaceType;

    [Header("Overrides")] //use overrides for when custom surface 
    public bool doOverrideValues;
    [Range(0f, 1f)]public float overrideHardness;
    public SurfaceMap overrideSurface;

    private void Start()
    {
        if (doOverrideValues)
        {
            ReadOverrideValues();
        }
        else
        {
            ReadMaterialValues();
        }
    }

    void ReadOverrideValues()
    {

    }

    void ReadMaterialValues()
    {

    }
}
