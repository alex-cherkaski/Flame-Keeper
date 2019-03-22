using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightUpAsPass : MonoBehaviour
{
    public float newRange;
    public float lightSpeed = 1.0f;
    public List<Light> lights;
    private bool passedThrough;
    // Start is called before the first frame update
    void Start()
    {
        passedThrough = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (passedThrough)
        {
            foreach (Light light in lights)
            {
                light.range = Mathf.Lerp(light.range, newRange, Time.deltaTime * lightSpeed);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            passedThrough = true;
        }
    }
}
