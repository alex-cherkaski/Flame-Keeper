using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseOptionElement : MonoBehaviour
{
    public PauseOptionType type;

    public TextMeshProUGUI optionText;
    public Image selectorImage;

    public Color selectedColor;
    public Color deselectedColor;

    public void SetState(bool selected)
    {
        optionText.color = selected ? selectedColor : deselectedColor;
        selectorImage.gameObject.SetActive(selected);
    }
}
