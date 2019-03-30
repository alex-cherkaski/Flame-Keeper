using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrystalScript : MonoBehaviour
{
    [Header("Transform Values")]
    public float degreesPerSecond = 25.0f;
    public float amplitude = 0.1f;
    public float frequency = 1.0f;

    [Header("Charge")]
    public float charge = 3;

    [Header("Particle Systems")]
    public ParticleSystem crystalParticles;
    public GameObject crystalFlame;

    private Vector3 positionOffset;
    private Vector3 temporaryPosition;

    private void Start()
    {
        positionOffset = transform.position;
        temporaryPosition = new Vector3();
    }

    void Update()
    {
        transform.Rotate(0.0f, degreesPerSecond * Time.deltaTime, 0.0f, Space.World);

        temporaryPosition = positionOffset;
        temporaryPosition.y += Mathf.Sin(Mathf.PI * frequency * Time.fixedTime) * amplitude;

        transform.position = temporaryPosition;
    }

    public float GetCharge()
    {
        return charge;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            ParticleSystem.ExternalForcesModule efm = crystalParticles.externalForces;
            efm.enabled = true;

            ParticleSystem.ColorOverLifetimeModule colm = crystalParticles.colorOverLifetime;
            colm.enabled = true;

            DisableParticleSystems();
        }   
    }

    private void DisableParticleSystems()
    {
        crystalParticles.Stop();
        foreach(Transform child in crystalFlame.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.CompareTag(StringConstants.Tags.FireComponent))
            {
                child.gameObject.GetComponent<ParticleSystem>().Stop();
            }
        }
    }
}
