using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This scene only serves to create the game manager and then
/// transition to the main menu.
/// </summary>
public class RootLevelConfig : LevelConfig
{
    protected override void OnLevelStart()
    {
        base.OnLevelStart();
        SceneManager.LoadScene(StringConstants.SceneNames.MainMenuScene);
    }
}
