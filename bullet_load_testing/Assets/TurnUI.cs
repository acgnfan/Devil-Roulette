using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    [Header("References")]
    public GameFlowController gameFlow;
    
    [Header("UI Elements")]
    public GameObject playerTurnPanel;
    public GameObject dealerTurnPanel;
    public Text statusText;
    
    void Update()
    {
        if (gameFlow == null || gameFlow.gameState == null) return;
        
        // Show/hide panels based on turn
        if (playerTurnPanel != null)
            playerTurnPanel.SetActive(gameFlow.gameState.gameActive && gameFlow.gameState.playerTurn);
        
        if (dealerTurnPanel != null)
            dealerTurnPanel.SetActive(gameFlow.gameState.gameActive && !gameFlow.gameState.playerTurn);
        
        // Update status
        if (statusText != null)
        {
            if (!gameFlow.gameState.gameActive)
            {
                if (gameFlow.gameState.playerHP <= 0)
                    statusText.text = "💀 Player Died!";
                else if (gameFlow.gameState.dealerHP <= 0)
                    statusText.text = "🎉 Dealer Died!";
                else
                    statusText.text = "Round Over";
            }
            else
            {
                statusText.text = gameFlow.gameState.playerTurn ? 
                    "Your turn - Choose target:" : "Dealer's turn...";
            }
        }
    }
}

