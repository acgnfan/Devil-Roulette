


// ################### 
// UPDATE: Add Target Choice
// ################### 

// using UnityEngine;
// using System.Collections.Generic;

// public class GameState : MonoBehaviour
// {
//     // ========== ROUND & GAME STATE ==========
//     public int currentRound = 1;
//     public int totalRounds = 3;
//     public int playerWins = 0;
//     public int totalRoundsPlayed = 0;
//     public bool playerWonLastRound = false;
    
//     // ========== HP SYSTEM ==========
//     public int playerHP = 0;
//     public int dealerHP = 0;
    
//     // HP per round configuration
//     private Dictionary<int, int> roundHP = new Dictionary<int, int>()
//     {
//         {1, 2}, // Round 1: 2 HP
//         {2, 4}, // Round 2: 4 HP  
//         {3, 6}  // Round 3: 6 HP
//     };
    
//     // ========== TURN SYSTEM ==========
//     public bool playerTurn = true;
//     public bool gameActive = false;
//     public bool gameOver = false;
//     public bool getsExtraTurn = false;
    
//     // Shooting target
//     public enum ShootTarget { Self, Opponent }
//     public ShootTarget lastShotTarget = ShootTarget.Self;
    
//     // ========== CHAMBER SYSTEM ==========
//     public enum ShellType { Empty, Live, Blank }
    
//     [System.Serializable]
//     public class ChamberShell
//     {
//         public ShellType type = ShellType.Empty;
//         public bool fired = false;
//     }
    
//     public List<ChamberShell> chamberShells = new List<ChamberShell>();
//     public int currentChamberIndex = 0;
//     public int totalBulletsLoaded = 0; // Total bullets (live + blank) in chamber
//     public int bulletsFired = 0;       // How many bullets fired
    
//     // ========== INITIALIZATION ==========
//     void Start()
//     {
//         // Initialize with 8 empty chambers
//         if (chamberShells.Count == 0)
//         {
//             for (int i = 0; i < 8; i++)
//             {
//                 chamberShells.Add(new ChamberShell());
//             }
//         }
//     }
    
//     // ========== HP MANAGEMENT ==========
    
//     // Get HP for current round
//     public int GetCurrentRoundHP()
//     {
//         if (roundHP.ContainsKey(currentRound))
//             return roundHP[currentRound];
//         return 2; // Default fallback
//     }
    
//     // Reset HP for new round based on round number
//     public void ResetHPForRound()
//     {
//         int hpForThisRound = GetCurrentRoundHP();
//         playerHP = hpForThisRound;
//         dealerHP = hpForThisRound;
//         gameActive = true;
//         getsExtraTurn = false;
        
//         Debug.Log($"Round {currentRound}: Starting with {hpForThisRound} HP each");
//     }
    
//     // Apply damage to someone
//     public void ApplyDamage(bool hitPlayer)
//     {
//         if (hitPlayer)
//         {
//             playerHP = Mathf.Max(0, playerHP - 1);
//             Debug.Log($"Player hit! HP: {playerHP}/{GetCurrentRoundHP()}");
//         }
//         else
//         {
//             dealerHP = Mathf.Max(0, dealerHP - 1);
//             Debug.Log($"Dealer hit! HP: {dealerHP}/{GetCurrentRoundHP()}");
//         }
        
//         CheckRoundEnd();
//     }
    
//     // Check if round should end
//     public bool CheckRoundEnd()
//     {
//         if (playerHP <= 0)
//         {
//             Debug.Log($"💀 PLAYER DIED! Dealer wins Round {currentRound}.");
//             gameActive = false;
//             RecordRoundResult(false); // Player lost this round
//             return true;
//         }
//         else if (dealerHP <= 0)
//         {
//             Debug.Log($"🎉 DEALER DIED! Player wins Round {currentRound}!");
//             gameActive = false;
//             RecordRoundResult(true); // Player won this round
//             return true;
//         }
        
//         return false; // Round continues
//     }
    
//     // ========== ROUND PROGRESSION ==========
    
//     // Record round result and check if game is over
//     public void RecordRoundResult(bool playerWon)
//     {
//         totalRoundsPlayed++;
//         if (playerWon) playerWins++;
//         playerWonLastRound = playerWon;
        
