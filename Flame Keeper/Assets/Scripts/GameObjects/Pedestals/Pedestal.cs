using System;
using System.Collections.Generic;

using UnityEngine;

public class Pedestal : MonoBehaviour, DynamicLightSource
{
    public float activateAnimationSpeed;
    public bool actAsCheckpoint = true;

    [Header("Connected Triggers")]
    public List<ActivatableObject> connectedTriggers = new List<ActivatableObject>();

    [Header("Emissions")]
    public List<MeshRenderer> emitters;
    public float emissionFadeTime = 1.0f;
    public float deactivatedEmitterIntensity = -3.0f;
    public float activatedEmitterIntensity = 3.0f;
    public List<MeshRenderer> renderersInEmission = new List<MeshRenderer>();
    public Material deactiveColor;

    [Header("Lights")]
    public DynamicLightController pointLightController;

    [Header("Orbital Targets")]
    public List<Transform> orbitalTargets;

    [Header("Testing, should delete later")]
    public float activateDistance = 2.0f;

    private PlayerControllerSimple player;
    private bool activated = false;
    private Color emissionColor;
    private MeshRenderer wireRenderer;
    private List<float> emissionColorLerpTimes = new List<float>(); // For tracking when to fade in emissive colors

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

    private GameObject audioController;

    /// <summary>
    /// Assign gameobject references and initialize variables
    /// </summary>
    private void Start()
    {
        currLevel = startLevel;
        pointLightController.Setup(this);
        player = FlameKeeper.Get().levelController.GetPlayer();

        if (currLevel > 0)
        {
            activated = true;
            ActivatePedestal();
        }

        if (emitters.Count > 0)
        {
            emissionColor = emitters[0].materials[1].color;
        }

        foreach (MeshRenderer ren in emitters)
        {
            emissionColorLerpTimes.Add(0.0f);
        }

        audioController = (GameObject)Instantiate(Resources.Load("audioController"), this.transform.position, this.transform.rotation);
        audioController.transform.SetParent(this.transform);
    }

    /// <summary>
    /// Non physics based update
    /// </summary>
    private void Update()
    {
        float playerDistance = Vector3.Distance(this.transform.position, player.transform.position);

        // Check for pedestal activation
        if (Input.GetButtonDown(StringConstants.Input.ActivateButton) && playerDistance < activateDistance && player.IsInputEnabled())
        {
            // Async on this action so it happens after the flame enters the pedestal
            Action onPedestalLit = () =>
            {
                currLevel++;
                emissionColorLerpTimes[currLevel - 1] = emissionFadeTime;
                if (actAsCheckpoint)
                    player.RecordCheckpoint();
                ActivatePedestal();
                audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.fire5);
            };
            if (currLevel < maxLevel)
            {
                player.RequestLanternUse(orbitalTargets[currLevel].position, onPedestalLit);
            }
        }

        // Check for pedestal deactivation
        if (Input.GetButtonDown(StringConstants.Input.DeactivateButton) && playerDistance < activateDistance && player.IsInputEnabled())
        {
            if (activated && player.RequestLanternAddition(orbitalTargets[currLevel-1].position))
            {
                currLevel--;
                emissionColorLerpTimes[currLevel] = emissionFadeTime;
                if (actAsCheckpoint)
                    player.RecordCheckpoint();
                if (currLevel <= 0)
                {
                    DeactivatePedestal();
                    currLevel = 0;
                    audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.fire1);
                }
                else
                {
                    ActivatePedestal();
                }
            }
        }

        int i = 0;
        foreach (MeshRenderer emitter in emitters)
        {
            // Lerp to emissive color, if needed
            emissionColorLerpTimes[i] = Mathf.Max(emissionColorLerpTimes[i] - Time.deltaTime, 0.0f);
            Color deactiveEmissive = deactiveColor.color * deactivatedEmitterIntensity * deactivatedEmitterIntensity * Mathf.Sign(deactivatedEmitterIntensity);
            Color activeEmissive = emissionColor * activatedEmitterIntensity * activatedEmitterIntensity * Mathf.Sign(activatedEmitterIntensity);
            if (i < currLevel)
            {
                emitter.materials[1].SetVector("_Color", Color.Lerp(emissionColor, deactiveColor.color, emissionColorLerpTimes[i]/emissionFadeTime));
                emitter.materials[1].SetVector("_EmissionColor", Color.Lerp(activeEmissive, deactiveEmissive, emissionColorLerpTimes[i] / emissionFadeTime));
            }
            else
            {
                emitter.materials[1].SetVector("_Color", Color.Lerp(deactiveColor.color, emissionColor, emissionColorLerpTimes[i] / emissionFadeTime));
                emitter.materials[1].SetVector("_EmissionColor", Color.Lerp(deactiveEmissive, activeEmissive, emissionColorLerpTimes[i] / emissionFadeTime));
            }
            i++;
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

    /// <summary>
    /// Checks if the pedestal has any amount of charge in it
    /// </summary>
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

    /// <summary>
    /// Gets the current number of charges in the pedestal
    /// </summary>
    public int GetCurrLevel()
    {
        return currLevel;
    }
}
