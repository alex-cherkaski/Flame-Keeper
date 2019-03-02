using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
    X = 0,
    Y = 1,
    Z = 2
}

public class MoveBackAndForth : MonoBehaviour
{
    public Axis moveAxis = Axis.Y;
    public float distance = 1.0f;
    public float speed = 1.0f;

    public Pedestal drainingWaterTrigger;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = this.transform.position;
    }

    private void Update()
    {
        Vector3 pos = startPosition;
        switch (moveAxis)
        {
            case (Axis.X):
                pos.x += Mathf.Sin(Time.realtimeSinceStartup * speed) * distance;
                break;
            case (Axis.Y):
                pos.y += Mathf.Sin(Time.realtimeSinceStartup * speed) * distance;
                break;
            case (Axis.Z):
                pos.z += Mathf.Sin(Time.realtimeSinceStartup * speed) * distance;
                break;
        }
        if (drainingWaterTrigger == null || (drainingWaterTrigger != null && drainingWaterTrigger.GetCurrLevel() == 0))
        {
            this.transform.position = pos;
        }
        
    }
}
