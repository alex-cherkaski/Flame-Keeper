using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLevelConfig : LevelConfig
{
    private bool m_allowInput = false;

    protected override void OnLevelStart()
    {
        base.OnLevelStart();

        m_allowInput = true;
    }

    public void Update()
    {
        if (m_allowInput && Input.GetButton(StringConstants.Input.Start))
        {
            SceneManager.LoadScene(StringConstants.SceneNames.TutorialSceneName);
        }
    }
}
