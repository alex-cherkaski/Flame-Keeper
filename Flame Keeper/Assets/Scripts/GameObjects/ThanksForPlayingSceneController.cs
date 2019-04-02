using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class ThanksForPlayingSceneController : MonoBehaviour
{
    private bool allowInput = false;

    void Start()
    {
        StartCoroutine(InputDelay(4.0f));
    }

    IEnumerator InputDelay(float time)
    {
        yield return new WaitForSeconds(time);
        allowInput = true;
    }

    private void Update()
    {
        if (allowInput)
        {
            if (Input.anyKey)
            {
                SceneManager.LoadScene(StringConstants.SceneNames.RootSceneName);
            }
        }
    }
}
