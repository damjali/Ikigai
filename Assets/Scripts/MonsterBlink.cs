using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class MonsterBlink : MonoBehaviour
{
    private Light2D monsterLight;
    private Coroutine blinkCoroutine;

    [Header("Blink Settings")]
    public float fadeDuration = 0.8f; // How long it takes to fade out
    public float normalIntensity = 0f;
    public float dimIntensity = 0f;

    void Awake()
    {
        monsterLight = GetComponent<Light2D>();
        //Xayne typeshi
        //Hazim was here
    }

    public void Blink()
    {
        if (blinkCoroutine != null) return;
        blinkCoroutine = StartCoroutine(FadeRoutine());
    }

    public void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        monsterLight.intensity = 0f;
    }

    private IEnumerator FadeRoutine()
    {
        while (true)
        {
            // 1. Instant "Pop" to full brightness
            monsterLight.intensity = normalIntensity;
            

            // 2. Slow Fade Out
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                
                // Calculate how far we are through the fade (0.0 to 1.0)
                float t = elapsed / fadeDuration;

                // Lerp from normal (5f) to dim (0f)
                monsterLight.intensity = Mathf.Lerp(normalIntensity, dimIntensity, t);
                
                yield return null; // Wait for the next frame
            }

            // Optional: Small pause at the bottom before popping back up
            yield return new WaitForSeconds(0.1f);
        }
    }
}