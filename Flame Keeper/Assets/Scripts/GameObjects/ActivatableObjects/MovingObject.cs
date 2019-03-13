using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : ActivatableObject
{
    [Header("Events")]
    //the object will always start by going towards the first position
    public List<Vector3> positions;
    public bool cycle;
    public float moveSpeed;

    public float startDelay;
    public float intervalPauseDelay;

    private float waitDelay;

    private Vector3 minStep;
    private Vector3 targetPosition;

    private bool isMoving;
    private bool forward;
    private int count;
    private GameObject moveableObject;

    private GameObject audioController;

    protected override void OnPowered(Pedestal pedestal, int newLevel)
    {
        if (newLevel == 1)
        {
            waitDelay = startDelay;
        }

        if (newLevel > 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    protected override void OnDepowered(Pedestal pedestal, int newLevel)
    {
        if (newLevel > 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    private void Start()
    {
        if (GetLevel() > 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        count = 0;
        minStep = Vector3.zero;
        forward = true;
        moveableObject = this.gameObject;
        targetPosition = positions[0];

        //audioController = (GameObject)Instantiate(Resources.Load("audioController"), this.transform.position, this.transform.rotation);
        //audioController.transform.SetParent(this.transform);
    }

    private void Update()
    {
        if (waitDelay >= 0)
        {
            waitDelay -= Time.deltaTime;
            return;
        }

        //move only if isMoving is true
        if (isMoving)
        {
            moveableObject.transform.position = Vector3.Lerp(moveableObject.transform.position, targetPosition, Time.deltaTime * moveSpeed) + minStep;
        }

        //checks if positions are approximately the same
        if (Vector3.Distance(moveableObject.transform.position, targetPosition) < 0.01 && waitDelay <= 0.0)
        {
            moveableObject.transform.position = targetPosition;

            if (cycle)
            {
                count = (count + 1) % positions.Count;
            }
            else
            {
                if (forward)
                {
                    count++;
                    //if at the end of the list, switch to decrementing count
                    if (count == positions.Count)
                    {
                        forward = false;
                        count = positions.Count - 2;
                        minStep = (positions[count] - positions[count + 1]) * 0.001f;
                    }
                    else
                    {
                        minStep = (positions[count] - positions[count - 1]) * 0.001f;
                    }
                    //audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.Scrape3);
                }
                else
                {
                    count--;
                    //if at the beginning of the list, switch between incrementing count
                    if (count == -1)
                    {
                        forward = true;
                        count = 1;
                        minStep = (positions[count] - positions[count - 1]) * 0.001f;
                    }
                    else
                    {
                        minStep = (positions[count] - positions[count + 1]) * 0.001f;
                    }
                    //audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.Scrape3);
                }
            }
            //switch targetposition to next on list
            targetPosition = positions[count];
            waitDelay = intervalPauseDelay;
        }
    }
}
