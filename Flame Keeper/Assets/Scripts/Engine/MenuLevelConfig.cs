using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLevelConfig : LevelConfig
{
    private bool m_allowInput = false;

    public Animator storyAnimator;
    public GameObject pressStartText;

    protected override void OnLevelStart()
    {
        base.OnLevelStart();

        m_allowInput = true;

        FlameKeeper.Get().musicController.PlayTrack(MusicController.MusicTracks.MainTheme);
    }

    public void Update()
    {
        if (m_allowInput && Input.GetButton(StringConstants.Input.Start))
        {
            pressStartText.SetActive(false);
            SceneManager.LoadScene(StringConstants.SceneNames.TutorialSceneName);
        }

        if (Input.GetButton(StringConstants.Input.SkipButton))
        {
            storyAnimator.SetTrigger("SkipCutscene");
        }
    }
}
