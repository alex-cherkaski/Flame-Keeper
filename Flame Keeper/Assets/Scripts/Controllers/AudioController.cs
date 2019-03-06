using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    // The source audio file.
    public AudioClip audioClip;

    // The audio source refernce.
    public AudioSource audioSource;

    public List<AudioClip> audioClipsList;

    public enum AudioClips
    {
        fire1,
        fire2,
        fire3,
        fire4,
        fire5,

        foleyFootsteps,
        stepsReverb,

        jumpNormal,
        reverbJump,

        chain1,
        chain2,
        chain3,
        chain4,
        chain5,
        doorClose,
        doorClose2,
        doorOpen,
        doorOpen2,
        doorOpen3,
        Scrape1,
        Scrape2,
        Scrape3,

        water1,
        water2,
        water3,
        water4,
        water5,
        water6,
        water7,
        water8,
        water9,
        water10,
        water11,
        water12,
        water13,
        water14,
        water15,
    }

    // Start is called before the first frame update
    void Start()
    {
        // Load the audio clip into the audio source.
        //audioSource.clip = audioClip;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Debug.Log("In Update");
        if (Input.GetButtonDown(StringConstants.Input.JumpButton) && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        */
    }

    public void PlayAudioClip(AudioClips clip)
    {
        audioSource.clip = audioClipsList[(int)clip];
        audioSource.Play();
    }
}
