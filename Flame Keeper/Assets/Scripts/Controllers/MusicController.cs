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


    public void FadeOut(float fadeTime)
    {
        StartCoroutine(FadeOutCoroutine(fadeTime));
    }
    private IEnumerator FadeOutCoroutine(float fadeTime)
    {
        float startVolume = musicSource.volume;
        float totalTime = 0.0f;

        while (musicSource.volume > 0)
        {
            totalTime += Time.deltaTime;
            musicSource.volume = startVolume * (1.0f - totalTime / fadeTime);

            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
        musicSource.gameObject.SetActive(false);
    }

    public void FadeIn(MusicTracks track, float fadeTime)
    {
        StartCoroutine(FadeInCoroutine(track, fadeTime));
    }
    private IEnumerator FadeInCoroutine(MusicTracks track, float fadeTime)
    {
        float startVolume = musicSource.volume;
        float totalTime = 0.0f;

        musicSource.gameObject.SetActive(true);
        musicSource.clip = musicTracks[(int)track];
        musicSource.volume = 0.0f;
        musicSource.Play();

        while (musicSource.volume < startVolume)
        {
            totalTime += Time.deltaTime;
            musicSource.volume = startVolume * totalTime / fadeTime;

            yield return null;
        }
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
