using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PuzzleFinish : MonoBehaviour
{
    public int waitTime;

    [Header("Events")]
    public List<UnityEvent> eventsToPlay;

    private bool played;
    private GameObject player;

    void Start()
    {
        played = false;
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (CheckIfFinished() && !played)
        {
            StartCoroutine(OnFinish());
            played = true;
        }
    }

    IEnumerator OnFinish()
    {
        player.GetComponent<PlayerControllerSimple>().DisableInput();
        foreach (UnityEvent events in eventsToPlay) 
        {
            if (events != null)
            {
                events.Invoke();
            }
        }
        yield return new WaitForSeconds(waitTime);
        player.GetComponent<PlayerControllerSimple>().EnableInput();
    }

    protected abstract bool CheckIfFinished();
}
