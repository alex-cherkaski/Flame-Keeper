using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITextCollider : MonoBehaviour
{
    public GameObject uiText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            uiText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            if (uiText.activeSelf)
            {
                uiText.GetComponent<UI>().FadeOut();
            }
        }

        Invoke("Deactivate", 1.1f);
    }

    private void Deactivate()
    {
        uiText.SetActive(false);
    }
}
