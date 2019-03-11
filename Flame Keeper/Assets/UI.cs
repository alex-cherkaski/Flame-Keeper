using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public Animator animator;
    public bool billboard = true; // If true, will always be facing towards the camera

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (billboard)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }

    public void FadeOut()
    {
        Debug.Log("In Fade Out");
        animator.SetTrigger("FadeOut");
    }
}
