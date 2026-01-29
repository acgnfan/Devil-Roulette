using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFaderGame : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;
    public float originalAlpha = 181f / 255f;

    private void Start()
    {
        // 游戏场景进入：淡入 + 播放游戏 BGM
        fadeCanvasGroup.alpha = 1f;
        StartCoroutine(FadeIn(1.0f));
        BGMManager.Instance.PlayGameBGM();
    }

    // 游戏结束时调用
    public void ReturnToMenu()
    {
        StartCoroutine(FadeAndReturnToMenu());
    }

    private IEnumerator FadeAndReturnToMenu()
    {
        // 2️⃣ 停游戏 BGM
        BGMManager.Instance.StopGameBGM();

        // 3️⃣ 加载菜单场景
        AsyncOperation asyncLoad =
            SceneManager.LoadSceneAsync("Create-with-VR-Starter-Scene");
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;
    }

    public IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(15.0f);
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha =
                Mathf.Lerp(originalAlpha, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha =
                Mathf.Lerp(1f, originalAlpha, timer / time);
            yield return null;
        }
        fadeCanvasGroup.alpha = originalAlpha;
    }

    public IEnumerator TurnBlack() // 变黑 
    {
        fadeCanvasGroup.alpha = 1f; yield return new WaitForSeconds(2f);
    }
}
