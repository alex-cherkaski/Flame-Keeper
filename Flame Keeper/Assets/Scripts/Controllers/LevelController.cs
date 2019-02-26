using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : BaseController
{
    private LevelConfig m_currentLevelConfig;

    /// <summary>
    /// Sets up callback for when a new level loads
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        SceneManager.sceneLoaded += OnNewLevel;
    }

    /// <summary>
    /// In the event the game start un-organically, load up the current
    /// level config (since the level is already loaded)
    /// </summary>
    public void StartSimulationMode()
    {
        if (m_currentLevelConfig == null)
        {
            FindLevelConfigInScene();
        }
    }

    void OnNewLevel(Scene scene, LoadSceneMode mode)
    {
        FindLevelConfigInScene();
    }

    /// <summary>
    /// Find the config that exists for this level somewhere in the scene
    /// </summary>
    private void FindLevelConfigInScene()
    {
        LevelConfig[] foundConfigs = FindObjectsOfType<LevelConfig>();
        int count = foundConfigs.Length;

        if (count == 0)
        {
            Debug.LogError("No level config found in the scene!");
            return;
        }
        else if (count > 1)
        {
            Debug.LogError("Multiple level configs found in the scene!");
            return;
        }
        else
        {
            // Setup the current level's configuration
            m_currentLevelConfig = foundConfigs[0];
            m_currentLevelConfig.InitConfig();
        }
    }

    /// <summary>
    /// Gets the player in the level, if it exists, otherwise null.
    /// </summary>
    public PlayerControllerSimple GetPlayer()
    {
        // TODO, we should have a controller specifically for game level logic.
        // This stuff shouldnt be shared across an abstract controller like this
        TempleLevelConfig level = (m_currentLevelConfig as TempleLevelConfig);
        if (level != null)
        {
            return level.player;
        }
        else
        {
            return null;
        }
    }
}
