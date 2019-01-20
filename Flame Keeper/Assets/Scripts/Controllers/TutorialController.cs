using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls functionality specific to the tutorial / demo scene
/// </summary>
public class TutorialController : BaseController
{
    private const string sceneName = StringConstants.SceneNames.TutorialSceneName;

    public void GoToTutorialScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
    }
}
