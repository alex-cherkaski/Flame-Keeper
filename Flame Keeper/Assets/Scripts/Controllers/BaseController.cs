using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template for any controllers you want to add, just override it
/// and add any functionality you want. Note that these are not MonoBehaviours,
/// since they don't rely any GameObject properties. But they still have Start
/// and Update Functions that are called from the game manager
/// </summary>
public abstract class BaseController
{
    public bool m_bInitialized { get; protected set; }

    public virtual void Initialize()
    {
        m_bInitialized = true;
    }

    public virtual void Update(float deltaTime)
    {

    }
}
