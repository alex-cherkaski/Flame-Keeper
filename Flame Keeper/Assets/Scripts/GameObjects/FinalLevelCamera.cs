using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalLevelCamera : MonoBehaviour
{
    public Camera levelCamera;
    public Light pedestalSpotlight;

    // Z values defining the interval of camera animation
    public float startingZValue = 40.0f;
    public float endingZValue = -3.0f;

    // Change in camera depth as user progresses through hallway
    public float startingCameraDepth = 10.0f;
    public float endingCameraDepth = 40.0f;

    // Change in camera height as user progresses through hallway
    public float startingCameraHeight = 3.2f;
    public float endingCameraHeight = 15.0f;

    // Change in pedestals spotlight intensity
    public float startingSpotlightIntensity = 10.0f;
    public float endingSpotlightIntensity = 2.0f;

    void Start()
    {
        FlameKeeper.Get().levelController.GetPlayer().DisableRotation();
    }

    private void Update()
    {
        PlayerControllerSimple player = FlameKeeper.Get().levelController.GetPlayer();
        float playerZ = player.transform.position.z;
        float t = Mathf.Clamp((playerZ - startingZValue) / (endingZValue - startingZValue), 0.0f, 1.0f);
        float depth = Mathf.Lerp(startingCameraDepth, endingCameraDepth, t);
        float height = Mathf.Lerp(startingCameraHeight, endingCameraHeight, t);
        levelCamera.transform.localPosition = new Vector3(levelCamera.transform.localPosition.x, height, depth);
        pedestalSpotlight.intensity = Mathf.Lerp(startingSpotlightIntensity, endingSpotlightIntensity, t);
    }
}
