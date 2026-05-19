using UnityEngine;
using System.Collections;

public class PopupFader : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowPopup(float duration = 2f)
    {
        // Stop any current fading to prevent glitches
        StopAllCoroutines();
        StartCoroutine(FadeSequence(duration));
    }

    private IEnumerator FadeSequence(float visibleTime)
    {
        // 1. Fade In
        yield return StartCoroutine(Fade(0, 1, 0.5f));

        // 2. Wait
        yield return new WaitForSeconds(visibleTime);

        // 3. Fade Out
        yield return StartCoroutine(Fade(1, 0, 0.5f));
    }

    private IEnumerator Fade(float start, float end, float lerpTime)
    {
        float timeStartedLerping = Time.time;
        float timeSinceStarted = 0;

        while (timeSinceStarted <= lerpTime)
        {
            timeSinceStarted = Time.time - timeStartedLerping;
            float percentageComplete = timeSinceStarted / lerpTime;

            canvasGroup.alpha = Mathf.Lerp(start, end, percentageComplete);
            yield return null;
        }

        canvasGroup.alpha = end;
    }
}