//         Debug.Log($"Round {currentRound} complete. Player wins: {playerWins}/{totalRoundsPlayed}");
        
//         // Move to next round or end game
//         if (currentRound < totalRounds)
//         {
//             currentRound++;
//             Debug.Log($"Moving to Round {currentRound}...");
//         }
//         else
//         {
//             EndGame();
//         }
//     }
    
//     // End the entire game (all 3 rounds done)
//     void EndGame()
//     {
//         gameOver = true;
//         gameActive = false;
        
//         Debug.Log("\n=================================");
//         Debug.Log("🎮 GAME OVER - ALL 3 ROUNDS COMPLETE!");
//         Debug.Log($"Player won {playerWins} out of {totalRounds} rounds");
        
//         if (playerWins >= 2)
//             Debug.Log("🏆 PLAYER WINS THE GAME!");
//         else
//             Debug.Log("😞 DEALER WINS THE GAME!");
//         Debug.Log("=================================");
//     }
    
//     // Check if we should start a new round
//     public bool ShouldStartNewRound()
//     {
//         return !gameActive && !gameOver && currentRound <= totalRounds;
//     }
    
//     // Calculate win rate
//     public float GetPlayerWinRate()
//     {
//         if (totalRoundsPlayed == 0)
//             return 0.5f; // Neutral start
//         return (float)playerWins / totalRoundsPlayed;
//     }
    
//     // ========== TURN MANAGEMENT ==========
    
//     // End turn with target choice rules
//     public void EndTurn(bool shotSelf, bool wasLiveBullet)
//     {
//         Debug.Log($"EndTurn: shotSelf={shotSelf}, wasLive={wasLiveBullet}");
        
//         if (playerTurn)
//         {
//             // PLAYER'S TURN ENDING
//             if (shotSelf && wasLiveBullet)
//             {
//                 // Player shot themselves with live bullet
//                 playerTurn = false;
//                 getsExtraTurn = false;
//                 Debug.Log("Player hit themselves! Switching to dealer.");
//             }
//             else if (shotSelf && !wasLiveBullet)
//             {
//                 // Player shot themselves with blank
//                 getsExtraTurn = true;
//                 Debug.Log("Player safe! Gets another turn.");
//             }
//             else
//             {
//                 // Player shot opponent (dealer)
//                 playerTurn = false;
//                 getsExtraTurn = false;
//                 Debug.Log("Player shot dealer. Switching turns.");
//             }
//         }
//         else
//         {
//             // DEALER'S TURN ENDING
//             if (shotSelf && wasLiveBullet)
//             {
//                 // Dealer shot themselves with live bullet
//                 playerTurn = true;
//                 getsExtraTurn = false;
//                 Debug.Log("Dealer hit themselves! Switching to player.");
//             }
//             else if (shotSelf && !wasLiveBullet)
//             {
//                 // Dealer shot themselves with blank
//                 getsExtraTurn = true;
//                 Debug.Log("Dealer safe! Gets another turn.");
//             }
//             else
//             {
//                 // Dealer shot opponent (player)
//                 playerTurn = true;
//                 getsExtraTurn = false;
//                 Debug.Log("Dealer shot player. Switching turns.");
//             }
//         }
//     }
    
//     // Check if should switch to other player
//     public bool ShouldSwitchPlayer()
//     {
//         return !getsExtraTurn;
//     }
    
//     // ========== BULLET MANAGEMENT ==========
    
//     // Check if need to reload
//     public bool NeedToReload()
//     {
//         // If no bullets loaded yet, we need to reload
//         if (totalBulletsLoaded == 0)
//             return true;
        
//         // If we've fired all loaded bullets, need to reload
//         return bulletsFired >= totalBulletsLoaded;
//     }
    
//     // Get remaining bullets
//     public int GetRemainingBullets()
//     {
//         return Mathf.Max(0, totalBulletsLoaded - bulletsFired);
//     }
    
//     // Check if all bullets fired
//     public bool AllBulletsFired()
//     {
//         return NeedToReload();
//     }
    
//     // Get current chamber type
//     public ShellType GetCurrentChamberType()
//     {
//         if (currentChamberIndex >= chamberShells.Count)
//             return ShellType.Empty;
        
//         return chamberShells[currentChamberIndex].type;
//     }
    
