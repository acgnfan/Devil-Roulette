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
            DontDestroyOnLoad(gameObject);
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
        StartCoroutine(FadeIn(2.0f));
        BGMManager.Instance.PlayMenuBGM();
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
        if (sceneName == "Game Scene")
        {
            yield return StartCoroutine(FadeOut());
        }

        if (sceneName == "Create-with-VR-Starter-Scene")
        {
            BGMManager.Instance.StopGameBGM();
        }
        else if (sceneName == "Game Scene")
        {
            BGMManager.Instance.StopMenuBGM();
        }

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
        yield return StartCoroutine(FadeIn(1.0f));

        if (sceneName == "Create-with-VR-Starter-Scene")
        {
            BGMManager.Instance.PlayMenuBGM();
        }
        else if (sceneName == "Game Scene")
        {
            BGMManager.Instance.PlayGameBGM();
        }
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

    public IEnumerator FadeIn(float fadeTime) // 变透明
    {
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, originalAlpha, timer / fadeTime);
            yield return null;
        }
        fadeCanvasGroup.alpha = originalAlpha;
    }

}