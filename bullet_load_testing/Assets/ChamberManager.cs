
// #######################
// UPDATE: Correcting the return value of some function 
// ####################### 

using UnityEngine;
using System.Collections.Generic;

public class ChamberManager : MonoBehaviour
{
    [Header("References")]
    public GameState gameState;
    public BulletSystem bulletSystem;
    
    [Header("Special Ability Settings")]
    public int maxInspectUsesPerRound = 2;
    public int maxRevealUsesPerRound = 1;
    
    private int inspectUsesRemaining;
    private int revealUsesRemaining;
    
    void Start()
    {
        ResetAbilityUses();
    }
    
    public void ResetAbilityUses()
    {
        inspectUsesRemaining = maxInspectUsesPerRound;
        revealUsesRemaining = maxRevealUsesPerRound;
    }


    // ################## 
    // UPDATE: Remove empty chamber logic
    // ################## 
    public void ReloadChamber()
    {
        Debug.Log("\n🔫 RELOADING CHAMBER (Consecutive Bullets)...");
        
        // Reset ability uses for new round
        ResetAbilityUses();
        
        // Get bullet configuration with LIVE and BLANK shells
        var (totalBullets, liveCount, livePositions, blankPositions) = 
            bulletSystem.GenerateBullets(
                gameState.currentRound,
                gameState.GetPlayerWinRate(),
                gameState.playerWonLastRound
            );
        
        // Store bullet count
        gameState.totalBulletsLoaded = totalBullets;
        gameState.bulletsFired = 0;
        
        // Clear and setup ONLY bullet chambers (no empties)
        gameState.chamberShells.Clear();
        for (int i = 0; i < totalBullets; i++) // Only create chambers for bullets
        {
            GameState.ChamberShell shell = new GameState.ChamberShell()
            {
                type = GameState.ShellType.Empty, // Will be set below
                fired = false
            };
            
            // Check if this position is live
            bool isLive = false;
            foreach (int pos in livePositions)
            {
                if (pos == i)
                {
                    isLive = true;
                    break;
                }
            }
            
            // Check if this position is blank
            bool isBlank = false;
            foreach (int pos in blankPositions)
            {
                if (pos == i)
                {
                    isBlank = true;
                    break;
                }
            }
            
            // Set the type
            if (isLive)
                shell.type = GameState.ShellType.Live;
            else if (isBlank)
                shell.type = GameState.ShellType.Blank;
            else
                shell.type = GameState.ShellType.Empty; // Shouldn't happen
            
            gameState.chamberShells.Add(shell);
        }
        
        // Start at chamber 0 (always has a bullet)
        gameState.currentChamberIndex = 0;
        
        Debug.Log($"Loaded {totalBullets} bullets: {liveCount} LIVE, {totalBullets-liveCount} BLANK");
        DebugChamberState();
    }
    

    // Fire current chamber and move to NEXT bullet (consecutive)
    public bool FireCurrentChamber()
    {
        if (gameState.currentChamberIndex >= gameState.chamberShells.Count || 
            gameState.chamberShells.Count == 0)
        {
            Debug.LogError("Trying to fire non-existent chamber!");
            return false;
        }
        
        // Get shell type
        GameState.ShellType shellType = gameState.chamberShells[gameState.currentChamberIndex].type;
        bool isLive = (shellType == GameState.ShellType.Live);
        
        // Simple log
        string shellName = isLive ? "LIVE" : "BLANK";
        Debug.Log($"\n💥 Firing chamber {gameState.currentChamberIndex}: {shellName}");
        Debug.Log($"Result: {(isLive ? "BANG!" : "Click")}");
        
        // Mark as fired
        gameState.chamberShells[gameState.currentChamberIndex].fired = true;
        
        // Track bullets fired
        gameState.bulletsFired++;
        
        // Move to next chamber if available
        if (gameState.currentChamberIndex + 1 < gameState.chamberShells.Count)
        {
            gameState.currentChamberIndex++;
            Debug.Log($"Next chamber: {gameState.currentChamberIndex}");
        }
        else
        {
            Debug.Log("All bullets fired!");
        }
        
        // Show simple updated state
        ShowSimpleChamberState();
        
        return isLive;
    }

