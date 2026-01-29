using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFaderMenu : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;
    public float originalAlpha = 181f / 255f;

    private void Start()
    {
        // 菜单场景启动：淡入 + 播放菜单 BGM
        fadeCanvasGroup.alpha = 1f;
        StartCoroutine(FadeIn(2.0f));
        BGMManager.Instance.PlayMenuBGM();
    }

    // UI Button 调用
    public void LoadGameScene()
    {
        StartCoroutine(FadeAndLoadGame());
    }

    private IEnumerator FadeAndLoadGame()
    {
        // 1️⃣ 淡出
        yield return StartCoroutine(FadeOut());

        // 2️⃣ 停菜单 BGM
        BGMManager.Instance.StopMenuBGM();

        // 3️⃣ 加载游戏场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game Scene");
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;
    }

    private IEnumerator FadeOut()
    {
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

    private IEnumerator FadeIn(float time)
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
