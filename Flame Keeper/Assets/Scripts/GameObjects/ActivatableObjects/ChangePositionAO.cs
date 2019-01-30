using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePositionAO : ActivatableObject
{
    public Transform moveableObject;
    public Transform activatedPosition;
    public Vector3 incrementPosition;
    public float moveSpeed = 1.0f;

    private Vector3 startingPosition;
    private Vector3 targetPosition;

    protected override void OnPowered(int level)
    {
        targetPosition = startingPosition + (incrementPosition * level);
    }

    protected override void OnDepowered()
    {
        targetPosition = startingPosition;
    }

    private void Start()
    {
        startingPosition = moveableObject.transform.position;
        targetPosition = startingPosition;
    }

    private void Update()
    {
        moveableObject.transform.position = Vector3.Lerp(moveableObject.transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}
