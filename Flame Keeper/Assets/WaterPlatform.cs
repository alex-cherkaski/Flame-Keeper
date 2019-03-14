using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPlatform : MonoBehaviour
{
    public float rotateSpeed = 0.1f;
    public float sinkSpeed = 0.1f;

    public float rotateIntensity = 0.3f; // Keep in the range 0-1, how much the platform tilts when you jump on it
    public float heightDecrease = 0.5f; // How low the platform sinks when the player jump on it

    private Quaternion targetRotation;
    private Vector3 startingPosition;
    private Vector3 targetPosition;
    private PlayerControllerSimple player;
    private bool playerInContact;
    private float startingWaterHeight;
    private float yOffset = 0.0f;

    private void Start()
    {
        player = FlameKeeper.Get().levelController.GetPlayer();
        targetRotation = Quaternion.identity;
        startingPosition = this.transform.position;
        targetPosition = this.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(StringConstants.Tags.Player))
        {
            playerInContact = true;

            // Set the correct target position, should sink a little bit since there is mass on it
            targetPosition = new Vector3(startingPosition.x, startingPosition.y - heightDecrease + yOffset, startingPosition.z);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(StringConstants.Tags.Player))
        {
            playerInContact = true;

            // Set the correct target position, should sink a little bit since there is mass on it
            targetPosition = new Vector3(startingPosition.x, startingPosition.y - heightDecrease + yOffset, startingPosition.z);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(StringConstants.Tags.Player))
        {
            playerInContact = false;

            // Set the correct target position, which is the starting resting position
            targetPosition = startingPosition;
        }
    }

    private void FixedUpdate()
    {
        if (playerInContact)
        {
            // Get a vector pointing towards the player
            Vector3 pointToPlayer = Vector3.Normalize(player.transform.position - this.transform.position);

            // Get the rotation from pointing straight up to that last vector
            targetRotation = Quaternion.FromToRotation(Vector3.up, Vector3.Lerp(Vector3.up, pointToPlayer, rotateIntensity));
        }
        else
        {
            targetRotation = Quaternion.identity;
        }

        // Animate towards the target rotation
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.time * 0.01f * rotateSpeed);

        // Always update position after doing the rotation math
        this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, Time.time * 0.01f * sinkSpeed);
    }
}
