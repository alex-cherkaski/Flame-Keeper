using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleEmitter : MonoBehaviour
{
    public ParticleSystem m_particleSystem;

    public void StopEmitting()
    {
        m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public float GetMaxLifetime()
    {
        return m_particleSystem.main.startLifetime.constantMax;
    }
}
