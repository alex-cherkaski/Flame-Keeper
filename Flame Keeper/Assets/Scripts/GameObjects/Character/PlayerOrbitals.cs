using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerOrbitals : MonoBehaviour
{
    public GameObject orbPrefab;

    public float orbitRadius = 1.5f;
    public float spinSpeed = 1.0f;
    public float lerpSpeed = 1.0f;

    private List<Transform> orbitals = new List<Transform>();
    private List<OrbitalOutAnimation> outAnimations = new List<OrbitalOutAnimation>();

    private class OrbitalOutAnimation
    {
        public Transform orbital;
        public Vector3 target;
        public Action onComplete;

        public OrbitalOutAnimation(Transform orbital, Vector3 target, Action onComplete = null)
        {
            this.orbital = orbital;
            this.target = target;
            this.onComplete = onComplete;
        }
    }

    /// <summary>
    /// Called when the number of orbitals should change
    /// </summary>
    public void OnLanternUsesChanged(int newValue, Vector3 source, Action onComplete = null)
    {
        int previousCount = orbitals.Count;
        if (newValue > orbitals.Count)
        {
            // Add orbitals
            for (int i = 0; i < newValue - previousCount; i++)
            {
                GameObject newOrb = Instantiate(orbPrefab);
                newOrb.transform.parent = this.transform;
                newOrb.transform.position = source;
                newOrb.SetActive(true);
                orbitals.Add(newOrb.transform);
            }
        }
        else if (newValue < orbitals.Count)
        {
            // Remove orbitals
            for (int i = 0; i < previousCount - newValue; i++)
            {
                Transform orbToRemove = orbitals[orbitals.Count - 1];
                orbitals.RemoveAt(orbitals.Count - 1);
                orbToRemove.transform.parent = null;
                outAnimations.Add(new OrbitalOutAnimation(orbToRemove, source, onComplete));
            }
        }

        CalculateOrbPositions();
    }

    private void Update()
    {
        // Ignore rotations of the player, (ie, orbs don't rotate as the player turns around)
        this.transform.rotation = Quaternion.identity;

        CalculateOrbPositions();
    }

    /// <summary>
    /// Move all the orbitals into the locations they should be in
    /// </summary>
    private void CalculateOrbPositions()
    {
        // Animate current orbitals
        int total = orbitals.Count;
        Vector3 anchor = this.transform.position;
        float i = 0;
        foreach (Transform orb in orbitals)
        {
            float offset = (i / total) * Mathf.PI * 2.0f;
            Vector3 target = new Vector3(Mathf.Sin(offset + Time.time * spinSpeed) * orbitRadius,
                                         0.0f,
                                         Mathf.Cos(offset + Time.time * spinSpeed) * orbitRadius);
            orb.localPosition = Vector3.Slerp(orb.localPosition, target, Time.deltaTime * lerpSpeed);
            i++;
        }

        // Animate orbitals leaving the player
        List<OrbitalOutAnimation> completedAnimations = new List<OrbitalOutAnimation>();
        foreach (OrbitalOutAnimation anim in outAnimations)
        {
            anim.orbital.position = Vector3.Slerp(anim.orbital.position, anim.target, Time.deltaTime * lerpSpeed);
            if (Vector3.Distance(anim.orbital.position, anim.target) < 0.1f)
            {
                anim.orbital.position = anim.target;
                anim.onComplete?.Invoke();
                completedAnimations.Add(anim);
            }
        }
        foreach (OrbitalOutAnimation anim in completedAnimations)
        {
            Destroy(anim.orbital.gameObject);
            outAnimations.Remove(anim);
        }
    }
}
