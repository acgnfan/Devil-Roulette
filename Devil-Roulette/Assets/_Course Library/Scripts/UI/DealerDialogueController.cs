using System.Collections;
using UnityEngine;
using TMPro;

public class DealerDialogueController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text dialogueText;
    public CanvasGroup canvasGroup;

    [Header("Typewriter Settings")]
    public float charInterval = 0.03f;
    public float holdAfterFinish = 1.5f;

    [Header("Fade Settings")]
    public float fadeDuration = 0.25f;

    Coroutine currentRoutine;

    void Awake()
    {
        // 初始隐藏
        canvasGroup.alpha = 0f;
        dialogueText.text = "";
    }

    /// <summary>
    /// 对外调用的唯一接口
    /// </summary>
    public IEnumerator Say(string line)
    {
        // 打断之前的台词
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(SayRoutine(line));
        yield return currentRoutine;
    }

    IEnumerator SayRoutine(string line)
    {
        dialogueText.text = "";

        // Fade in
        yield return Fade(0f, 1f);

        // 打字机效果
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charInterval);
        }

        // 停留
        yield return new WaitForSeconds(holdAfterFinish);

        // Fade out
        yield return Fade(1f, 0f);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
