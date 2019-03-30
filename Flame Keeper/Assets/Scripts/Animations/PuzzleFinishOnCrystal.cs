using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleFinishOnCrystal : PuzzleFinish
{
    [Header("Final Pedestal State")]
    //make sure to input them in the same order
    public GameObject crystal;

    protected override bool CheckIfFinished()
    {
        if (crystal.activeSelf == false)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
