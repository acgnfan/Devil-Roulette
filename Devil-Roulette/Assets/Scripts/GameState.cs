


// ################### 
// UPDATE: Add Target Choice
// ################### 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    // ========== ROUND & GAME STATE ==========
    public int currentRound = 1;
    public int totalRounds = 3;
    public int playerWins = 0;
    public int totalRoundsPlayed = 0;
    public bool playerWonLastRound = false;
    
    // ========== HP SYSTEM ==========
    public int playerHP = 0;
    public int dealerHP = 0;
    public float originalAlpha = 0.7f;

    public WristHealthDisplay wristUI;
    public SceneFaderGame sceneFader;
    public WinUIController winUI;
    
    // HP per round configuration
    private Dictionary<int, int> roundHP = new Dictionary<int, int>()
    {
        {1, 2}, // Round 1: 2 HP
        {2, 4}, // Round 2: 4 HP  
        {3, 6}  // Round 3: 6 HP
    };
    
    // ========== TURN SYSTEM ==========
    public bool playerTurn = true;
    public bool gameActive = false;
    public bool gameOver = false;
    public bool getsExtraTurn = false;
    
    // Shooting target
    public enum ShootTarget { Self, Opponent }
    public ShootTarget lastShotTarget = ShootTarget.Self;
    
    // ========== CHAMBER SYSTEM ==========
    public enum ShellType { Empty, Live, Blank }
    
    [System.Serializable]
    public class ChamberShell
    {
        public ShellType type = ShellType.Empty;
        public bool fired = false;
    }
    
    public List<ChamberShell> chamberShells = new List<ChamberShell>();
    public int currentChamberIndex = 0;
    public int totalBulletsLoaded = 0; // Total bullets (live + blank) in chamber
    public int bulletsFired = 0;       // How many bullets fired
    
    // ========== INITIALIZATION ==========
    void Start()
    {
        // Initialize with 8 empty chambers
        if (chamberShells.Count == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                chamberShells.Add(new ChamberShell());
            }
        }
    }
    
    // ========== HP MANAGEMENT ==========
    
    // Get HP for current round
    public int GetCurrentRoundHP()
    {
        if (roundHP.ContainsKey(currentRound))
            return roundHP[currentRound];
        return 2; // Default fallback
    }
    
    // Reset HP for new round based on round number
    public void ResetHPForRound()
    {
        int hpForThisRound = GetCurrentRoundHP();
        playerHP = hpForThisRound;
        dealerHP = hpForThisRound;
        gameActive = true;
        getsExtraTurn = false;
        
        Debug.Log($"Round {currentRound}: Starting with {hpForThisRound} HP each");
    }
    
    // Apply damage to someone
    public void ApplyDamage(bool hitPlayer)
    {
        if (hitPlayer)
        {
            playerHP = Mathf.Max(0, playerHP - 1);
            Debug.Log($"Player hit! HP: {playerHP}/{GetCurrentRoundHP()}");
        }
        else
        {
            dealerHP = Mathf.Max(0, dealerHP - 1);
            Debug.Log($"Dealer hit! HP: {dealerHP}/{GetCurrentRoundHP()}");
        }
        
        CheckRoundEnd();
    }
    
    // Check if round should end
    public bool CheckRoundEnd()
    {
        if (playerHP <= 0)
        {
            Debug.Log($"💀 PLAYER DIED! Dealer wins Round {currentRound}.");
            gameActive = false;
            StartCoroutine(RecordRoundResult(false)); // Player lost this round
            return true;
        }
        else if (dealerHP <= 0)
        {
            Debug.Log($"🎉 DEALER DIED! Player wins Round {currentRound}!");
            gameActive = false;
            StartCoroutine(RecordRoundResult(true)); // Player won this round
            return true;
        }
        
        return false; // Round continues
    }
    
    // ========== ROUND PROGRESSION ==========
    
    // Record round result and check if game is over
    public IEnumerator RecordRoundResult(bool playerWon)
    {
        totalRoundsPlayed++;
        if (playerWon) playerWins++;
        playerWonLastRound = playerWon;

        yield return new WaitForSeconds(1.0f);
        
        Debug.Log($"Round {currentRound} complete. Player wins: {playerWins}/{totalRoundsPlayed}");
        yield return winUI.Say("Player wins this round!");
        
        // Move to next round or end game
        if (currentRound < totalRounds && playerWonLastRound)
        {
            currentRound++;
            Debug.Log($"Moving to Round {currentRound}...");
            yield return winUI.Say("Moving to next round...");
        }
        else
        {
            EndGame();
        }
    }
    
    // End the entire game (all 3 rounds done)
    void EndGame()
    {
        gameOver = true;
        gameActive = false;
        
        Debug.Log("\n=================================");
        Debug.Log("🎮 GAME OVER!");
        
        if (playerWins >= 2)
            Debug.Log("🏆 PLAYER WINS THE GAME!");
        else
            Debug.Log("😞 DEALER WINS THE GAME!");
        Debug.Log("=================================");

        sceneFader.FadeOut();
        sceneFader.ReturnToMenu();
    }
    
    // Check if we should start a new round
    public bool ShouldStartNewRound()
    {
        return !gameActive && !gameOver && currentRound <= totalRounds;
    }
    
    // Calculate win rate
    public float GetPlayerWinRate()
    {
        if (totalRoundsPlayed == 0)
            return 0.5f; // Neutral start
        return (float)playerWins / totalRoundsPlayed;
    }
    
    // ========== TURN MANAGEMENT ==========
    
    // End turn with target choice rules
    public void EndTurn(bool shotSelf, bool wasLiveBullet)
    {
        Debug.Log($"EndTurn: shotSelf={shotSelf}, wasLive={wasLiveBullet}");
        
        if (playerTurn)
        {
            // PLAYER'S TURN ENDING
            if (shotSelf && wasLiveBullet)
            {
                // Player shot themselves with live bullet
                playerTurn = false;
                getsExtraTurn = false;
                Debug.Log("Player hit themselves! Switching to dealer.");
            }
            else if (shotSelf && !wasLiveBullet)
            {
                // Player shot themselves with blank
                getsExtraTurn = true;
                Debug.Log("Player safe! Gets another turn.");
            }
            else
            {
                // Player shot opponent (dealer)
                playerTurn = false;
                getsExtraTurn = false;
                Debug.Log("Player shot dealer. Switching turns.");
            }
        }
        else
        {
            // DEALER'S TURN ENDING
            if (shotSelf && wasLiveBullet)
            {
                // Dealer shot themselves with live bullet
                playerTurn = true;
                getsExtraTurn = false;
                Debug.Log("Dealer hit themselves! Switching to player.");
            }
            else if (shotSelf && !wasLiveBullet)
            {
                // Dealer shot themselves with blank
                getsExtraTurn = true;
                Debug.Log("Dealer safe! Gets another turn.");
            }
            else
            {
                // Dealer shot opponent (player)
                playerTurn = true;
                getsExtraTurn = false;
                Debug.Log("Dealer shot player. Switching turns.");
            }
        }
    }
    
    // Check if should switch to other player
    public bool ShouldSwitchPlayer()
    {
        return !getsExtraTurn;
    }
    
    // ========== BULLET MANAGEMENT ==========
    
    // Check if need to reload
    public bool NeedToReload()
    {
        // If no bullets loaded yet, we need to reload
        if (totalBulletsLoaded == 0)
            return true;
        
        // If we've fired all loaded bullets, need to reload
        return bulletsFired >= totalBulletsLoaded;
    }
    
    // Get remaining bullets
    public int GetRemainingBullets()
    {
        return Mathf.Max(0, totalBulletsLoaded - bulletsFired);
    }
    
    // Check if all bullets fired
    public bool AllBulletsFired()
    {
        return NeedToReload();
    }
    
    // Get current chamber type
    public ShellType GetCurrentChamberType()
    {
        if (currentChamberIndex >= chamberShells.Count)
            return ShellType.Empty;
        
        return chamberShells[currentChamberIndex].type;
    }
    
    // Check if current chamber has been fired
    public bool IsCurrentChamberFired()
    {
        if (currentChamberIndex >= chamberShells.Count)
            return true;
        
        return chamberShells[currentChamberIndex].fired;
    }
    
    // ========== DEBUG HELPERS ==========
    
    public void DebugBulletStatus()
    {
        Debug.Log("=== BULLET STATUS ===");
        Debug.Log($"totalBulletsLoaded: {totalBulletsLoaded}");
        Debug.Log($"bulletsFired: {bulletsFired}");
        Debug.Log($"NeedToReload(): {NeedToReload()}");
        Debug.Log($"Remaining: {GetRemainingBullets()}");
        
        // Chamber by chamber
        for (int i = 0; i < chamberShells.Count; i++)
        {
            string status = $"Ch{i}: ";
            status += chamberShells[i].type switch
            {
                ShellType.Live => "LIVE",
                ShellType.Blank => "BLANK",
                _ => "EMPTY"
            };
            status += chamberShells[i].fired ? " (fired)" : " (loaded)";
            if (i == currentChamberIndex) status += " ← CURRENT";
            Debug.Log(status);
        }
    }
    
    public void DebugGameStatus()
    {
        Debug.Log("=== GAME STATUS ===");
        Debug.Log($"Round: {currentRound}/{totalRounds}");
        Debug.Log($"Player HP: {playerHP}/{GetCurrentRoundHP()}");
        Debug.Log($"Dealer HP: {dealerHP}/{GetCurrentRoundHP()}");
        Debug.Log($"Player Turn: {playerTurn}");
        Debug.Log($"Game Active: {gameActive}");
        Debug.Log($"Game Over: {gameOver}");
        Debug.Log($"Player Wins: {playerWins}/{totalRoundsPlayed}");
    }
}