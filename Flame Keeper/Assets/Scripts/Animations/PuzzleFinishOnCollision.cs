using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleFinishOnCollision : PuzzleFinish
{
    private bool enteredArea = false;

    protected override bool CheckIfFinished()
    {
        return enteredArea;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            enteredArea = true;
        }
    }
}
