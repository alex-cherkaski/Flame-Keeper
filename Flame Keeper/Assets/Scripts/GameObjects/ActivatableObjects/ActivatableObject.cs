using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableObject : MonoBehaviour
{
    // Pedestals connected to this object that are currently powered
    protected HashSet<Pedestal> activeSources = new HashSet<Pedestal>();

    /// <summary>
    /// Gets the current power level for this object
    /// </summary>
    public int GetLevel()
    {
        int level = 0;
        foreach (Pedestal pedestal in activeSources)
        {
            level += pedestal.GetCurrLevel();
        }
        return level;
    }

    public void OnPedestalActivate(Pedestal source)
    {
        if (!activeSources.Contains(source))
        {
            activeSources.Add(source);
        }

        if (this.enabled)
        {
            OnPowered(source, GetLevel());
        }
    }

    public void OnPedestalDeactivate(Pedestal source)
    {
        if (source.GetCurrLevel() <= 0 && activeSources.Contains(source))
        {
            activeSources.Remove(source);
        }
        else
        {
            Debug.LogError("Activatable Object did not reference active pedestal! " + this);
        }

        int level = GetLevel();

        if (this.enabled)
        {
            OnDepowered(source, GetLevel());
        }


    }

    protected abstract void OnPowered(Pedestal pedestal, int newLevel); // Called when the object receives a power source
    protected abstract void OnDepowered(Pedestal pedestal, int newLevel); // Called when the object loses a power source
}
