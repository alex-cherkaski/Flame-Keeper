using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalLevelSceneTransition : MonoBehaviour
{
    public void GoToThankYouScene()
    {
        SceneManager.LoadScene(StringConstants.SceneNames.ThankYou);
    }
}