    // Add this helper method for simple state display
    private void ShowSimpleChamberState()
    {
        if (gameState.chamberShells.Count == 0) return;
        
        string positionLine = "Position: ";
        string typeLine = "Type:     ";
        string firedLine = "Fired:    ";
        
        for (int i = 0; i < gameState.chamberShells.Count; i++)
        {
            positionLine += $"{i} ";
            
            string typeSymbol = gameState.chamberShells[i].type switch
            {
                GameState.ShellType.Live => "L",
                GameState.ShellType.Blank => "B",
                _ => "E"
            };
            typeLine += $"{typeSymbol} ";
            firedLine += $"{(gameState.chamberShells[i].fired ? "✓" : "•")} ";
        }
        
        Debug.Log("Updated chamber:");
        Debug.Log(positionLine);
        Debug.Log(typeLine);
        Debug.Log(firedLine);
    }
    
    // Reveals a random unfired bullet chamber (like spinning the chamber)
    public (bool success, int chamberIndex, GameState.ShellType shellType) RevealRandomShell()
    {
        Debug.Log("\n🎰 REVEALING RANDOM SHELL...");
        
        // Check if ability can be used
        if (revealUsesRemaining <= 0)
        {
            Debug.Log("No reveal uses remaining this round!");
            return (false, -1, GameState.ShellType.Empty);
        }
        
        // Collect all unfired bullet chambers (live or blank)
        List<int> unfiredBulletChambers = new List<int>();
        
        // for (int i = 0; i < 8; i++)
        for (int i = 0; i < gameState.chamberShells.Count; i++)  // Changed from i < 8
        {
            if (gameState.chamberShells[i].type != GameState.ShellType.Empty &&
                !gameState.chamberShells[i].fired)
            {
                unfiredBulletChambers.Add(i);
            }
        }
        
        if (unfiredBulletChambers.Count == 0)
        {
            Debug.Log("No unfired bullets remaining!");
            return (false, -1, GameState.ShellType.Empty);
        }
        
        // Pick a random unfired chamber
        int randomIndex = Random.Range(0, unfiredBulletChambers.Count);
        int chosenChamber = unfiredBulletChambers[randomIndex];
        GameState.ShellType revealedType = gameState.chamberShells[chosenChamber].type;
        
        // Decrement uses
        revealUsesRemaining--;
        
        // Log the revelation
        string shellName = revealedType == GameState.ShellType.Live ? "LIVE" : "BLANK";
        Debug.Log($"Revealed chamber {chosenChamber}: {shellName} bullet");
        Debug.Log($"Reveals remaining: {revealUsesRemaining}");
        
        return (true, chosenChamber, revealedType);
    }
    
    // Inspects the current chamber player is about to fire
    public (bool success, GameState.ShellType shellType) InspectCurrentChamber()
    {
        Debug.Log("\n🔍 INSPECTING CURRENT CHAMBER...");
        
        // Check if ability can be used
        if (inspectUsesRemaining <= 0)
        {
            Debug.Log("No inspect uses remaining this round!");
            return (false, GameState.ShellType.Empty);
        }
        
        // Check if we can inspect (only if it's a bullet)
        if (gameState.GetCurrentChamberType() == GameState.ShellType.Empty)
        {
            Debug.Log("Current chamber is EMPTY - nothing to inspect");
            return (false, GameState.ShellType.Empty);
        }
        
        // Check if current chamber has been fired
        if (gameState.chamberShells[gameState.currentChamberIndex].fired)
        {
            Debug.Log("Current chamber already fired!");
            return (false, GameState.ShellType.Empty);
        }
        
        GameState.ShellType currentType = gameState.GetCurrentChamberType();
        string shellName = currentType == GameState.ShellType.Live ? "LIVE" : "BLANK";
        
        // Decrement uses
        inspectUsesRemaining--;
        
        Debug.Log($"Chamber {gameState.currentChamberIndex} contains: {shellName} bullet");
        Debug.Log($"Inspects remaining: {inspectUsesRemaining}");
        
        return (true, currentType);
    }
    
