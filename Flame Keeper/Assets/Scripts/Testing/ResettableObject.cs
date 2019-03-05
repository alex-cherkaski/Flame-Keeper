using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResettableObject : ActivatableObject
{
    public List<BurnableObject> resetObject;

    protected override void OnPowered(Pedestal pedestal, int newLevel)
    {
        foreach (BurnableObject obj in resetObject)
        {
            if (obj.CanBeReset())
            {
                obj.ResetThis();
            }
        }
    }

    protected override void OnDepowered(Pedestal pedestal, int newLevel)
    {
    }

}
