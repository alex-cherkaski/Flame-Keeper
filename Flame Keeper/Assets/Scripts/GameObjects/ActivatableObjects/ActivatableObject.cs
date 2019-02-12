using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableObject : MonoBehaviour
{
    protected HashSet<Pedestal> activeSources = new HashSet<Pedestal>();
    public int maxLevel;

    public int GetLevel()
    {
        int level = 0;
        foreach (Pedestal pedestal in activeSources)
        {
            level += pedestal.GetCurrLevel();
        }
        Debug.Log("Current level is " + level);
        return level;
    }

    public void OnPedestalActivate(Pedestal source)
    {
        int level = GetLevel();

        if (level <= maxLevel)
        {
            if (!activeSources.Contains(source))
            {
                activeSources.Add(source);
            }
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
        OnDepowered(source, GetLevel());
    }

    protected abstract void OnPowered(Pedestal pedestal, int newLevel); // Called when the object receives a power source
    protected abstract void OnDepowered(Pedestal pedestal, int newLevel); // Called when the object loses a power source
}
