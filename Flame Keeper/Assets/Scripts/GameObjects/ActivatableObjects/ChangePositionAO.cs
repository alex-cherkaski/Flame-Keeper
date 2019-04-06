using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePositionAO : ActivatableObject
{
    public Transform moveableObject;
    public Vector3 incrementPosition;
    public float moveSpeed = 1.0f;

    public Color activatedColor;
    public Color deactivatedColor;
    private Color currentColor;

    private MeshRenderer meshRenderer;

    private GameObject audioController;

    private Vector3 startingPosition;
    private Vector3 targetPosition;

    private int level;

    protected override void OnPowered(Pedestal pedestal, int newLevel)
    {
        level = newLevel;
        targetPosition = startingPosition + (incrementPosition * level);

        if (audioController != null && this.CompareTag(StringConstants.Tags.StoneBridge))
        {
            if (level == 1)
            {
                audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.water6);
            }
            else if (level == 2)
            {
                audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.water1);
            }
        }
        else if (audioController != null && this.CompareTag(StringConstants.Tags.SlidingDoor))
        {
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.Scrape1);
        }
        else if (audioController != null && this.CompareTag(StringConstants.Tags.TutorialTotem))
        {
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.Scrape2);
        }
    }

    protected override void OnDepowered(Pedestal pedestal, int newLevel)
    {
        if (newLevel == 0)
        {
            targetPosition = startingPosition;
        }

        if (audioController != null && this.CompareTag(StringConstants.Tags.SlidingDoor))
        {
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.Scrape1);
        }
        else if (audioController != null && this.CompareTag(StringConstants.Tags.TutorialTotem))
        {
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.Scrape2);
        }
    }

    void Start()
    {
        startingPosition = moveableObject.transform.position;
        targetPosition = startingPosition + (incrementPosition * GetLevel());
        audioController = (GameObject)Instantiate(Resources.Load("audioController"), this.transform.position, this.transform.rotation);
        audioController.transform.SetParent(this.transform);

        if (this.CompareTag(StringConstants.Tags.Platform))
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (GetLevel() > 0)
            {
                meshRenderer.material.color = activatedColor;
                currentColor = activatedColor;
            }
            else
            {
                meshRenderer.material.color = deactivatedColor;
                currentColor = deactivatedColor;
            }
        }
    }

    private void Update()
    {
        moveableObject.transform.position = Vector3.Lerp(moveableObject.transform.position, targetPosition, Time.deltaTime * moveSpeed);

        if (this.CompareTag(StringConstants.Tags.Platform))
        {
            if (GetLevel() > 0)
            {
                currentColor = activatedColor;
            }
            else
            {
                currentColor = deactivatedColor;
            }
            meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, currentColor, Time.deltaTime);
        }
    }
}
