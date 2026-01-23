using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class HoverFloat : MonoBehaviour
{
    public float floatHeight = 0.05f;
    public float floatSpeed = 8f;

    Rigidbody rb;
    ReturnToOrigin returner;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    Vector3 originalPos;
    Vector3 targetPos;
    bool hoverActive = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        returner = GetComponent<ReturnToOrigin>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        originalPos = rb.position;
        targetPos = originalPos;
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
            Time.fixedDeltaTime * floatSpeed
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

        originalPos = rb.position;
        targetPos = originalPos + Vector3.up * floatHeight;

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

        targetPos = originalPos;
        rb.useGravity = true;
        hoverActive = false;
    }
}
