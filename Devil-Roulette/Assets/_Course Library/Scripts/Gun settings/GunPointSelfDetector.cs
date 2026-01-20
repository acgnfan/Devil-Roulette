using UnityEngine;


public class GunPointSelfDetector : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public Transform playerCamera;
    public FadeHintUI hintUI;

    [Header("Angle Settings")]
    [Range(0.7f, 0.99f)]
    public float dotThreshold = 0.9f;   // 越大越严格

    [Header("Distance Settings")]
    public float maxDistance = 1.0f;    // 枪口到头部的最大距离

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    bool isShowing = false;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        // Debug.Log("GunPointSelfByAngle_Debug Awake: grab=" + (grab != null));
    }

    void Update()
    {
        // 1. 先打印变量是否为空
        // if (muzzle == null || playerCamera == null || hintUI == null)
        // {
        //     Debug.LogWarning("GunPointSelfByAngle_Debug: 绑定缺失 -> muzzle=" + muzzle + " camera=" + playerCamera + " hintUI=" + hintUI);
        //     return;
        // }

        // 2. 如果没抓住枪，不显示
        if (grab != null && !grab.isSelected)
        {
            if (isShowing) Hide();
            return;
        }

        Vector3 toCamera = playerCamera.position - muzzle.position;
        float distance = toCamera.magnitude;

        if (distance > maxDistance)
        {
            if (isShowing) Hide();
            return;
        }

        Vector3 gunForward = muzzle.forward.normalized;
        Vector3 dirToCamera = toCamera.normalized;
        float dot = Vector3.Dot(gunForward, dirToCamera);

        // Debug.DrawRay(muzzle.position, gunForward * 2f, Color.red);
        // Debug.DrawRay(muzzle.position, dirToCamera * 2f, Color.green);

        // 打印关键数值
        // Debug.Log("Dot=" + dot + " Dist=" + distance + " isSelected=" + (grab != null ? grab.isSelected : false));

        if (dot > dotThreshold)
            Show();
        else
            Hide();
    }

    void Show()
    {
        if (isShowing) return;
        isShowing = true;
        // Debug.Log("Show Hint");
        hintUI.ShowHint("You");
    }

    void Hide()
    {
        if (!isShowing) return;
        isShowing = false;
        // Debug.Log("Hide Hint");
        hintUI.HideHint();
    }
}
