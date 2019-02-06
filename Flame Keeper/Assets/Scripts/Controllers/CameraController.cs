using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    private void Start()
    {

    }

    void Update()
    {
        // Keep the Camera holder where the player is.
        this.transform.position = player.transform.position;
    }
}
