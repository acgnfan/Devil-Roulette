using UnityEngine;
using TMPro;
using System.Collections;

public class FadeHintUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.25f;

    Coroutine fadeCoroutine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
    }

    public void ShowHint(string msg)
    {
        text.text = msg;
        FadeTo(1f);
    }

    public void HideHint()
    {
        FadeTo(0f);
    }

    void FadeTo(float target)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(target));
    }

    IEnumerator FadeRoutine(float target)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
