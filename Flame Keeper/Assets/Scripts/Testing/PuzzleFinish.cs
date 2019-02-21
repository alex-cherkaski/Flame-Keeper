using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PuzzleFinish : MonoBehaviour
{
    [Header("Events")]
    public List<UnityEvent> eventsToPlay;

    private bool played;

    void Start()
    {
        played = false;
    }

    void Update()
    {
        if (CheckIfFinished() && !played)
        {
            OnFinish();
            played = true;
        }
    }

    protected void OnFinish()
    {
        foreach (UnityEvent events in eventsToPlay) 
        {
            if (events != null)
            {
                events.Invoke();
            }
        }
        
    }

    protected abstract bool CheckIfFinished();
}
