using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    public GameObject text;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            text.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            text.GetComponent<TextScript>().FadeOut();
        }
    }
}
