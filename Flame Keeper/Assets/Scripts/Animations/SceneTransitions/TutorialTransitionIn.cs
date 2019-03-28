using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class TutorialTransitionIn : MonoBehaviour
{
    public FreezePlayer startingCutscene;
    public Cinemachine.CinemachineVirtualCamera startingCamera;
    public Image overlayImage;
    public Animator fadeInAnimator;

    private PlayerControllerSimple player;
    private bool shouldTransitionIn;

    void Start()
    {
        shouldTransitionIn = !FlameKeeper.Get().levelController.CutscenesDisabled();
        player = FlameKeeper.Get().levelController.GetPlayer();

        TutorialFadeStateBehaviour tutorialFadeBehaviour = fadeInAnimator.GetBehaviour<TutorialFadeStateBehaviour>();
        tutorialFadeBehaviour.tutorialTransition = this;

        if (!shouldTransitionIn)
        {
            overlayImage.color = Color.clear;
            fadeInAnimator.SetBool("Skip", true);
        }
        else
        {
            overlayImage.color = Color.black;
            player.DisableInput();
            startingCamera.Priority = 100; // Start on the cutscene camera, but don't play it just yet
            fadeInAnimator.SetBool("Start", true);
        }
    }

    public void OnFadeInComplete()
    {
        if (shouldTransitionIn)
        {
            startingCutscene.PlayTimeline();
            startingCamera.Priority = 1; // Let cinemachine do its thing
        }
    }
}
