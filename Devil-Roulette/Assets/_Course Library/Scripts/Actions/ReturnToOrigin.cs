using System.Collections;
using UnityEngine;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ReturnToOrigin : MonoBehaviour
{
    public float returnDuration = 1.0f;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    Rigidbody rb;

    Vector3 originPos;
    Quaternion originRot;

    bool isBusy;               
    bool allowAutoReturn = true;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        originPos = transform.position;
        originRot = transform.rotation;
    }

    /* ================= 公共接口 ================= */

    public bool IsBusy => isBusy;

    public void LockAll()
    {
        isBusy = true;
        allowAutoReturn = false;

        // 立刻切断 XR & 物理（关键）
        if (grab.isSelected && grab.firstInteractorSelecting != null)
        {
            grab.interactionManager.SelectExit(
                grab.firstInteractorSelecting,
                grab
            );
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        grab.enabled = false;
        rb.isKinematic = true;
    }

    public void UnlockAll()
    {
        isBusy = false;
        allowAutoReturn = true;

        rb.isKinematic = false;
        rb.useGravity = true;
        grab.enabled = true;
    }

    public void TryAutoReturn()
    {
        if (!allowAutoReturn || isBusy) return;
        StartCoroutine(ReturnRoutine());
    }

    public void ForceReturn()
    {
        if (isBusy)
            StartCoroutine(ReturnRoutine());
    }

    /* ================= 核心返回逻辑 ================= */

    IEnumerator ReturnRoutine()
{
    LockAll(); // 确保 kinematic 已经开启

    Vector3 startPos = transform.position;
    Quaternion startRot = transform.rotation;

    float t = 0f;
    while (t < returnDuration)
    {
        float n = t / returnDuration;
        transform.position = Vector3.Lerp(startPos, originPos, n);
        transform.rotation = Quaternion.Slerp(startRot, originRot, n);
        t += Time.deltaTime;
        yield return null;
    }

    // 确保最终位置精确
    transform.position = originPos;
    transform.rotation = originRot;

    // **保持 kinematic 一段时间，等 physics settle**
    rb.isKinematic = true;
    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;

    yield return new WaitForSeconds(0.05f);

    // 再解锁
    UnlockAll();
}

}
