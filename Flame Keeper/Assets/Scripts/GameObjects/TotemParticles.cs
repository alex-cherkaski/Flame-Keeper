using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemParticles : MonoBehaviour
{
    public ParticleSystem particles;
    public GameObject totem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (totem.transform.position.y > -9.7f)
        {
            particles.Stop();
        }
        else if (totem.transform.position.y <= -9.7f && !particles.isPlaying)
        {
            particles.Play();
        }
        //Debug.Log(this.transform.parent.position.y);
        //Debug.Log(totem.transform.position.y);
    }
}
