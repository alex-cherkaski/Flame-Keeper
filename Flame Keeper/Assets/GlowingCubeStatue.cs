using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowingCubeStatue : MonoBehaviour
{
    private GameObject audioController;

    // Start is called before the first frame update
    void Start()
    {
        audioController = (GameObject)Instantiate(Resources.Load("audioController"), this.transform.position, this.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.water6);
        }
    }
}
