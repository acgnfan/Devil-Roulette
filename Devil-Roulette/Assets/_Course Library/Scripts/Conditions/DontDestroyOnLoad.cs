using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Setting")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f; // 淡入淡出耗时（秒）

    public float originalAlpha = 181f / 255f; // 初始透明度（0~1）

    private void Awake()
    {
        // 单例模式：确保全局只有一个 Fader
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 游戏刚开始时，执行一次淡入（从黑变亮）
        float counter = 0f;
        while (counter < 2f)
        {
            counter += Time.deltaTime * 2f;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, originalAlpha, counter / 2f);
        }
        fadeCanvasGroup.alpha = originalAlpha;
    }

    // 供外部按钮调用的公共方法
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    // 核心协程：淡出 -> 加载 -> 淡入
    private IEnumerator FadeAndLoad(string sceneName)
    {
        // 1. 淡出（变黑）
        yield return StartCoroutine(FadeOut());

        // 2. 异步加载场景（防止卡顿）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // 加载完先别急着显示

        // 等待加载进度接近完成
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 允许场景激活
        asyncLoad.allowSceneActivation = true;
        
        // 等待一帧让新场景初始化
        yield return null; 

        // 3. 淡入（变亮）
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut() // 变黑
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(originalAlpha, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator TurnBlack() // 变黑
    {
        fadeCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator FadeIn() // 变透明
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, originalAlpha, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = originalAlpha;
    }

}