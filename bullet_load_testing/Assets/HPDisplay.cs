using UnityEngine;
using UnityEngine.UI;

public class HPDisplay : MonoBehaviour
{
    [Header("References")]
    public GameState gameState;
    
    [Header("UI Elements")]
    public Text playerHPText;
    public Text dealerHPText;
    public Text turnText;
    public Text roundText;
    public Text statusText;
    public Text gameProgressText;
    
    void Update()
    {
        if (gameState == null) return;
        
        // Update HP displays with round-specific max HP
        int maxHP = gameState.GetCurrentRoundHP();
        playerHPText.text = $"Player HP: {gameState.playerHP}/{maxHP}";
        dealerHPText.text = $"Dealer HP: {gameState.dealerHP}/{maxHP}";
        
        // Update turn indicator
        if (gameState.gameActive)
        {
            turnText.text = gameState.playerTurn ? "👤 Player's Turn" : "🤖 Dealer's Turn";
            turnText.color = gameState.playerTurn ? Color.green : Color.yellow;
        }
        else
        {
            turnText.text = "Round Over";
            turnText.color = Color.red;
        }
        
        // Round info
        roundText.text = $"Round: {gameState.currentRound}/3 ({maxHP} HP each)";
        
        // Game progress
        gameProgressText.text = $"Player Wins: {gameState.playerWins}/3 rounds";
        
        // Game status
        if (gameState.gameOver)
        {
            if (gameState.playerWins >= 2)
                statusText.text = "🏆 PLAYER WINS THE GAME! Press R to restart";
            else
                statusText.text = "😞 DEALER WINS THE GAME! Press R to restart";
        }
        else if (!gameState.gameActive)
        {
            if (gameState.playerHP <= 0)
                statusText.text = $"💀 Player lost Round {gameState.currentRound-1}! Next round starting...";
            else if (gameState.dealerHP <= 0)
                statusText.text = $"🎉 Player won Round {gameState.currentRound-1}! Next round starting...";
            else
                statusText.text = "Press N for next round";
        }
        else
        {
            statusText.text = gameState.playerTurn ? 
                "Press SPACE to fire" : "Dealer is thinking...";
        }
    }
}
