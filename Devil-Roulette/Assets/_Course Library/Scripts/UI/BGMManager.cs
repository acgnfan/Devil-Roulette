using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("Audio Sources")]
    public AudioSource menuBGMSource;
    public AudioSource gameBGMSource;

    [Header("Menu BGM")]
    public float menuVolume = 0.6f;

    [Header("Game BGM Volume")]
    public float gameBaseVolume = 0.2f;     // 正常状态（很轻）
    public float gameHitPeakVolume = 0.8f;  // 被击中瞬间
    public float hitFadeDownTime = 2.5f;    // 慢慢降回去的时间

    Coroutine gameVolumeRoutine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        menuBGMSource.loop = true;
        gameBGMSource.loop = true;

        menuBGMSource.volume = 0f;
        gameBGMSource.volume = gameBaseVolume;
    }

    // ===============================
    // Menu BGM
    // ===============================

    public void PlayMenuBGM()
    {
        StopGameBGM();
        menuBGMSource.volume = menuVolume;
        if (!menuBGMSource.isPlaying)
            menuBGMSource.Play();
    }

    public void StopMenuBGM()
    {
        menuBGMSource.Stop();
    }

    // ===============================
    // Game BGM
    // ===============================

    public void PlayGameBGM()
    {
        StopMenuBGM();

        gameBGMSource.volume = gameBaseVolume;
        if (!gameBGMSource.isPlaying)
            gameBGMSource.Play();
    }

    public void StopGameBGM()
    {
        if (gameVolumeRoutine != null)
            StopCoroutine(gameVolumeRoutine);

        gameBGMSource.Stop();
    }

    // ===============================
    // 🎯 玩家被击中 → 音量冲击
    // ===============================

    public void OnPlayerHit()
    {
        if (gameVolumeRoutine != null)
            StopCoroutine(gameVolumeRoutine);

        gameVolumeRoutine = StartCoroutine(HitVolumePulse());
    }

    IEnumerator HitVolumePulse()
    {
        // 1️⃣ 立刻拉高
        gameBGMSource.volume = gameHitPeakVolume;

        // 2️⃣ 缓慢回落
        float t = 0f;
        while (t < hitFadeDownTime)
        {
            t += Time.deltaTime;
            gameBGMSource.volume = Mathf.Lerp(
                gameHitPeakVolume,
                gameBaseVolume,
                t / hitFadeDownTime
            );
            yield return null;
        }

        gameBGMSource.volume = gameBaseVolume;
    }
}
