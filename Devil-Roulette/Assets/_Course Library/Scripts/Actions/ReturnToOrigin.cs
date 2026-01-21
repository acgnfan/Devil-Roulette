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

        grab.enabled = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void UnlockAll()
    {
        isBusy = false;
        allowAutoReturn = true;

        rb.isKinematic = false;
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

        transform.position = originPos;
        transform.rotation = originRot;

        UnlockAll();
    }
}
