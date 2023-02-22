using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLogger : MonoBehaviour
{
    public Transform noticeHolder;
    private string loggingTextName = "logText";

    private ObjectPooler objPooler;
    public bool doLogging = false;

    private void Start()
    {
        objPooler = ObjectPooler.Instance;
        
    }

    public void LogNotice(string notice)
    {
        if (doLogging)
        {
            GameObject noticeInstance = objPooler.SpawnFromPool(loggingTextName, Vector3.zero, Quaternion.identity, noticeHolder);
            TextMeshProUGUI textInstance = noticeInstance.GetComponent<TextMeshProUGUI>();
            textInstance.text = notice;
        }
    }
}
