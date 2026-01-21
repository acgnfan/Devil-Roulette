using UnityEngine;
using System.Collections;

public class ShotgunFireController : MonoBehaviour
{
    public float floatTime = 1.0f;
    // public float kickbackDistance = 0.05f;

    ReturnToOrigin returner;
    bool firedOnce; // 🔒 防连发

    void Awake()
    {
        returner = GetComponent<ReturnToOrigin>();
    }

    public void OnFireActivated()
    {
        // 🚫 正在忙 or 已经开过火
        if (returner.IsBusy || firedOnce) return;

        firedOnce = true;

        // 🔒 立刻锁死一切（同一帧）
        returner.LockAll();

        // 后坐
        // transform.position -= transform.forward * kickbackDistance;

        StartCoroutine(FireSequence());
    }

    IEnumerator FireSequence()
    {
        // 悬停
        yield return new WaitForSeconds(floatTime);

        // 强制返回
        returner.ForceReturn();

        firedOnce = false;
    }
}
