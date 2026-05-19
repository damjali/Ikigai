using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HeartHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image imageComponent;
    private RectTransform rectTransform;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.7f, 0.7f);

    [Header("Heartbeat Settings")]
    public float beatInterval = 0.8f;   // time for one full heartbeat cycle
    public float beatScale = 1.25f;     // size of the beat

    private Vector3 originalScale;
    private bool isHovering = false;
    private float timer = 0f;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        imageComponent.color = normalColor;
        originalScale = rectTransform.localScale;
    }

    void Update()
    {
        if (isHovering)
        {
            timer += Time.deltaTime;

            float t = timer % beatInterval;

            float scale = 1f;

            // First beat (lub)
            if (t < 0.1f)
            {
                scale = Mathf.Lerp(1f, beatScale, t / 0.1f);
            }
            else if (t < 0.2f)
            {
                scale = Mathf.Lerp(beatScale, 1f, (t - 0.1f) / 0.1f);
            }
            // Second beat (dub)
            else if (t < 0.3f)
            {
                scale = Mathf.Lerp(1f, beatScale * 0.9f, (t - 0.2f) / 0.1f);
            }
            else if (t < 0.4f)
            {
                scale = Mathf.Lerp(beatScale * 0.9f, 1f, (t - 0.3f) / 0.1f);
            }
            // Rest phase
            else
            {
                scale = 1f;
            }

            rectTransform.localScale = originalScale * scale;
        }
        else
        {
            timer = 0f;

            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                originalScale,
                Time.deltaTime * 10f
            );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        imageComponent.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        imageComponent.color = normalColor;
    }
}