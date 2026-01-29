using System.Collections;
using UnityEngine;

public class DealerGunInteractor : MonoBehaviour
{
    [Header("Dealer")]
    public Transform dealerRoot;

    [Header("Hands")]
    public Transform leftHand;
    public Transform rightHand;
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    [Header("Gun")]
    public Transform gun;
    public Rigidbody gunRb;
    public ReturnToOrigin gunReturn;
    public GunFireGate gunFireGate;

    [Header("Gun Attach Points")]
    public Transform leftHandTarget;   // 枪栓
    public Transform rightHandTarget;  // 扳机

    [Header("Gun Aim Targets")]
    public Transform gunTargetFront;
    public Transform gunTargetSelf;
    public Transform gunTargetPlayer;

    [Header("Timings")]
    public float handMoveDuration = 0.25f;
    public float gunMoveDuration = 0.4f;
    public float handResetDuration = 0.25f;

    // ================= cached =================
    Vector3 leftHandStartPos;
    Quaternion leftHandStartRot;
    Vector3 rightHandStartPos;
    Quaternion rightHandStartRot;
    Transform leftHandOriginalParent;
    Transform rightHandOriginalParent;

    void Awake()
    {
        leftHandStartPos = leftHand.position;
        leftHandStartRot = leftHand.rotation;

        rightHandStartPos = rightHand.position;
        rightHandStartRot = rightHand.rotation;

        leftHandOriginalParent = leftHand.parent;
        rightHandOriginalParent = rightHand.parent;
    }

    // =========================================================
    // 1️⃣ Dealer 拿起枪
    // =========================================================
    public IEnumerator PlayPickupAnimation()
    {
        // 锁物理
        gunReturn.LockAll();

        // 手移动到枪
        yield return MoveHandsToTargets(leftHandTarget, rightHandTarget);

        // 手挂到枪上
        leftHand.SetParent(gun, true);
        rightHand.SetParent(gun, true);

        // 抓握
        SetHandGrip(1f);

        yield return new WaitForSeconds(0.5f);

        // 枪移动到 dealer 面前
        yield return MoveGunTo(gunTargetFront);
    }

    // =========================================================
    // 2️⃣ 枪指向自己
    // =========================================================
    public IEnumerator AimGunAtSelf()
    {
        yield return MoveGunTo(gunTargetSelf);
    }

    // =========================================================
    // 3️⃣ 枪指向玩家
    // =========================================================
    public IEnumerator AimGunAtPlayer()
    {
        yield return MoveGunTo(gunTargetPlayer);
    }

    // =========================================================
    // 4️⃣ 放下枪（重点）
    // =========================================================
    public IEnumerator PlayPutDownGun()
    {
        // 1️⃣ 松手
        SetHandGrip(0f);
        yield return new WaitForSeconds(0.1f);

        // 2️⃣ 手解除枪的 parent
        leftHand.SetParent(null, true);
        rightHand.SetParent(null, true);

        // 3️⃣ 手回到初始位置
        yield return MoveHandsBackToStart();

        // 4️⃣ 手重新挂回 dealer 本体
        leftHand.SetParent(leftHandOriginalParent, true);
        rightHand.SetParent(rightHandOriginalParent, true);

        // 5️⃣ 枪回到桌面
        gunReturn.ForceReturn();
    }

    public void EmergencyPutDownGun()
    {
        SetHandGrip(0f);

        leftHand.SetParent(null, true);
        rightHand.SetParent(null, true);

        leftHand.SetPositionAndRotation(leftHandStartPos, leftHandStartRot);
        rightHand.SetPositionAndRotation(rightHandStartPos, rightHandStartRot);

        leftHand.SetParent(leftHandOriginalParent, true);
        rightHand.SetParent(rightHandOriginalParent, true);

        gunReturn.ForceReturn();
    }

    public void DealerActivateGun(bool isLive, bool aimingPlayer)
    {
        gunFireGate.Fire(isLive, aimingPlayer);
    }

    // =========================================================
    // =================== INTERNAL ============================
    // =========================================================

    IEnumerator MoveHandsToTargets(Transform leftTarget, Transform rightTarget)
    {
        Vector3 lFromPos = leftHand.position;
        Quaternion lFromRot = leftHand.rotation;
        Vector3 rFromPos = rightHand.position;
        Quaternion rFromRot = rightHand.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / handMoveDuration;

            leftHand.position = Vector3.Lerp(lFromPos, leftTarget.position, t);
            leftHand.rotation = Quaternion.Slerp(lFromRot, leftTarget.rotation, t);

            rightHand.position = Vector3.Lerp(rFromPos, rightTarget.position, t);
            rightHand.rotation = Quaternion.Slerp(rFromRot, rightTarget.rotation, t);

            yield return null;
        }

        leftHand.SetPositionAndRotation(leftTarget.position, leftTarget.rotation);
        rightHand.SetPositionAndRotation(rightTarget.position, rightTarget.rotation);
    }

    IEnumerator MoveHandsBackToStart()
    {
        Vector3 lFromPos = leftHand.position;
        Quaternion lFromRot = leftHand.rotation;
        Vector3 rFromPos = rightHand.position;
        Quaternion rFromRot = rightHand.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / handResetDuration;

            leftHand.position = Vector3.Lerp(lFromPos, leftHandStartPos, t);
            leftHand.rotation = Quaternion.Slerp(lFromRot, leftHandStartRot, t);

            rightHand.position = Vector3.Lerp(rFromPos, rightHandStartPos, t);
            rightHand.rotation = Quaternion.Slerp(rFromRot, rightHandStartRot, t);

            yield return null;
        }

        leftHand.SetPositionAndRotation(leftHandStartPos, leftHandStartRot);
        rightHand.SetPositionAndRotation(rightHandStartPos, rightHandStartRot);
    }

    IEnumerator MoveGunTo(Transform target)
    {
        Vector3 fromPos = gun.position;
        Quaternion fromRot = gun.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / gunMoveDuration;
            gun.position = Vector3.Lerp(fromPos, target.position, t);
            gun.rotation = Quaternion.Slerp(fromRot, target.rotation, t);
            yield return null;
        }

        gun.SetPositionAndRotation(target.position, target.rotation);
    }

    void SetHandGrip(float value)
    {
        if (leftHandAnimator)
            leftHandAnimator.SetFloat("Grip", value);
        if (rightHandAnimator)
            rightHandAnimator.SetFloat("Grip", value);
    }
}
