using System.Collections;
using UnityEngine;

public class DealerGunInteractor : MonoBehaviour
{
    [Header("References")]
    public Transform dealerRoot;
    public Transform leftHand;
    public Transform rightHand;
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;
    public Transform gun;

    [Header("Gun Targets")]
    public Transform leftHandTarget;   // 枪栓 attach 点
    public Transform rightHandTarget;  // 扳机 attach 点
    public Transform gunTargetFront;   // 枪最终在dealer面前的位置和旋转

    [Header("Settings")]
    public float handMoveDuration = 0.25f;
    public float gunMoveDuration = 0.4f;
    public float handGripDuration = 0.25f;

    /// <summary>
    /// 播放dealer拿起枪动画
    /// </summary>
    public IEnumerator PlayPickupAnimation()
    {
        // --------------------------
        // 1️⃣ 左右手移动到枪上
        // --------------------------
        Vector3 leftFrom = leftHand.position;
        Quaternion leftRotFrom = leftHand.rotation;

        Vector3 rightFrom = rightHand.position;
        Quaternion rightRotFrom = rightHand.rotation;

        Vector3 leftTo = leftHandTarget.position;
        Quaternion leftRotTo = leftHandTarget.rotation;

        Vector3 rightTo = rightHandTarget.position;
        Quaternion rightRotTo = rightHandTarget.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / handMoveDuration;
            leftHand.position = Vector3.Lerp(leftFrom, leftTo, t);
            leftHand.rotation = Quaternion.Slerp(leftRotFrom, leftRotTo, t);

            rightHand.position = Vector3.Lerp(rightFrom, rightTo, t);
            rightHand.rotation = Quaternion.Slerp(rightRotFrom, rightRotTo, t);
            yield return null;
        }

        leftHand.position = leftTo;
        leftHand.rotation = leftRotTo;

        rightHand.position = rightTo;
        rightHand.rotation = rightRotTo;

        // --------------------------
        // 2️⃣ 临时把手挂到枪上
        // --------------------------
        leftHand.SetParent(gun, true);  // true = 保持世界位置
        rightHand.SetParent(gun, true);

        // --------------------------
        // 3️⃣ 同时抓握动画
        // --------------------------
        leftHandAnimator.SetFloat("Grip", 1f);
        rightHandAnimator.SetFloat("Grip", 1f);

        // --------------------------
        // 4️⃣ 枪移动到dealer面前
        // --------------------------
        Vector3 gunFrom = gun.position;
        Quaternion gunFromRot = gun.rotation;
        Vector3 gunTo = gunTargetFront.position;
        Quaternion gunToRot = gunTargetFront.rotation;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / gunMoveDuration;
            gun.position = Vector3.Lerp(gunFrom, gunTo, t);
            gun.rotation = Quaternion.Slerp(gunFromRot, gunToRot, t);
            yield return null;
        }

        gun.position = gunTo;
        gun.rotation = gunToRot;

        // --------------------------
        // Pickup动画完成，手可保持挂在枪上
        // --------------------------
    }
}