//     // Check if current chamber has been fired
//     public bool IsCurrentChamberFired()
//     {
//         if (currentChamberIndex >= chamberShells.Count)
//             return true;
        
//         return chamberShells[currentChamberIndex].fired;
//     }
    
//     // ========== DEBUG HELPERS ==========
    
//     public void DebugBulletStatus()
//     {
//         Debug.Log("=== BULLET STATUS ===");
//         Debug.Log($"totalBulletsLoaded: {totalBulletsLoaded}");
//         Debug.Log($"bulletsFired: {bulletsFired}");
//         Debug.Log($"NeedToReload(): {NeedToReload()}");
//         Debug.Log($"Remaining: {GetRemainingBullets()}");
        
//         // Chamber by chamber
//         for (int i = 0; i < chamberShells.Count; i++)
//         {
//             string status = $"Ch{i}: ";
//             status += chamberShells[i].type switch
//             {
//                 ShellType.Live => "LIVE",
//                 ShellType.Blank => "BLANK",
//                 _ => "EMPTY"
//             };
//             status += chamberShells[i].fired ? " (fired)" : " (loaded)";
//             if (i == currentChamberIndex) status += " ← CURRENT";
//             Debug.Log(status);
//         }
//     }
    
//     public void DebugGameStatus()
//     {
//         Debug.Log("=== GAME STATUS ===");
//         Debug.Log($"Round: {currentRound}/{totalRounds}");
//         Debug.Log($"Player HP: {playerHP}/{GetCurrentRoundHP()}");
//         Debug.Log($"Dealer HP: {dealerHP}/{GetCurrentRoundHP()}");
//         Debug.Log($"Player Turn: {playerTurn}");
//         Debug.Log($"Game Active: {gameActive}");
//         Debug.Log($"Game Over: {gameOver}");
//         Debug.Log($"Player Wins: {playerWins}/{totalRoundsPlayed}");
//     }
// }


// ###################### 
// UPDATE: add tools
// ######################

