using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomCamera : MonoBehaviour
{
    public Camera mainCamera;
    public float newFieldOfView;
    public float zoomSpeed = 1.0f;
    private float oldFieldOfView;
    private float targetFieldOfView;
    // Start is called before the first frame update
    void Start()
    {
        oldFieldOfView = mainCamera.fieldOfView;
        targetFieldOfView = oldFieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFieldOfView, Time.deltaTime * zoomSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            targetFieldOfView = newFieldOfView;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            targetFieldOfView = oldFieldOfView;
        }
    }
}
