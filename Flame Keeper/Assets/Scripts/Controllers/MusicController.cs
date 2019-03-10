using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : BaseController
{
    public List<AudioClip> musicTracks;

    // Create more sources if needed,
    // right now the scope of the project is to only have one track going at any time
    public AudioSource musicSource;

    public enum MusicTracks
    {
        MainTheme
    }

    public void PlayTrack(MusicTracks track)
    {
        // I'm enabling / disabling the gameobject just so you can see what music tracks are active
        // in the scene's hierarchy
        musicSource.gameObject.SetActive(true);
        musicSource.clip = musicTracks[(int)track];
        musicSource.Play();
    }

    public void PauseCurrentTrack()
    {
        musicSource.Pause();
    }

    public void UnPauseCurrentTrack()
    {
        musicSource.UnPause();
    }

    public void StopCurrentTrack()
    {
        musicSource.Stop();
        musicSource.gameObject.SetActive(false);
    }
}