using UnityEngine;
using System.Collections.Generic;

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
    
    // ========== TOOL SYSTEM ==========
    public List<ToolSystem.Tool> playerTools = new List<ToolSystem.Tool>();
    public List<ToolSystem.Tool> dealerTools = new List<ToolSystem.Tool>();
    public bool handSawActive = false; // Track if saw effect is active
    public bool handSawUsedByPlayer = false; // Who used the saw?
    public bool nextShotDoubleDamage = false; // Hand saw effect
    
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
        int damage = 1;
        
        // Apply hand saw multiplier if active
        if (nextShotDoubleDamage)
        {
            damage = ApplyDamageMultiplier(damage, !hitPlayer);
        }
        
        if (hitPlayer)
        {
            playerHP = Mathf.Max(0, playerHP - damage);
            Debug.Log($"Player hit! {damage} damage. HP: {playerHP}/{GetCurrentRoundHP()}");
        }
        else
        {
            dealerHP = Mathf.Max(0, dealerHP - damage);
            Debug.Log($"Dealer hit! {damage} damage. HP: {dealerHP}/{GetCurrentRoundHP()}");
        }
        
        CheckRoundEnd();
    }
    
    // Apply double damage effect if saw was used
    public int ApplyDamageMultiplier(int baseDamage, bool isPlayerShooting)
    {
        if (nextShotDoubleDamage && handSawUsedByPlayer == isPlayerShooting)
        {
            Debug.Log("⚔️ Hand Saw effect: Damage doubled!");
            nextShotDoubleDamage = false;
            handSawActive = false;
            return baseDamage * 2;
        }
        return baseDamage;
    }
    
    // Check if round should end
    public bool CheckRoundEnd()
    {
        if (playerHP <= 0)
        {
            Debug.Log($"💀 PLAYER DIED! Dealer wins Round {currentRound}.");
            gameActive = false;
            RecordRoundResult(false); // Player lost this round
            return true;
        }
        else if (dealerHP <= 0)
        {
            Debug.Log($"🎉 DEALER DIED! Player wins Round {currentRound}!");
            gameActive = false;
            RecordRoundResult(true); // Player won this round
            return true;
        }
        
        return false; // Round continues
    }
    
    // ========== ROUND PROGRESSION ==========
    
    // Record round result and check if game is over
    public void RecordRoundResult(bool playerWon)
    {
        totalRoundsPlayed++;
        if (playerWon) playerWins++;
        playerWonLastRound = playerWon;
        
        Debug.Log($"Round {currentRound} complete. Player wins: {playerWins}/{totalRoundsPlayed}");
        
        // Move to next round or end game
        if (currentRound < totalRounds)
        {
            currentRound++;
            Debug.Log($"Moving to Round {currentRound}...");
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
        Debug.Log("🎮 GAME OVER - ALL 3 ROUNDS COMPLETE!");
        Debug.Log($"Player won {playerWins} out of {totalRounds} rounds");
        
        if (playerWins >= 2)
            Debug.Log("🏆 PLAYER WINS THE GAME!");
        else
            Debug.Log("😞 DEALER WINS THE GAME!");
        Debug.Log("=================================");
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
        // if (totalBulletsLoaded == 0)
        if (totalBulletsLoaded == 0 || chamberShells.Count == 0)
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
    
    // ========== TOOL MANAGEMENT ==========
    
    // Get tools for current round
    // public void DistributeTools(ToolSystem toolSystem)
    // public void DistributeTools()
    // {
    //     // Clear old tools (tools don't carry between rounds)
    //     playerTools.Clear();
    //     dealerTools.Clear();
    //     handSawActive = false;
    //     nextShotDoubleDamage = false;
        
    //     // Get tool count for this round
    //     int toolCount = GetToolCountForRound();
        
    //     // Get random tools for each player
    //     // playerTools = toolSystem.GetRandomTools(toolCount);
    //     // dealerTools = toolSystem.GetRandomTools(toolCount);
    //     // playerTools = toolSystem.GetToolsForRound(currentRound, true);
    //     // dealerTools = toolSystem.GetToolsForRound(currentRound, false);
    //     playerTools = ToolSystem.Instance.GetToolsForRound(currentRound, true);
    //     dealerTools = ToolSystem.Instance.GetToolsForRound(currentRound, false);
        
    //     Debug.Log($"Tool Distribution - Round {currentRound} ({toolCount} tools each):");
    //     Debug.Log($"  Player: {GetToolList(true)}");
    //     Debug.Log($"  Dealer: {GetToolList(false)}");
    // }

    // 
    // 
    // 
    // Clear and get fresh tools at round start
    public void InitializeRoundTools()
    {
        // Clear all tools at round start
        playerTools.Clear();
        dealerTools.Clear();
        handSawActive = false;
        nextShotDoubleDamage = false;
        
        // Get tool count for this round
        int toolCount = GetToolCountForRound();
        
        // Get fresh tools
        List<ToolSystem.Tool> newPlayerTools = ToolSystem.Instance.GetToolsForRound(currentRound, true);
        List<ToolSystem.Tool> newDealerTools = ToolSystem.Instance.GetToolsForRound(currentRound, false);
        
        // Add the new tools
        playerTools.AddRange(newPlayerTools);
        dealerTools.AddRange(newDealerTools);
        
        // Ensure not over maximum (8)
        if (playerTools.Count > 8) 
        {
            Debug.Log($"Player has {playerTools.Count} tools, capping at 8");
            playerTools = playerTools.GetRange(0, 8);
        }
        if (dealerTools.Count > 8) 
        {
            Debug.Log($"Dealer has {dealerTools.Count} tools, capping at 8");
            dealerTools = dealerTools.GetRange(0, 8);
        }
        
        Debug.Log($"=== ROUND {currentRound} START === ");
        Debug.Log($"Fresh tools: {toolCount} each (MAX: 8)");
        Debug.Log($"Player: {playerTools.Count} tools - {GetToolList(true)}");
        Debug.Log($"Dealer: {dealerTools.Count} tools - {GetToolList(false)}");
    }

    // Add more tools during reload (accumulate, max 8)
    public void AddToolsOnReload()
    {
        // Get tool count for this round
        int toolCount = GetToolCountForRound();
        
        // Get additional tools
        List<ToolSystem.Tool> newPlayerTools = ToolSystem.Instance.GetToolsForRound(currentRound, true);
        List<ToolSystem.Tool> newDealerTools = ToolSystem.Instance.GetToolsForRound(currentRound, false);
        
        // Check if we have room for new tools
        int playerSpace = 8 - playerTools.Count;
        int dealerSpace = 8 - dealerTools.Count;
        
        if (playerSpace <= 0 && dealerSpace <= 0)
        {
            Debug.Log("=== RELOAD: Both players have MAX tools (8), no more can be added ===");
            return;
        }
        
        // Add as many as we have space for
        int playerToAdd = Mathf.Min(newPlayerTools.Count, playerSpace);
        int dealerToAdd = Mathf.Min(newDealerTools.Count, dealerSpace);
        
        if (playerToAdd > 0)
        {
            playerTools.AddRange(newPlayerTools.GetRange(0, playerToAdd));
        }
        
        if (dealerToAdd > 0)
        {
            dealerTools.AddRange(newDealerTools.GetRange(0, dealerToAdd));
        }
        
        Debug.Log($"=== RELOAD: Adding tools (MAX 8) ===");
        Debug.Log($"Player: Added {playerToAdd}, now {playerTools.Count}/8 - Space: {8 - playerTools.Count}");
        Debug.Log($"Dealer: Added {dealerToAdd}, now {dealerTools.Count}/8 - Space: {8 - dealerTools.Count}");
        
        if (playerTools.Count >= 8)
            Debug.Log("Player has MAX tools (8)!");
        if (dealerTools.Count >= 8)
            Debug.Log("Dealer has MAX tools (8)!");
    }

    // Remove the old DistributeTools() method and replace with:
    public void ClearTools()
    {
        playerTools.Clear();
        dealerTools.Clear();
        handSawActive = false;
        nextShotDoubleDamage = false;
        Debug.Log("All tools cleared");
    }

    // Optional: Helper method to check if at max capacity
    public bool IsPlayerAtMaxTools()
    {
        return playerTools.Count >= 8;
    }

    public bool IsDealerAtMaxTools()
    {
        return dealerTools.Count >= 8;
    }
// 
    
    int GetToolCountForRound()
    {
        return currentRound switch
        {
            1 => 0,
            2 => 2,
            3 => 4,
            _ => 0
        };
    }
    
    // Check if has tool
    public bool HasTool(bool isPlayer, ToolSystem.ToolType toolType)
    {
        List<ToolSystem.Tool> tools = isPlayer ? playerTools : dealerTools;
        
        foreach (var tool in tools)
        {
            if (tool.type == toolType)
                return true;
        }
        return false;
    }
    
    // Use tool (remove from inventory - ALL TOOLS ARE CONSUMABLE)
    public void RemoveTool(bool isPlayer, ToolSystem.ToolType toolType)
    {
        List<ToolSystem.Tool> tools = isPlayer ? playerTools : dealerTools;
        
        for (int i = tools.Count - 1; i >= 0; i--)
        {
            if (tools[i].type == toolType)
            {
                Debug.Log($"{(isPlayer ? "Player" : "Dealer")} used {tools[i].name}");
                
                // ALL TOOLS ARE CONSUMABLE - Remove from inventory
                tools.RemoveAt(i);
                
                // Special effects
                if (toolType == ToolSystem.ToolType.HandSaw)
                {
                    handSawActive = true;
                    handSawUsedByPlayer = isPlayer;
                    nextShotDoubleDamage = true;
                    Debug.Log("⚔️ Hand Saw active! Next shot deals 2x damage!");
                }
                
                return; 
            }
        }
    }
    
    // Get list of tool names for UI
    public string GetToolList(bool isPlayer)
    {
        List<ToolSystem.Tool> tools = isPlayer ? playerTools : dealerTools;
        
        if (tools.Count == 0)
            return "None";
        
        string result = "";
        foreach (var tool in tools)
        {
            result += $"{tool.name}, ";
        }
        return result.TrimEnd(',', ' ');
    }
    
    // Clear tools (between rounds)
    // public void ClearTools()
    // {
    //     playerTools.Clear();
    //     dealerTools.Clear();
    //     handSawActive = false;
    //     nextShotDoubleDamage = false;
    // }
    
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
        Debug.Log($"Player Tools: {playerTools.Count}");
        Debug.Log($"Dealer Tools: {dealerTools.Count}");
    }
}