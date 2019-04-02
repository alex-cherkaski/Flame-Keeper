using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalLevelSceneTransition : MonoBehaviour
{
    public void Start()
    {
        FlameKeeper.Get().musicController.FadeOut(5.0f);
    }

    public void GoToThankYouScene()
    {
        SceneManager.LoadScene(StringConstants.SceneNames.ThankYou);
    }
}