    // Skips the current chamber without firing (like spinning to next bullet)
    public (bool success, int originalIndex, int newIndex, GameState.ShellType originalType, GameState.ShellType newType) SkipCurrentChamber()
    {
        Debug.Log("\n⏩ SKIPPING CURRENT CHAMBER...");
        
        // Store original chamber info
        int originalIndex = gameState.currentChamberIndex;
        GameState.ShellType originalType = gameState.GetCurrentChamberType();
        
        // Can't skip if it's already been fired
        if (gameState.chamberShells[originalIndex].fired)
        {
            Debug.Log("Cannot skip already fired chamber!");
            return (false, originalIndex, -1, originalType, GameState.ShellType.Empty);
        }
        
        // Find the next unfired bullet chamber
        int nextChamber = FindNextBulletChamber(gameState.currentChamberIndex + 1);
        
        if (nextChamber != -1 && nextChamber != originalIndex)
        {
            GameState.ShellType newType = gameState.chamberShells[nextChamber].type;
            
            // Move to the next bullet
            gameState.currentChamberIndex = nextChamber;
            
            string originalTypeName = originalType == GameState.ShellType.Live ? "LIVE" : "BLANK";
            string newTypeName = newType == GameState.ShellType.Live ? "LIVE" : "BLANK";
            
            Debug.Log($"Skipped chamber {originalIndex} ({originalTypeName}) → Now at chamber {gameState.currentChamberIndex} ({newTypeName})");
            return (true, originalIndex, nextChamber, originalType, newType);
        }
        else
        {
            // No more bullets to skip to
            Debug.Log("No more unfired bullets to skip to!");
            return (false, originalIndex, -1, originalType, GameState.ShellType.Empty);
        }
    }
    


    // ################
    // UPDATE: simplify since all are bullets
    // ################ 
    // Find next chamber with a bullet (live or blank) that hasn't been fired
    public int FindNextBulletChamber(int startIndex)
    {
        for (int i = 0; i < gameState.chamberShells.Count; i++) // Check only bullet chambers
        {
            int checkIndex = (startIndex + i) % gameState.chamberShells.Count;
            
            // All chambers have bullets, just check if not fired
            if (!gameState.chamberShells[checkIndex].fired)
            {
                return checkIndex;
            }
        }
        
        return -1; // No unfired bullets found
    }
    
    // Check if need to reload
    public bool NeedToReload()
    {
        return gameState.AllBulletsFired() || gameState.totalBulletsLoaded == 0;
    }
    
    // Get remaining ability uses
    public (int inspectUses, int revealUses) GetRemainingAbilityUses()
    {
        return (inspectUsesRemaining, revealUsesRemaining);
    }
    


    // 
    // 
    // 
    public void DebugChamberState()
    {
        if (gameState.chamberShells.Count == 0)
        {
            Debug.Log("Chamber is EMPTY - Need to reload!");
            return;
        }
        
        // Build simple visual
        string positionLine = "Position: ";
        string typeLine = "Type:     ";
        
        for (int i = 0; i < gameState.chamberShells.Count; i++)
        {
            positionLine += $"{i} ";
            
            string typeSymbol = gameState.chamberShells[i].type switch
            {
                GameState.ShellType.Live => "L",
                GameState.ShellType.Blank => "B",
                _ => "E"
            };
            typeLine += $"{typeSymbol} ";
        }
        
        Debug.Log("\n🔫 CURRENT CHAMBER:");
        Debug.Log(positionLine);
        Debug.Log(typeLine);
        
        // Show current position with arrow
        string currentLine = "Current:  ";
        for (int i = 0; i < gameState.chamberShells.Count; i++)
        {
            currentLine += $"{(i == gameState.currentChamberIndex ? "↑" : " ")} ";
        }
        Debug.Log(currentLine);
        
        // Simple status
        Debug.Log($"Index: {gameState.currentChamberIndex}, " +
                $"Fired: {gameState.bulletsFired}/{gameState.totalBulletsLoaded}");
    }

        
    // Get chamber info for UI display
    public List<(int index, GameState.ShellType type, bool fired, bool isCurrent)> GetAllChamberInfo()
    {
        List<(int, GameState.ShellType, bool, bool)> chamberInfo = new List<(int, GameState.ShellType, bool, bool)>();
        
        for (int i = 0; i < gameState.chamberShells.Count; i++)
        {
            chamberInfo.Add((
                i,
                gameState.chamberShells[i].type,
                gameState.chamberShells[i].fired,
                i == gameState.currentChamberIndex
            ));
        }
        
        return chamberInfo;
    }
    
    // Check if specific chamber can be inspected (for UI highlighting)
    public bool CanInspectChamber(int chamberIndex)
    {
        if (chamberIndex < 0 || chamberIndex >= gameState.chamberShells.Count)
            return false;
            
        return gameState.chamberShells[chamberIndex].type != GameState.ShellType.Empty &&
               !gameState.chamberShells[chamberIndex].fired &&
               inspectUsesRemaining > 0;
    }
}





