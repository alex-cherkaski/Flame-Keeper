using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrbitals : MonoBehaviour
{
    public GameObject orbPrefab;
    public float orbitRadius = 1.5f;
    public float spinSpeed = 1.0f;

    private List<Transform> orbitals = new List<Transform>();

    /// <summary>
    /// Called when the number of orbitals should change
    /// </summary>
    public void OnLanternUsesChanged(int newValue, Vector3 source)
    {
        int previousCount = orbitals.Count;
        if (newValue > orbitals.Count)
        {
            // Add orbitals
            for (int i = 0; i < newValue - previousCount; i++)
            {
                GameObject newOrb = Instantiate(orbPrefab);
                newOrb.transform.parent = this.transform;
                newOrb.SetActive(true);
                orbitals.Add(newOrb.transform);
            }
        }
        else if (newValue < orbitals.Count)
        {
            // Remove orbitals
            for (int i = 0; i < previousCount - newValue; i++)
            {
                Transform orbToRemove = orbitals[0];
                orbitals.RemoveAt(0);
                Destroy(orbToRemove.gameObject);
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
        int total = orbitals.Count;
        Vector3 anchor = this.transform.position;
        float i = 0;
        foreach (Transform orb in orbitals)
        {
            float offset = (i / total) * Mathf.PI * 2.0f;
            orb.localPosition = new Vector3(Mathf.Sin(offset + Time.time * spinSpeed) * orbitRadius,
                                            0.0f,
                                            Mathf.Cos(offset + Time.time * spinSpeed) * orbitRadius);
            i++;
        }
    }
}
