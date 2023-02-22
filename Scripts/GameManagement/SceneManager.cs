using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private bool inDebugMode = true;
    [SerializeField] private int playerSceneReference;
    [SerializeField] private int debugLevelSceneReference;

    private void Start()
    {
        //load the default scene
        if (inDebugMode)
        {
            LoadForDebug();
        }
    }

    void LoadForDebug()
    {

    }
}
