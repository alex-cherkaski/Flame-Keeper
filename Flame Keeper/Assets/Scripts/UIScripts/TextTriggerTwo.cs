using UnityEngine;

public class TextTriggerTwo : MonoBehaviour
{
    public GameObject text1;
    public GameObject text2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            text1.SetActive(true);
            text2.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.Player))
        {
            text1.GetComponent<TextScript>().FadeOut();
            text2.GetComponent<TextScript>().FadeOut();
        }
    }
}
