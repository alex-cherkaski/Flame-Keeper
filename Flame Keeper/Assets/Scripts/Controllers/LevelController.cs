using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : BaseController
{
    private LevelConfig m_currentLevelConfig;

    public override void Initialize()
    {
        base.Initialize();

        SceneManager.sceneLoaded += OnNewLevel;
    }

    public PlayerControllerSimple GetCurrentPlayer()
    {
        return m_currentLevelConfig.player;
    }

    void OnNewLevel(Scene scene, LoadSceneMode mode)
    {
        // Find the config that exists for this level somewhere in the scene
        LevelConfig[] foundConfigs = FindObjectsOfType<LevelConfig>();
        int count = foundConfigs.Length;

        if (count == 0)
        {
            Debug.LogError("No level config found in the scene!");
        }
        else if (count > 1)
        {
            Debug.LogError("Multiple level configs found in the scene!");
        }
        else
        {
            // Setup the current level's configuration
            m_currentLevelConfig = foundConfigs[0];
            m_currentLevelConfig.transform.SetParent(this.transform);
        }
    }
}
