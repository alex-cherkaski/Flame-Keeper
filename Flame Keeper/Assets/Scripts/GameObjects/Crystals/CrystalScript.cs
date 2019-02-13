using UnityEngine;

public class CrystalScript : MonoBehaviour
{
    public float warmth = 10;
    public float rotateSpeed = 1.0f;

    // Update is called once per frame
    void Update()
    {
        float angle = 45 * Time.deltaTime * rotateSpeed;
        this.transform.Rotate(angle, angle, angle, Space.Self);
    }

    public float GetWarmth()
    {
        return warmth;
    }
}
