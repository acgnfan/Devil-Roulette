using UnityEngine;


public class InteractableTurnLock : MonoBehaviour
{
    [Header("References")]
    public GameState gameState;   // 你的主逻辑脚本
    public GameFlowController gameFlowController;
    public ReturnToOrigin returnToOrigin;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    [Header("Settings")]
    public bool disableDuringDealerTurn = true;     // 非玩家回合是否禁用交互

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable == null)
        {
            Debug.LogWarning("InteractableTurnLock: No XRBaseInteractable found on this object.");
        }
    }

    void Update()
    {
        // Debug.Log($"gamestate:{gameState.playerTurn}");
        if (gameState == null || interactable == null) return;

        // 玩家回合：允许交互
        if (gameState.playerTurn && gameFlowController.isProcessingTurn == false && returnToOrigin.IsBusy == false)
        {
            if (!interactable.enabled)
            {
                interactable.enabled = true;
                Debug.Log("InteractableTurnLock: enabling interactable.");
            }
        }
        else
        {
            // 非玩家回合：禁用交互
            if (disableDuringDealerTurn)
            {
                if (interactable.enabled)
                {
                    interactable.enabled = false;
                    Debug.Log("InteractableTurnLock: disabling interactable.");
                }
            }
        }
    }
}
