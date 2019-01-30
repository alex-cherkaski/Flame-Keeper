using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableObject : MonoBehaviour
{
    protected HashSet<Pedestal> activeSources = new HashSet<Pedestal>();
    public int maxLevel;

    public void OnPedestalActivate(Pedestal source)
    {
        if (!activeSources.Contains(source))
        {
            activeSources.Add(source);
        }

        int level = 0;
        foreach(Pedestal pedestal in activeSources)
        {
            level += pedestal.GetCurrLevel();
            Debug.LogError(this + "'s level is " + level.ToString());
        }
        
        if (level <= maxLevel)
        {
            OnPowered(level);
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

        if (activeSources.Count == 0)
        {
            OnDepowered();
        }
    }

    protected abstract void OnPowered(int level); // Called when the object receives its first power source
    protected abstract void OnDepowered(); // Called when the object loses its last power source
}
