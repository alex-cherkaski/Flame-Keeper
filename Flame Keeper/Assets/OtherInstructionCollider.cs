using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherInstructionCollider : MonoBehaviour
{
    public PlayerControllerSimple player;
    public GameObject instructions;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            instructions.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            instructions.GetComponent<UI>().FadeOut();
            Invoke("Deactivate", 1.1f);
        }
    }

    private void Deactivate()
    {
        instructions.SetActive(false);
    }
}
