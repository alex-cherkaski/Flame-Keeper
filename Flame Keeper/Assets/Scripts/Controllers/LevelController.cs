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

    void OnNewLevel(Scene scene, LoadSceneMode mode)
    {
        // Find the config that exists for this level somewhere in the scene
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
        }

        // Setup all our objects in the right positions and what not
        ResetLevel();
    }

    private void ResetLevel()
    {
        if (!m_currentLevelConfig)
        {
            Debug.LogError("No level configuration to set up!");
            return;
        }

        // Set up the player according the level parameters
        m_currentLevelConfig.player.Setup(m_currentLevelConfig.playerStartingPosition,
            m_currentLevelConfig.startingLanternUses,
            m_currentLevelConfig.maxLanternUses);
    }

    public PlayerControllerSimple GetLevelPlayer()
    {
        return m_currentLevelConfig.player;
    }

    public List<Pedestal> GetLevelPedestals()
    {
        return m_currentLevelConfig.GetAllPedestals();
    }
}
