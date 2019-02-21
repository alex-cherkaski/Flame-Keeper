using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleFinishOnPedestal : PuzzleFinish
{
    [Header("Final Pedestal State")]
    //make sure to input them in the same order
    public List<Pedestal> pedestals;
    public List<int> finalLevels; 

    protected override bool CheckIfFinished()
    {
        int i;
        for (i = 0; i < pedestals.Count; i++)
        {
            if (pedestals[i] == null || !IsLevelFinal(pedestals[i], finalLevels[i]))
            {
                return false;
            }
        }
        return true;
    }

    bool IsLevelFinal(Pedestal pedestal, int finalLevel)
    {
        if (pedestal == null)
        {
            return false;
        }
        if (pedestal.GetCurrLevel() == finalLevel)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}