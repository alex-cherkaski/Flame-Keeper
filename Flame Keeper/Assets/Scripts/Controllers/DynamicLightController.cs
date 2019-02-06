using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface DynamicLightSource
{
    void OnLightSourceValueChange(int newValue);
    int GetLightSourceValue();
}

/// <summary>
/// Responsible for controlling the light / illumination of the player
/// </summary>
public class DynamicLightController : MonoBehaviour
{
    [System.Serializable]
    public struct PointLightConfig
    {
        public float range;
        public float intensity;
    }

    private DynamicLightSource m_parentController;
    private PointLightConfig m_targetConfig;
    private bool m_setup = false;

    public Light pointLight;
    public List<PointLightConfig> lightConfigs = new List<PointLightConfig>();
    public float updateSpeed = 1.0f;

    public void Setup(DynamicLightSource parentController)
    {
        m_parentController = parentController;
        CalculateLightTargets(m_parentController.GetLightSourceValue());

        // On setup, immediately go to starting values:
        pointLight.intensity = m_targetConfig.intensity;
        pointLight.range = m_targetConfig.range;

        m_setup = true;
    }

    public bool IsSetup()
    {
        return m_setup;
    }

    // Lerp towards our target light values
    private void Update()
    {
        pointLight.intensity = Mathf.Lerp(pointLight.intensity, m_targetConfig.intensity, Time.deltaTime * updateSpeed);
        pointLight.range = Mathf.Lerp(pointLight.range, m_targetConfig.range, Time.deltaTime * updateSpeed);
    }

    // Recalculate our target light values
    public void CalculateLightTargets(int uses)
    {
        int index = uses >= lightConfigs.Count ? lightConfigs.Count - 1 : uses;

        m_targetConfig = lightConfigs[index];
    }
}
