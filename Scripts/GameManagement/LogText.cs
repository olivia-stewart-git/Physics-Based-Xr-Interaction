using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogText : MonoBehaviour, IPooledObject
{
    private Coroutine deactivateCoroutine;

    public void OnObjectSpawn()
    {
        if(deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }
        deactivateCoroutine = StartCoroutine(DeactivateText());
    }

    IEnumerator DeactivateText()
    {
        yield return new WaitForSeconds(4f);
        gameObject.SetActive(false);
    }
}
