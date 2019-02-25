using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : ActivatableObject
{
    [Header("Events")]
    //the object will always start by going towards the first position
    public List<Vector3> positions;
    public float moveSpeed;
    
    private Vector3 targetPosition;

    private bool isMoving;
    private bool forward;
    private int count;
    private GameObject moveableObject;

    protected override void OnPowered(Pedestal pedestal, int newLevel)
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
        forward = true;
        moveableObject = this.gameObject;
        targetPosition = positions[0];
    }

    private void Update()
    {
        //checks if positions are approximately the same 
        if (Vector3.Distance(moveableObject.transform.position, targetPosition) < 0.1)
        {
            if (forward)
            {
                count++;
                //if at the end of the list, switch to decrementing count
                if (count == positions.Count)
                {
                    forward = false;
                    count = positions.Count - 2;
                }
            }
            else
            {
                count--;
                //if at the beginning of the list, switch between incrementing count
                if (count == -1)
                {
                    forward = true;
                    count = 0;
                }
            }
            //switch targetposition to next on list
            targetPosition = positions[count];
        }
        //move only if isMoving is true
        if (isMoving)
        {
            moveableObject.transform.position = Vector3.Lerp(moveableObject.transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }
}
