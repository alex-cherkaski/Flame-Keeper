using UnityEngine;

public class CrystalScript : MonoBehaviour
{
    public float warmth = 10;
    public float rotateSpeed = 1.0f;
    public ParticleSystem particles;

    // Update is called once per frame
    void Update()
    {
        float angle = 45 * Time.deltaTime * rotateSpeed;
        this.transform.Rotate(0, angle, 0, Space.Self);
    }

    public float GetWarmth()
    {
        return warmth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            particles.Stop();
        }   
    }
}
