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
    public PlayQuickSound liveFireSound;          
    public PlayQuickSound blankFireSound; 
    public ShotgunFireController shotgunFireController;
    public MuzzleFlashLight muzzleFlashFire;

    [Header("Game Flow")]
    public GameState gameState;
    public GameFlowController gameFlowController;

    [Header("Debug")]
    public bool drawDebugRay = true;

    // XR Grab Interactable -> Activate 事件 绑定这个
    public void OnActivate()
    {
        // 判断目标
        bool aimingSelf = IsAimingAtSelf();
        bool aimingDealer = IsHoveringDealer();

        // 两者都不是 → 不允许开火
        if (!aimingSelf && !aimingDealer)
            return;

        GameState.ShellType shellType = gameState.GetCurrentChamberType();
        bool isLive = (shellType == GameState.ShellType.Live);

        // 先执行枪的物理 / 表现层开火
        Fire(isLive, shellType);

        // 再通知 Game Flow（逻辑层）
        if (gameFlowController != null)
        {
            if (aimingSelf)
            {
                gameFlowController.OnShootChoice(true);
            }
            else if (aimingDealer)
            {
                gameFlowController.OnShootChoice(false);
            }
        }
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
    void Fire(bool isLive, GameState.ShellType shellType)
    {
        if (isLive)
        {
            if (liveFireSound != null)
                liveFireSound.Play();

            if (muzzleFlashFire != null)
                muzzleFlashFire.Fire();
        }
        else if (shellType == GameState.ShellType.Blank)
        {
            if (blankFireSound != null)
                blankFireSound.Play();
        }
        else
        {
            Debug.LogWarning("⚠️ Tried to fire EMPTY chamber");
        }

        if (shotgunFireController != null)
            shotgunFireController.OnFireActivated();
    }
    #endregion
}
