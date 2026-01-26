using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverFloat : MonoBehaviour
{
    public float floatHeight = 0.05f;
    public float floatSpeed = 8f;

    Rigidbody rb;
    ReturnToOrigin returner;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    Vector3 referencePos;   // 🔒 固定参考高度
    Vector3 targetPos;

    bool hoverActive = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        returner = GetComponent<ReturnToOrigin>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // ✅ 只在 Awake / Start 记录一次
        referencePos = rb.position;
        targetPos = referencePos;
    }

    void Update()
    {
        // 🔒 Return / Fire 系统在控制时，Hover 完全让路
        if (returner != null && returner.IsBusy)
            return;

        if (!hoverActive)
            return;

        Vector3 newPos = Vector3.Lerp(
            rb.position,
            targetPos,
            Time.deltaTime * floatSpeed
        );

        rb.MovePosition(newPos);
    }

    public void OnHoverEntered()
    {
        // ❌ 被抓 / 被锁 / XR 被禁用 → 不浮
        if ((grab != null && grab.isSelected) ||
            (returner != null && returner.IsBusy) ||
            (grab != null && !grab.enabled))
            return;

        // ❗ 不再改 referencePos
        targetPos = referencePos + Vector3.up * floatHeight;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        hoverActive = true;
    }

    public void OnHoverExited()
    {
        // 🔒 Fire / Return 中，忽略 Hover Exit
        if (returner != null && returner.IsBusy)
            return;

        targetPos = referencePos;
        rb.useGravity = true;
        hoverActive = false;
    }

    /* ================= 对外接口 ================= */

    // 🔁 给 ReturnToOrigin 用：真正“落位”后刷新基准高度
    public void ResetReferencePos()
    {
        referencePos = rb.position;
        targetPos = referencePos;
    }
}
