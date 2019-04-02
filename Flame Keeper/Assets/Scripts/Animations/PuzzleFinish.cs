using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PuzzleFinish : MonoBehaviour
{
    public int waitTime;

    [Header("Events")]
    public List<UnityEvent> eventsToPlay;
    public List<UnityEvent> eventsToPlayOnCutsceneEnd;

    private bool played;
    private PlayerControllerSimple player;

    void Start()
    {
        played = false;
        player = FlameKeeper.Get().levelController.GetPlayer();
    }

    void Update()
    {
        if (CheckIfFinished() && !played && !(FlameKeeper.Get().levelController.CutscenesDisabled()))
        {
            StartCoroutine(OnFinish());
            played = true;
        }
    }

    IEnumerator OnFinish()
    {
        player.DisableInput();
        foreach (UnityEvent events in eventsToPlay)
        {
            if (events != null)
            {
                events.Invoke();
            }
        }
        yield return new WaitForSeconds(waitTime);
        foreach (UnityEvent events in eventsToPlayOnCutsceneEnd)
        {
            if (events != null)
            {
                events.Invoke();
            }
        }
        player.GetComponent<PlayerControllerSimple>().EnableInput();
    }

    protected abstract bool CheckIfFinished();
}
