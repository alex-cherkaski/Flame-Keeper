using UnityEngine;

public class CrystalScript : MonoBehaviour
{
    public float warmth = 10;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(45 * Time.deltaTime, 45 * Time.deltaTime, 45 * Time.deltaTime, Space.Self);
    }

    public float GetWarmth()
    {
        return warmth;
    }
}
