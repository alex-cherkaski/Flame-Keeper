using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class Totem : MonoBehaviour
{
    public Animator endAnimator;
    public Animator whiteOutAnimator;
    public string nextSceneName;
    public Transform endPoint;
    public bool startWhite = false;
    public FreezePlayer startingCutscene;
    public Cinemachine.CinemachineVirtualCamera startingCamera;
    public AudioController flamesAudioController;
    public AudioController burnAudioController;

    [HideInInspector]
    public float burnPercent = 0.0f; // To be controlled in the animation controller

    private List<ParticleSystem> particleSystems;
    private PlayerControllerSimple player;

    private void Start()
    {
        player = FlameKeeper.Get().levelController.GetPlayer();
        particleSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());

        WhiteOutBehaviour whiteOutBehaviour = whiteOutAnimator.GetBehaviour<WhiteOutBehaviour>();
        whiteOutBehaviour.totemController = this;

        WhiteInBehaviour whiteInBehaviour = whiteOutAnimator.GetBehaviour<WhiteInBehaviour>();
        whiteInBehaviour.totemController = this;

        if (startWhite)
        {
            whiteOutAnimator.SetBool("StartWhite", true);
            whiteOutAnimator.SetBool("WhiteIn", false);
            whiteOutAnimator.SetBool("WhiteOut", true);

            player.DisableInput();

            if (!FlameKeeper.Get().levelController.CutscenesDisabled() && startingCutscene != null && startingCamera != null)
                startingCamera.Priority = 100; // Start on the cutscene camera, but don't play it just yet
        }

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        FlameKeeper.Get().dataminingController.StopTrackingScene();
        if (other.CompareTag(StringConstants.Tags.Player))
        {
            player.DisableInput();
            player.MoveToPoint(endPoint.position, OnPlayerFinishMove);
        }
    }

    private void OnPlayerFinishMove()
    {
        player.RotateToPoint(endPoint.position + this.transform.forward, OnPlayerFinishRotate);
    }

    private void OnPlayerFinishRotate()
    {
        endAnimator.SetTrigger("EndAnimation");
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Play(false);
        }
        flamesAudioController.PlayAudioClip(AudioController.AudioClips.fire4);
        StartCoroutine(ExecuteAfterTime(1.33f, () =>
        {
            burnAudioController.PlayAudioClip(AudioController.AudioClips.fire1);
        }));
        StartCoroutine(ExecuteAfterTime(3.0f, () =>
        {
            whiteOutAnimator.SetBool("WhiteIn", true);
            whiteOutAnimator.SetBool("WhiteOut", false);
        }));
    }

    /// <summary>
    /// yeah i hate this but its fast to implements
    /// </summary>
    /// <param name="time"></param>
    IEnumerator ExecuteAfterTime(float time, Action onComplete = null)
    {
        yield return new WaitForSeconds(time);

        onComplete?.Invoke();
    }

    private void Update()
    {
        player.SetBurnPercent(burnPercent);
    }

    public void OnWhiteInComplete()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnWhiteOutComplete()
    {
        whiteOutAnimator.SetBool("StartWhite", false);

        if (!FlameKeeper.Get().levelController.CutscenesDisabled() && startingCutscene != null && startingCamera != null)
        {
            startingCutscene.PlayTimeline();
            startingCamera.Priority = 1;
        }
        else
        {
            player.EnableInput();
        }
    }
}
