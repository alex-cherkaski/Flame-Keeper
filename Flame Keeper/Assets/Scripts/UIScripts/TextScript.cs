using UnityEngine;

public class TextScript : MonoBehaviour
{
    public Animator animator;

    public void FadeOut()
    {
        animator.SetTrigger("FadeOut");
        Invoke("Deactivate", 1.1f);
    }

    private void Deactivate()
    {
        this.gameObject.SetActive(false);
    }
}
