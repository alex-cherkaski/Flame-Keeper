using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableObject : MonoBehaviour
{
    protected HashSet<Pedestal> activeSources = new HashSet<Pedestal>();

    public void OnPedestalActivate(Pedestal source)
    {
        bool firstActive = activeSources.Count == 0;

        if (!activeSources.Contains(source))
        {
            activeSources.Add(source);
        }

        if (firstActive)
        {
            OnPowered();
        }
    }

    public void OnPedestalDeactivate(Pedestal source)
    {
        if (activeSources.Contains(source))
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

    protected abstract void OnPowered(); // Called when the object receives its first power source
    protected abstract void OnDepowered(); // Called when the object loses its last power source
}
