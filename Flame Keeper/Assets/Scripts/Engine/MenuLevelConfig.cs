using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLevelConfig : LevelConfig
{
    private bool m_transitionInProgress = false;
    private bool m_storyFinished = false;

    public Animator storyAnimator;
    public AudioController audioController;
    public GameObject pressStartText;

    protected override void OnLevelStart()
    {
        base.OnLevelStart();

        // Link references to our state behaviours for animation callbacks
        StoryStateBehaviour storyStateBehaviour = storyAnimator.GetBehaviour<StoryStateBehaviour>();
        storyStateBehaviour.mainMenuConfig = this;

        WipeOutStateBehaviour wipeOutStateBehaviour = storyAnimator.GetBehaviour<WipeOutStateBehaviour>();
        wipeOutStateBehaviour.mainMenuConfig = this;

        FlameKeeper.Get().musicController.FadeIn(MusicController.MusicTracks.MainTheme, 5.0f);
    }

    public void Update()
    {
        if (Input.GetButton(StringConstants.Input.SkipButton))
        {
            storyAnimator.SetTrigger("SkipCutscene");
        }

        if (m_storyFinished && Input.GetButton(StringConstants.Input.Start) && !m_transitionInProgress)
        {
            Destroy(pressStartText);
            audioController.PlayAudioClip(AudioController.AudioClips.menuFire);
            storyAnimator.SetTrigger("WipeOut");
            m_transitionInProgress = true;
        }
    }

    public void OnStoryFinished()
    {
        m_storyFinished = true;
    }

    public void OnWipeOutFinished()
    {
        SceneManager.LoadScene(StringConstants.SceneNames.TutorialSceneName);
    }
}
