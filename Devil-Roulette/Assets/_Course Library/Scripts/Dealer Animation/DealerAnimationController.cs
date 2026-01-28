using UnityEngine;
using System.Collections;

public class DealerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Transform dealerRoot;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Grab Anchors (LOCAL)")]
    public Transform grabAnchorLeft;
    public Transform grabAnchorRight;

    [Header("Settings")]
    public float flyBackDistance = 0.6f;
    public float flyBackDuration = 0.25f;

    public float handReachDuration = 0.25f;
    public float recoverDuration = 0.4f;
    public float handResetDuration = 0.25f;

    [Header("Optional")]
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    // ---- cached ----
    Vector3 rootStartPos;
    Vector3 leftHandStartLocalPos;
    Vector3 rightHandStartLocalPos;

    public bool isdead = false;

    void Awake()
    {
        rootStartPos = dealerRoot.position;

        leftHandStartLocalPos = leftHand.localPosition;
        rightHandStartLocalPos = rightHand.localPosition;
    }

    // =========================
    // PUBLIC API
    // =========================

    /// <summary>
    /// Dealer 被击中后向后飞（不管死没死都会播）
    /// </summary>
    public IEnumerator PlayHitFlyBack()
    {
        Vector3 from = dealerRoot.position;
        Vector3 to = from + dealerRoot.forward * flyBackDistance;

        yield return MoveWorld(dealerRoot, from, to, flyBackDuration);
    }

    /// <summary>
    /// Dealer 还活着 → 抓桌边拉回
    /// </summary>
    public IEnumerator PlayRecoverIfAlive()
    {
        // 1️⃣ 手脱离dealerRoot
        leftHand.parent = null;
        rightHand.parent = null;

        // 2️⃣ 双手抓桌
        yield return MoveHandsToGrab();

        // 3️⃣ 抓握动画
        SetHandGrip(1f);

        // 4️⃣ dealerRoot 返回原位，手不动
        yield return MoveWorld(dealerRoot, dealerRoot.position, rootStartPos, recoverDuration);

        // 5️⃣ 松手
        SetHandGrip(0f);

        // 6️⃣ 把手重新绑回dealerRoot，并回到原始local位置
        leftHand.parent = dealerRoot;
        rightHand.parent = dealerRoot;
        yield return ResetHandsToLocal();
    }


    // =========================
    // INTERNAL SEQUENCES
    // =========================

    IEnumerator MoveHandsToGrab()
    {
        Vector3 lFrom = leftHand.position;
        Vector3 rFrom = rightHand.position;

        Vector3 lTo = grabAnchorLeft.position;
        Vector3 rTo = grabAnchorRight.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / handReachDuration;
            leftHand.position = Vector3.Lerp(lFrom, lTo, t);
            rightHand.position = Vector3.Lerp(rFrom, rTo, t);
            yield return null;
        }
    }

    IEnumerator ResetHandsToLocal()
    {
        Vector3 lFrom = leftHand.position;
        Vector3 rFrom = rightHand.position;

        Vector3 lTo = dealerRoot.TransformPoint(leftHandStartLocalPos);
        Vector3 rTo = dealerRoot.TransformPoint(rightHandStartLocalPos);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / handResetDuration;
            leftHand.position = Vector3.Lerp(lFrom, lTo, t);
            rightHand.position = Vector3.Lerp(rFrom, rTo, t);
            yield return null;
        }
    }

    IEnumerator MoveWorld(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            target.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }

    void SetHandGrip(float grip)
    {
        if (leftHandAnimator)
            leftHandAnimator.SetFloat("Grip", grip);

        if (rightHandAnimator)
            rightHandAnimator.SetFloat("Grip", grip);
    }
}
