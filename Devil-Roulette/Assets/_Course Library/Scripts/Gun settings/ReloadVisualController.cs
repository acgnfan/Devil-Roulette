using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ReloadVisualController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject liveShellPrefab;
    [SerializeField] private GameObject blankShellPrefab;

    [Header("Spawn Points (Left → Right)")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 0.08f;

    [Header("Input")]
    [Tooltip("XR Controller A / Primary Button")]
    [SerializeField] private InputActionReference confirmAction;

    [Header("Audio")]
    [SerializeField] private PlayQuickSound reloadBulletSound;
    [SerializeField] private PlayQuickSound LoadSound;

    [Header("References")]
    public DealerGunInteractor dealerGun;
    public DealerDialogueController dealerDialogue;


    List<GameObject> spawnedShells = new List<GameObject>();
    bool waitingForConfirm = false;

    void OnEnable()
    {
        if (confirmAction != null)
            confirmAction.action.Enable();
    }

    void OnDisable()
    {
        if (confirmAction != null)
            confirmAction.action.Disable();
    }

    public IEnumerator PlayReloadVisual(int liveCount, int blankCount, int reloadTime)
    {
        yield return StartCoroutine(ReloadSequence(liveCount, blankCount, reloadTime));
    }

    IEnumerator ReloadSequence(int liveCount, int blankCount, int reloadTime)
    {
        ClearShells();
        waitingForConfirm = false;

        int spawnIndex = 0;

        // ① 先生成空弹
        for (int i = 0; i < blankCount; i++)
        {
            if (spawnIndex >= spawnPoints.Length)
                break;

            Spawn(spawnPoints[spawnIndex], blankShellPrefab);
            spawnIndex++;
            yield return new WaitForSeconds(spawnInterval);
        }

        // ② 再生成实弹
        for (int i = 0; i < liveCount; i++)
        {
            if (spawnIndex >= spawnPoints.Length)
                break;

            Spawn(spawnPoints[spawnIndex], liveShellPrefab);
            spawnIndex++;
            yield return new WaitForSeconds(spawnInterval);
        }

        // ③ 等待玩家确认
        waitingForConfirm = true;
        // yield return new WaitUntil(PlayerConfirmed);
        yield return dealerDialogue.Say($"{liveCount} live shells, {blankCount} blanks.");
        yield return new WaitForSeconds(1.5f);

        // ④ 确认后：子弹一起消失
        ClearShells();

        yield return dealerGun.PlayPickupAnimation();
        if (reloadTime == 1)
        {
            yield return dealerDialogue.Say("I insert the shells in an unknown order.");
        }
        else if (reloadTime == 2)
        {
            yield return dealerDialogue.Say("They enter the chamber in a hidden sequence.");
        }

        // ⑤ 播放装弹音效（确认完成）
        if (reloadBulletSound != null && LoadSound != null)
        {
            for (int i = 0; i < liveCount + blankCount; i++)
            {
                reloadBulletSound.Play();
                yield return new WaitForSeconds(0.235f);
            }  
            LoadSound.Play();
            yield return new WaitForSeconds(1.0f);
        }

        yield return dealerGun.PlayPutDownGun();
    }

    bool PlayerConfirmed()
    {
        if (!waitingForConfirm)
            return false;

        if (confirmAction == null)
            return false;

        return confirmAction.action.WasPressedThisFrame();
    }

    void Spawn(Transform point, GameObject prefab)
    {
        if (point == null || prefab == null)
            return;

        GameObject shell = Instantiate(
            prefab,
            point.position,
            point.rotation
        );

        spawnedShells.Add(shell);
    }

    void ClearShells()
    {
        foreach (var shell in spawnedShells)
        {
            if (shell != null)
                Destroy(shell);
        }

        spawnedShells.Clear();
    }
}
