using System;
using System.Collections.Generic;

using UnityEngine;

public class Pedestal : MonoBehaviour, DynamicLightSource
{
    [Header("Triggers")]
    public List<ActivatableObject> connectedTriggers = new List<ActivatableObject>();

    [Header("Emissions")]
    public List<MeshRenderer> emitters;
    public float emissionFadeTime = 1.0f;
    public float deactivatedEmitterIntensity = -3.0f;
    public float activatedEmitterIntensity = 3.0f;
    public List<MeshRenderer> renderersInEmission = new List<MeshRenderer>(); // Not in use, but will be if we do lightmaps
    public Material deactiveColor;

    [Header("Lights")]
    public DynamicLightController pointLightController;

    [Header("Orbital Targets")]
    public List<Transform> orbitalTargets;

    [Header("Activation parameters")]
    public float activateDistance = 2.0f;

    [Header("Checkpoints")]
    public bool actAsCheckpoint = true;
    public GameObject checkPoint;

    [Header("Charges")]
    public int maxLevel;
    public int startLevel;

    private PlayerControllerSimple player;
    private bool activated = false; // Ie, does the pedestal have at least one charge?

    private Color emissionColor;
    private List<float> emissionColorLerpTimes = new List<float>(); // For tracking when to fade in emissive colors

    // Important distinction for coders: current orbitals and current level are seperate ideas
    // although them seem similar at first glance. The number of orbitals include orbitals that are currently
    // flying into the totem, though they haven't actually powered it yet. Ie, the number of orbitals may be
    // higher than the current level / power the pedestal is giving off. It's split this way so that we don't
    // accidentally 'overcharge' a pedestal when an orbital is currently animating into it.
    private int currentOrbitals;
    private int _currentLevel;
    private int currentLevel
    {
        get
        {
            return _currentLevel;
        }
        set
        {
            _currentLevel = value;

            if (pointLightController && pointLightController.IsSetup())
                OnLightSourceValueChange(_currentLevel);
        }
    }

    private GameObject audioController;

    /// <summary>
    /// Assign gameobject references and initialize variables
    /// </summary>
    private void Start()
    {
        currentLevel = startLevel;
        currentOrbitals = startLevel;
        pointLightController.Setup(this);
        player = FlameKeeper.Get().levelController.GetPlayer();

        // Check to see if the pedestal is set up correctly
        if (orbitalTargets.Count != maxLevel)
        {
            Debug.LogWarning("Not enough orbital targets set for " + this);
        }

        if (emitters.Count != maxLevel)
        {
            Debug.LogWarning("Not enough emitters set for " + this);
        }

        if (currentLevel > 0)
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
                currentLevel++;
                emissionColorLerpTimes[currentLevel - 1] = emissionFadeTime;
                if (actAsCheckpoint)
                    player.RecordCheckpoint(checkPoint.transform.position);
                ActivatePedestal();
                audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.fire5);
            };
            if (currentOrbitals < maxLevel && player.RequestLanternUse(orbitalTargets[currentOrbitals].position, onPedestalLit))
            {
                currentOrbitals++;
            }
        }

        // Check for pedestal deactivation
        if (Input.GetButtonDown(StringConstants.Input.DeactivateButton) && playerDistance < activateDistance && player.IsInputEnabled())
        {
            if (activated && player.RequestLanternAddition(orbitalTargets[currentLevel-1].position))
            {
                currentLevel--;
                currentOrbitals--;
                emissionColorLerpTimes[currentLevel] = emissionFadeTime;
                if (actAsCheckpoint)
                    player.RecordCheckpoint(checkPoint.transform.position);
                if (currentLevel <= 0)
                {
                    DeactivatePedestal();
                    currentLevel = 0;
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
            if (i < currentLevel)
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

    /// <summary>
    /// Notifies all triggers of an activation
    /// </summary>
    public void ActivatePedestal()
    {
        activated = true;

        foreach (ActivatableObject obj in connectedTriggers)
        {
            obj.OnPedestalActivate(this);
        }
    }

    /// <summary>
    /// Notifies all triggers of a deactivation
    /// </summary>
    public void DeactivatePedestal()
    {
        activated = false;

        foreach (ActivatableObject obj in connectedTriggers)
        {
            obj.OnPedestalDeactivate(this);
        }
    }

    /// <summary>
    /// Gets the current number of charges in the pedestal
    /// </summary>
    public int GetCurrLevel()
    {
        return currentLevel;
    }
}
