using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GunFireGate : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public Camera playerCamera;

    [Header("Dealer (Hover Based)")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable dealerInteractable;

    [Header("Self Direction Check")]
    [Range(-1f, 1f)]
    public float selfFireDotThreshold = 0.8f;

    [Header("Fire Components")]
    public PlayQuickSound playQuickSound;
    public ShotgunFireController shotgunFireController;
    public MuzzleFlashLight muzzleFlashFire;

    [Header("Debug")]
    public bool drawDebugRay = true;

    // XR Grab Interactable -> Activate 事件 绑定这个
    public void OnActivate()
    {
        if (!CanFire())
            return;

        Fire();
    }

    bool CanFire()
    {
        return IsAimingAtSelf() || IsHoveringDealer();
    }

    #region Self Check
    bool IsAimingAtSelf()
    {
        if (muzzle == null || playerCamera == null)
            return false;

        Vector3 gunForward = muzzle.forward.normalized;
        Vector3 toHead = (playerCamera.transform.position - muzzle.position).normalized;

        float dot = Vector3.Dot(gunForward, toHead);

        if (drawDebugRay)
        {
            Debug.DrawRay(muzzle.position, gunForward * 0.5f, Color.red);
            Debug.DrawRay(muzzle.position, toHead * 0.5f, Color.green);
        }

        return dot > selfFireDotThreshold;
    }
    #endregion

    #region Dealer Hover Check
    bool IsHoveringDealer()
    {
        if (dealerInteractable == null)
            return false;

        return dealerInteractable.isHovered;
    }
    #endregion

    #region Fire Logic
    void Fire()
    {
        // 1️⃣ 声音
        if (playQuickSound != null)
            playQuickSound.Play();

        // 2️⃣ 枪口火光
        if (muzzleFlashFire != null)
            muzzleFlashFire.Fire();

        // 3️⃣ 原有开火逻辑（弹药 / 回座 / 冷却 / 锁定等）
        if (shotgunFireController != null)
            shotgunFireController.OnFireActivated();

        // Debug.Log("🔥 Shotgun Fired (Validated Target)");
    }
    #endregion
}
