using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICollider : MonoBehaviour
{
    public PlayerControllerSimple player;

    public Pedestal pedestal;

    public GameObject giveFlame;
    public GameObject takeFlame;
    public GameObject giveOrTakeFlame;

    private int pastPedestalLevel;

    // Start is called before the first frame update
    void Start()
    {
        pastPedestalLevel = pedestal.GetCurrLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pastPedestalLevel = pedestal.GetCurrLevel();
            if (pedestal.GetCurrLevel() == 0)
            {
                giveFlame.SetActive(true);
            }
            else if (pedestal.GetCurrLevel() > 0 && pedestal.GetCurrLevel() < pedestal.maxLevel)
            {
                giveOrTakeFlame.SetActive(true);
            }
            else if (pedestal.GetCurrLevel() == pedestal.maxLevel)
            {
                takeFlame.SetActive(true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && pastPedestalLevel != pedestal.GetCurrLevel())
        {
            if (giveFlame.activeSelf)
            {
                //giveFlame.GetComponent<UI>().FadeOut();
                //Invoke("DeactivateGiveFlame", 1.1f);
                Invoke("DeactivateGiveFlame", 0f);
            }
            else if (takeFlame.activeSelf)
            {
                //takeFlame.GetComponent<UI>().FadeOut();
                //Invoke("DeactivateTakeFlame", 1.1f);
                Invoke("DeactivateTakeFlame", 0f);
            }
            else if (giveOrTakeFlame.activeSelf)
            {
                //giveOrTakeFlame.GetComponent<UI>().FadeOut();
                //Invoke("GiveOrTakeFlameDeactivate", 1.1f);
                Invoke("GiveOrTakeFlameDeactivate", 0f);
            }

            if (pedestal.GetCurrLevel() == 0)
            {
                giveFlame.SetActive(true);
            }
            else if (pedestal.GetCurrLevel() > 0 && pedestal.GetCurrLevel() < pedestal.maxLevel)
            {
                giveOrTakeFlame.SetActive(true);
            }
            else if (pedestal.GetCurrLevel() == pedestal.maxLevel)
            {
                takeFlame.SetActive(true);
            }

            pastPedestalLevel = pedestal.GetCurrLevel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (giveFlame.activeSelf)
            {
                giveFlame.GetComponent<UI>().FadeOut();
            }
            else if (takeFlame.activeSelf)
            {
                takeFlame.GetComponent<UI>().FadeOut();
            }
            else if (giveOrTakeFlame.activeSelf)
            {
                giveOrTakeFlame.GetComponent<UI>().FadeOut();
            }

            Invoke("Deactivate", 1.1f);
        }
    }

    private void Deactivate()
    {
        giveFlame.SetActive(false);
        takeFlame.SetActive(false);
        giveOrTakeFlame.SetActive(false);
    }

    private void DeactivateGiveFlame()
    {
        giveFlame.SetActive(false);
    }

    private void DeactivateTakeFlame()
    {
        takeFlame.SetActive(false);
    }

    private void GiveOrTakeFlameDeactivate()
    {
        giveOrTakeFlame.SetActive(false);
    }
}
