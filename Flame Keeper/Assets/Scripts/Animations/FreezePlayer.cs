using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class FreezePlayer : MonoBehaviour
{
    public TimelineAsset timeline;
    public bool startOnAwake = true;

    private PlayableDirector director;
    private PlayerControllerSimple player;
    private float duration;

    // Start is called before the first frame update
    void Start()
    {
        director = this.gameObject.GetComponent<PlayableDirector>();
        if (!FlameKeeper.Get().levelController.CutscenesDisabled() && startOnAwake)
        {
            director.Play();
            duration = (float)timeline.duration;
            StartCoroutine(Freeze());
        }
    }

    public void PlayTimeline()
    {
        if (!FlameKeeper.Get().levelController.CutscenesDisabled())
        {
            director.Play();
            duration = (float)timeline.duration;
            StartCoroutine(Freeze());
        }
    }

    IEnumerator Freeze()
    {
        yield return new WaitForSeconds(0.1f);
        player = FlameKeeper.Get().levelController.GetPlayer();
        player.DisableInput();
        yield return new WaitForSeconds(duration);
        player.EnableInput();
    }
}
