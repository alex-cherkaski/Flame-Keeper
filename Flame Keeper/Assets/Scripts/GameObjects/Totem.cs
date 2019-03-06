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
            FlameKeeper.Get().dataminingController.StopTrackingScene();
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
