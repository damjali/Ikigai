using System.Collections;
using UnityEngine;
using TMPro; // 1. Add this namespace

public class Lvl1Canvas : MonoBehaviour
{
    // 2. Change 'Text' to 'TextMeshProUGUI'
    public TextMeshProUGUI text;
    
    public void TriggerPopup()
    {
        if (text == null) return; // Safety check
        StopAllCoroutines(); 
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        // 3. The rest of the logic stays the same!
        Color textColor = text.color;
        textColor.a = 1f;
        text.color = textColor;

        yield return new WaitForSeconds(1f);

        float duration = 2f;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(1f, 0f, currentTime / duration);
            text.color = textColor;
            yield return null;
        }

        textColor.a = 0f;
        text.color = textColor;
    }
}