using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class Totem : MonoBehaviour
{
    public string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(StringConstants.Tags.Player))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
