using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textComponent;
    
    [Header("Colors")]
    // Blood Red
    public Color normalColor = new Color(0.6f, 0f, 0f); 
    // Brighter Red
    public Color hoverColor = new Color(1f, 0.2f, 0.2f); 

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        textComponent.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textComponent.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textComponent.color = normalColor;
    }
}