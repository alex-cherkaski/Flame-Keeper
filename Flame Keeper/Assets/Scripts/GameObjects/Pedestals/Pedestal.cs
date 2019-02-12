using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : MonoBehaviour, DynamicLightSource
{
    public float activateAnimationSpeed;

    [Header("Connected Triggers")]
    public List<ActivatableObject> connectedTriggers = new List<ActivatableObject>();

    [Header("Emissions")]
    public GameObject emitter;
    public float deactivatedEmitterIntensity = -3.0f;
    public float activatedEmitterIntensity = 3.0f;
    public List<MeshRenderer> renderersInEmission = new List<MeshRenderer>();

    [Header("Lights")]
    public DynamicLightController pointLightController;

    [Header("Wire")]
    public GameObject wire;
    public Color wireDeactivatedColor;
    public Color wireActivatedColor;

    [Header("Testing, should delete later")]
    public PlayerControllerSimple player;
    public float activateDistance = 2.0f;

    private bool activated = false;
    private MeshRenderer wireRenderer;
    private MeshRenderer emitterRenderer;

    public int maxLevel;
    public int startLevel;
    int _currLevel;
    private int currLevel
    {
        get
        {
            return _currLevel;
        }
        set
        {
            _currLevel = value;

            if (pointLightController && pointLightController.IsSetup())
                OnLightSourceValueChange(_currLevel);
        }
    }

    private void Start()
    {
        wireRenderer = wire.GetComponent<MeshRenderer>();
        emitterRenderer = emitter.GetComponent<MeshRenderer>();
        currLevel = startLevel;
        if (currLevel > 0)
        {
            activated = true;
        }

        pointLightController.Setup(this);

        if (activated)
        {
            wireRenderer.material.color = wireActivatedColor;
            ActivatePedestal();
        }
        else
        {
            wireRenderer.material.color = wireDeactivatedColor;
        }
    }

    private void Update()
    {
        float playerDistance = Vector3.Distance(emitter.transform.position, player.transform.position); // TODO: Should have a level controller or whatever where we get the player reference
        if (Input.GetButtonDown(StringConstants.Input.ActivateButton) && playerDistance < activateDistance)
        {
            if (currLevel < maxLevel && player.RequestLanternUse())
            {
                currLevel++;
                ActivatePedestal();
            }
        }
        if (Input.GetButtonDown(StringConstants.Input.DeactivateButton) && playerDistance < activateDistance)
        {
            if (activated && player.RequestLanternAddition())
            {
                currLevel--;
                if (currLevel <= 0)
                {
                    DeactivatePedestal();
                    currLevel = 0;
                }
                else
                {
                    ActivatePedestal();
                }
            }
        }

        if (activated)
        {
            wireRenderer.material.color = Color.Lerp(wireRenderer.material.color, wireActivatedColor, Time.deltaTime * activateAnimationSpeed);
            emitterRenderer.materials[1].SetVector("_EmissionColor", emitterRenderer.materials[1].color * activatedEmitterIntensity * activatedEmitterIntensity * Mathf.Sign(activatedEmitterIntensity));
        }
        else
        {
            wireRenderer.material.color = Color.Lerp(wireRenderer.material.color, wireDeactivatedColor, Time.deltaTime * activateAnimationSpeed);
            emitterRenderer.materials[1].SetVector("_EmissionColor", emitterRenderer.materials[1].color * deactivatedEmitterIntensity * deactivatedEmitterIntensity * Mathf.Sign(deactivatedEmitterIntensity));
        }

        // If we are updating an emission intensity, we have to tell all materials that it touches
        // to re compute their global illumination. There is probably a better method to do this but
        // for now I think this is ok.
        foreach (MeshRenderer rend in renderersInEmission)
        {
            rend.UpdateGIMaterials();
        }
    }

    /// <summary>
    /// Notifies light controller of new value
    /// </summary>
    public void OnLightSourceValueChange(int newValue)
    {
        pointLightController.CalculateLightTargets(newValue);
    }

    /// <summary>
    /// Returns the number of lantern uses the player currently has
    /// </summary>
    /// <returns></returns>
    public int GetLightSourceValue()
    {
        return GetCurrLevel();
    }


    public bool IsActive()
    {
        return activated;
    }

    public void ActivatePedestal()
    {
        activated = true;
        foreach (ActivatableObject obj in connectedTriggers)
        {
            obj.OnPedestalActivate(this);
        }
    }

    public void DeactivatePedestal()
    {
        foreach (ActivatableObject obj in connectedTriggers)
        {
            obj.OnPedestalDeactivate(this);
        }

        activated = false;
    }

    public int GetCurrLevel()
    {
        return currLevel;
    }
}
