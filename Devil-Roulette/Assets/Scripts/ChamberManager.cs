// #############
// UPDATE: Proper Tracking
// #############

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChamberManager : MonoBehaviour
{
    [Header("References")]
    public GameState gameState;
    public BulletSystem bulletSystem;

    [Header("Visual Reload")]
    public ReloadVisualController reloadVisual;

    public IEnumerator ReloadChamber()
    {
        Debug.Log("\n🔫 RELOADING CHAMBER...");

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

        // Clear and setup 8 chambers
        gameState.chamberShells.Clear();
        for (int i = 0; i < 8; i++)
        {
            gameState.chamberShells.Add(new GameState.ChamberShell()
            {
                type = GameState.ShellType.Empty,
                fired = false
            });
        }

        // Mark LIVE positions
        foreach (int pos in livePositions)
        {
            if (pos >= 0 && pos < 8)
            {
                gameState.chamberShells[pos].type = GameState.ShellType.Live;
            }
        }

        // Mark BLANK positions
        foreach (int pos in blankPositions)
        {
            if (pos >= 0 && pos < 8)
            {
                gameState.chamberShells[pos].type = GameState.ShellType.Blank;
            }
        }

        // Find first chamber with a bullet (live or blank)
        gameState.currentChamberIndex = FindNextBulletChamber(0);

        if (reloadVisual != null)
        {
            Debug.Log("Playing reload visual...");
            yield return StartCoroutine(reloadVisual.PlayReloadVisual(
                liveCount,
                totalBullets - liveCount   // blankCount
            ));
        }

        Debug.Log($"Loaded {totalBullets} bullets: {liveCount} LIVE, {totalBullets - liveCount} BLANK");
        DebugChamberState();
    }

    // Fire current chamber and move to NEXT bullet (skip empties)
    public bool FireCurrentChamber()
    {
        if (gameState.currentChamberIndex >= 8 ||
            gameState.chamberShells.Count == 0 ||
            gameState.GetCurrentChamberType() == GameState.ShellType.Empty)
        {
            Debug.LogError("Trying to fire empty chamber!");
            return false;
        }

        // Get shell type
        GameState.ShellType shellType = gameState.GetCurrentChamberType();
        bool isLive = (shellType == GameState.ShellType.Live);

        // Mark as fired
        gameState.chamberShells[gameState.currentChamberIndex].fired = true;

        Debug.Log($"Firing chamber {gameState.currentChamberIndex}: " +
                 $"{(isLive ? "LIVE - BANG!" : "BLANK - click")}");

        // Track bullets fired
        gameState.bulletsFired++;

        // Find NEXT chamber with a bullet (skip empty chambers)
        int nextChamber = FindNextBulletChamber(gameState.currentChamberIndex + 1);

        if (nextChamber != -1)
        {
            gameState.currentChamberIndex = nextChamber;
        }
        else
        {
            // No more bullets, stay here (next fire will trigger reload)
            gameState.currentChamberIndex = (gameState.currentChamberIndex + 1) % 8;
        }

        return isLive;
    }

    // Find next chamber with a bullet (live or blank) that hasn't been fired
    int FindNextBulletChamber(int startIndex)
    {
        for (int i = 0; i < 8; i++) // Check all 8 chambers
        {
            int checkIndex = (startIndex + i) % 8;

            // If this chamber has a bullet (live or blank) AND not fired yet
            if (gameState.chamberShells[checkIndex].type != GameState.ShellType.Empty &&
                !gameState.chamberShells[checkIndex].fired)
            {
                return checkIndex;
            }
        }

        return -1; // No bullets found
    }

    // Check if need to reload
    public bool NeedToReload()
    {
        return gameState.AllBulletsFired() || gameState.totalBulletsLoaded == 0;
    }

    void DebugChamberState()
    {
        string visual = "Chamber State: ";
        for (int i = 0; i < gameState.chamberShells.Count; i++)
        {
            string indicator = "";
            if (i == gameState.currentChamberIndex)
                indicator = ">";

            string type = "";
            switch (gameState.chamberShells[i].type)
            {
                case GameState.ShellType.Live: type = "L"; break;
                case GameState.ShellType.Blank: type = "B"; break;
                default: type = "E"; break; // Empty
            }

            string fired = gameState.chamberShells[i].fired ? "*" : "";

            visual += $"{indicator}Ch{i}:{type}{fired} ";
        }

        visual += $"\nBullets: {gameState.GetRemainingBullets()}/{gameState.totalBulletsLoaded} remain";
        Debug.Log(visual);
    }
}
