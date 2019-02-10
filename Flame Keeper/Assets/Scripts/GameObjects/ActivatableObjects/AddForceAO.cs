using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddForceAO : ActivatableObject
{
    public Rigidbody controlledBody;

    private Vector3 currentDirection = Vector3.zero;

    protected override void OnPowered(Pedestal pedestal, int level)
    {
        AddForcePedestal forcePedestal = pedestal as AddForcePedestal;
        if (forcePedestal == null)
        {
            Debug.LogError("Add Force Activatable objects must be powered by their respective pedestals!");
            return;
        }

        currentDirection += Vector3.Normalize(forcePedestal.worldForceVector) * forcePedestal.forceDelta;
    }

    protected override void OnDepowered(Pedestal pedestal, int level)
    {
        AddForcePedestal forcePedestal = pedestal as AddForcePedestal;
        if (forcePedestal == null)
        {
            Debug.LogError("Add Force Activatable objects must be powered by their respective pedestals!");
            return;
        }

        if (level == 0)
        {
            currentDirection = Vector3.zero;
        }
        else
        {
            currentDirection -= Vector3.Normalize(forcePedestal.worldForceVector) * forcePedestal.forceDelta;
        }
    }

    private void FixedUpdate()
    {
        controlledBody.AddForce(currentDirection);
    }
}
