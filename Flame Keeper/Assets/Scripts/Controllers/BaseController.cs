using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template for any controllers you want to add, just override it
/// and add any functionality you want.
/// </summary>
public abstract class BaseController : MonoBehaviour
{
    public bool m_bInitialized { get; protected set; }

    // If Initalized, the game manager has setup this object
    public virtual void Initialize()
    {
        m_bInitialized = true;
    }
}
