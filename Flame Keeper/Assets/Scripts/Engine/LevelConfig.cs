using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelConfig : MonoBehaviour
{
    protected bool m_levelStarted = false;

    /// <summary>
    /// Called by a parent script when the config is first registered
    /// </summary>
    public void InitConfig()
    {
        m_levelStarted = true;
        OnLevelStart();
    }

    /// <summary>
    /// Called when the level starts
    /// </summary>
    protected virtual void OnLevelStart()
    {
        // Implement in inherited classes
    }
}
