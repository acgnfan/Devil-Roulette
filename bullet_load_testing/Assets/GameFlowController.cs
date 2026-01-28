

// ###################### 
// UPDATE: add tools
// ######################
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameFlowController : MonoBehaviour
{
    [Header("CORE REFERENCES")]
    public GameState gameState;
    public BulletSystem bulletSystem;
    public ChamberManager chamberManager;
    public ToolSystem toolSystem;
    
    [Header("UI REFERENCES")]
    public Button shootSelfButton;
    public Button shootOpponentButton;
    public Text turnText;
    public Text instructionText;
    public Text roundText;
    public Text playerHPText;
    public Text dealerHPText;
    public Text bulletCountText;
    public Text playerToolsText;
    public Text dealerToolsText;
    public GameObject playerTurnPanel;
    public GameObject dealerTurnPanel;
    
    [Header("TOOL UI")]                                    // No use now 
    public GameObject toolSelectionPanel;                  // No use now 
    public Button[] toolButtons; // 6 buttons for tools    // No use now 
    public Text toolInfoText;                              // No use now 
    
    [Header("GAME SETTINGS")]
    public bool autoStart = true;
    public float dealerThinkTime = 1.5f;
    public float roundTransitionTime = 2f;
    
    [Header("DEBUG")]
    public bool debugMode = false;
    private bool isProcessingTurn = false;
    private bool isSelectingTool = false;
    // private bool hasUsedToolThisTurn = false;          // player can use multiple tools before shooting 
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // Setup button listeners
        if (shootSelfButton != null)
        {
            shootSelfButton.onClick.RemoveAllListeners();
            shootSelfButton.onClick.AddListener(() => OnShootChoice(true));
        }
        
        if (shootOpponentButton != null)
        {
            shootOpponentButton.onClick.RemoveAllListeners();
            shootOpponentButton.onClick.AddListener(() => OnShootChoice(false));
        }
        
        // Setup tool button listeners
        if (toolButtons != null && toolButtons.Length >= 6)
        {
            for (int i = 0; i < toolButtons.Length; i++)
            {
                int index = i;
                toolButtons[i].onClick.RemoveAllListeners();
                toolButtons[i].onClick.AddListener(() => OnToolSelected(index));
            }
        }
        
        // Start the game
        if (autoStart)
        {
            StartNewRound();
        }
    }
    
    // ========== ROUND MANAGEMENT ==========
    
    public void StartNewRound()
    {
        if (gameState.gameOver)
        {
            Debug.Log("Game is over! Press R to restart.");
            UpdateAllUI();
            return;
        }
        
        Debug.Log($"\n{'='.Repeat(40)}");
        Debug.Log($"=== STARTING ROUND {gameState.currentRound} of {gameState.totalRounds} ===");
        Debug.Log($"{'='.Repeat(40)}");
        
        // Reset game state for new round
        gameState.ResetHPForRound();
        gameState.playerTurn = true;
        gameState.getsExtraTurn = false; 
        isProcessingTurn = false;
        isSelectingTool = false;
        // hasUsedToolThisTurn = false;
        
        // DISTRIBUTE TOOLS for this round
        // gameState.DistributeTools();
        gameState.InitializeRoundTools();
        
        // Initial reload
        chamberManager.ReloadChamber();
        
        // Update UI
        UpdateAllUI();
        
        Debug.Log($"Round {gameState.currentRound}: {gameState.GetCurrentRoundHP()} HP each");
        Debug.Log("Player's turn first!");
    }
    
    // ========== PLAYER ACTIONS ==========
    
    // Called when player chooses who to shoot
    public void OnShootChoice(bool shootSelf)
    {
        if (isProcessingTurn) return;
        if (!gameState.gameActive)
        {
            Debug.Log("Round is not active!");
            return;
        }
        
        if (!gameState.playerTurn)
        {
            Debug.Log("Not player's turn!");
            return;
        }
        
        isProcessingTurn = true;
        string target = shootSelf ? "themselves" : "the dealer";
        Debug.Log($"\n👤 Player chooses to shoot {target}");
        
        ExecuteShot(shootSelf);
    }
    
    // ========== SHOT EXECUTION ==========
    
    void ExecuteShot(bool shootSelf)
    {
        // Check if need to reload BEFORE showing choice (already handled)
        // Fire the chamber
        bool isLive = chamberManager.FireCurrentChamber();
        
        // Apply damage based on target (with hand saw multiplier)
        if (isLive)
        {
            if (shootSelf)
            {
                // Shooter hit themselves
                Debug.Log("💥 BANG! Shooter hit themselves!");
                gameState.ApplyDamage(gameState.playerTurn);
            }
            else
            {
                // Shooter hit opponent
                string hitWho = gameState.playerTurn ? "Dealer" : "Player";
                Debug.Log($"💥 BANG! {hitWho} was hit!");
                gameState.ApplyDamage(!gameState.playerTurn);
            }
        }
        else
        {
            Debug.Log("🔘 Click... safe!");
        }
        
        // Update UI immediately
        UpdateHPUI();
        UpdateBulletUI();
        
        // Record the shot target
        gameState.lastShotTarget = shootSelf ? 
            GameState.ShootTarget.Self : GameState.ShootTarget.Opponent;
        
        // End turn based on rules
        gameState.EndTurn(shootSelf, isLive);
        
        // Check if round ended
        if (!gameState.gameActive)
        {
            isProcessingTurn = false;
            HandleRoundEnd();
            return;
        }
        
        // Handle turn continuation
        HandleTurnContinuation();
    }
    
    // ========== TURN MANAGEMENT ==========
    
    void HandleTurnContinuation()
    {
        // If it's still the same player's turn (blank self-shot)
        if (gameState.getsExtraTurn)
        {
            Debug.Log("🎯 Same player gets another turn!");
            gameState.getsExtraTurn = false;
            
            if (gameState.playerTurn)
            {
                // Player gets another turn
                StartCoroutine(PlayerTurnSequence());
            }
            else
            {
                // Dealer gets another turn
                StartCoroutine(DealerTurnSequence());
            }
        }
        else
        {
            // Switch to other player
            if (!gameState.playerTurn)
            {
                // Dealer's turn
                StartCoroutine(DealerTurnSequence());
            }
            else
            {
                // Player's turn
                StartCoroutine(PlayerTurnSequence());
            }
        }
    }
    
    IEnumerator PlayerTurnSequence()
    {
        isProcessingTurn = false;
        // hasUsedToolThisTurn = false;
        
        // Check if need to reload BEFORE showing buttons
        if (chamberManager.NeedToReload())
        {
            Debug.Log("🔁 Reloading before player's turn...");
            chamberManager.ReloadChamber();

            // DISTRIBUTE NEW TOOLS (ACCUMULATE)
            // gameState.DistributeTools(); // <-- Add tools on reload
            gameState.AddToolsOnReload();

            // Always player's turn after reload
            gameState.playerTurn = true;

            UpdateBulletUI();
            UpdateToolUI(); // Update

            yield return new WaitForSeconds(0.5f);
        }
        
        UpdateAllUI();
        
        // Check if player has tools to use
        if (gameState.playerTools.Count > 0)
        {
            Debug.Log("Player has tools available. Press T to use or Space to shoot.");
        }
        
        Debug.Log("Player's turn - waiting for choice...");
    }
    
    IEnumerator DealerTurnSequence()
    {
        isProcessingTurn = false;
        
        // Check if need to reload BEFORE dealer acts
        if (chamberManager.NeedToReload())
        {
            Debug.Log("🔁 Reloading before dealer's turn...");
            chamberManager.ReloadChamber();

            // DISTRIBUTE NEW TOOLS (ACCUMULATE)
            // gameState.DistributeTools(); // <-- Add tools on reload
            gameState.AddToolsOnReload();

            // Switch to player's turn after reload
            gameState.playerTurn = true;

            UpdateBulletUI();
            UpdateToolUI(); // Update
            yield return new WaitForSeconds(0.5f);

            //
            // 
            // 
            // Player goes after reload
            StartCoroutine(PlayerTurnSequence());
            yield break;
        }
        
        UpdateAllUI();
        yield return new WaitForSeconds(0.5f);
        
        // Dealer AI decision: use tools first if available
        // if (gameState.dealerTools.Count > 0 && Random.value < 0.7f) // 70% chance to use tool
        // {
        //     yield return StartCoroutine(DealerUseTool());
        // }
        // else
        // {
            // Then shoot
        //     StartCoroutine(DealerTakeTurn());
        // }

        // 
        // 
        // 
        // Dealer can use MULTIPLE tools if available
        while (gameState.dealerTools.Count > 0 && Random.value < 0.7f) // 70% chance per tool
        {
            yield return StartCoroutine(DealerUseTool());
            yield return new WaitForSeconds(0.5f);
        }
        
        // Then shoot
        StartCoroutine(DealerTakeTurn());

    }
    
    IEnumerator DealerUseTool()
    {
        Debug.Log("🤖 Dealer considering using a tool...");
        yield return new WaitForSeconds(dealerThinkTime);
        
        // Simple AI: pick random tool
        if (gameState.dealerTools.Count > 0)
        {
            int randomIndex = Random.Range(0, gameState.dealerTools.Count);
            var toolToUse = gameState.dealerTools[randomIndex].type;
            
            Debug.Log($"Dealer decides to use {toolToUse}");
            // UseTool(false, toolToUse);
            UseSpecificTool(false, toolToUse); 
            
            // Wait then take turn
            yield return new WaitForSeconds(1f);
            // StartCoroutine(DealerTakeTurn());
        }
        // else
        // {
        //     StartCoroutine(DealerTakeTurn());
        // }
    }
    
    void HandleRoundEnd()
    {
        Debug.Log("\n🏁 ROUND ENDED!");
        
        if (gameState.playerHP <= 0)
            Debug.Log("💀 Player lost this round!");
        else if (gameState.dealerHP <= 0)
            Debug.Log("🎉 Player won this round!");
        
        UpdateAllUI();
        
        // Check if should start next round
        if (gameState.ShouldStartNewRound())
        {
            StartCoroutine(AutoNextRound());
        }
        else if (gameState.gameOver)
        {
            Debug.Log("\n🎮 GAME OVER!");
            UpdateAllUI();
        }
    }
    
    // ========== DEALER AI ==========
    
    // IEnumerator DealerTakeTurn()
    // {
    //     Debug.Log("\n🤖 DEALER'S TURN...");
        
    //     // Show dealer thinking
    //     if (dealerTurnPanel != null)
    //         dealerTurnPanel.SetActive(true);
    //     if (playerTurnPanel != null)
    //         playerTurnPanel.SetActive(false);
        
    //     // Wait for "thinking" time
    //     yield return new WaitForSeconds(dealerThinkTime);
        
    //     // For now, dealer always shoots themselves
    //     // Later we'll add AI decision making
    //     Debug.Log("Dealer chooses to shoot themselves...");
        
    //     isProcessingTurn = true;
    //     ExecuteShot(true); // Dealer shoots self
    // }

    // 
    // 
    // 
    // ========== DEALER AI ==========

    // Helper method to count remaining bullet types
    private (int liveCount, int blankCount) GetRemainingBulletStats()
    {
        int liveCount = 0;
        int blankCount = 0;
        
        foreach (var shell in gameState.chamberShells)
        {
            if (!shell.fired)
            {
                if (shell.type == GameState.ShellType.Live)
                    liveCount++;
                else if (shell.type == GameState.ShellType.Blank)
                    blankCount++;
            }
        }
        
        return (liveCount, blankCount);
    }

    IEnumerator DealerTakeTurn()
    {
        Debug.Log("\n🤖 DEALER'S TURN...");
        
        // Show dealer thinking
        if (dealerTurnPanel != null)
            dealerTurnPanel.SetActive(true);
        if (playerTurnPanel != null)
            playerTurnPanel.SetActive(false);
        
        // Wait for "thinking" time
        yield return new WaitForSeconds(dealerThinkTime);
        
        // Calculate bullet statistics for decision making
        var (liveCount, blankCount) = GetRemainingBulletStats();
        Debug.Log($"Remaining bullets: {liveCount} LIVE, {blankCount} BLANK");
        
        bool shootSelf;
        
        if (blankCount >= liveCount)
        {
            // If blank bullets >= live bullets, shoot self (safer)
            shootSelf = true;
            Debug.Log($"Dealer logic: {blankCount} blanks >= {liveCount} lives → SHOOT SELF");
        }
        else
        {
            // If more live bullets than blanks, shoot opponent (riskier but could kill)
            shootSelf = false;
            Debug.Log($"Dealer logic: {blankCount} blanks < {liveCount} lives → SHOOT PLAYER");
        }
        
        // Add some randomness to make it less predictable (optional)
        if (Random.value < 0.1f) // 10% chance to go against the logic
        {
            shootSelf = !shootSelf;
            Debug.Log($"Dealer goes against logic! Will shoot {(shootSelf ? "self" : "player")}");
        }
        
        Debug.Log($"Dealer chooses to shoot {(shootSelf ? "themselves" : "the player")}...");
        
        isProcessingTurn = true;
        ExecuteShot(shootSelf); // Dealer makes intelligent decision
    }
    
    // ========== TOOL SYSTEM ==========
    
    void ShowToolSelection()
    {
        if (!gameState.gameActive || !gameState.playerTurn || isSelectingTool)
            return;
        
        // Check if player has any tools
        if (gameState.playerTools.Count == 0)
        {
            Debug.Log("No tools available!");
            if (toolInfoText != null)
                toolInfoText.text = "No tools available!";
            return;
        }
        
        isSelectingTool = true;
        // hasUsedToolThisTurn = false;
        
        // Show tool panel
        if (toolSelectionPanel != null)
            toolSelectionPanel.SetActive(true);
        
        // Update which tools are available
        UpdateToolButtons();

        // LIST TOOLS IN CONSOLE
        Debug.Log("=== PLAYER TOOLS AVAILABLE ===");
        for (int i = 0; i < gameState.playerTools.Count; i++)
        {
            var tool = gameState.playerTools[i];
            Debug.Log($"{i+1}. {tool.name} ({tool.type}) - {tool.description}");
        }
        Debug.Log("===============================");
        
        Debug.Log("Select a tool to use (or press Space to shoot)");
        if (toolInfoText != null)
            toolInfoText.text = "Select a tool (or Space to shoot)";
    }
    
    void UpdateToolButtons()
    {
        if (toolButtons == null) return;
        
        // Map tool types to buttons
        ToolSystem.ToolType[] toolOrder = {
            ToolSystem.ToolType.BurnerPhone,
            ToolSystem.ToolType.MagnifyingGlass,
            ToolSystem.ToolType.Beer,
            ToolSystem.ToolType.Pills,
            ToolSystem.ToolType.HandSaw,
            ToolSystem.ToolType.Adrenaline
        };
        
        for (int i = 0; i < toolButtons.Length; i++)
        {
            if (i < toolOrder.Length)
            {
                bool hasTool = gameState.HasTool(true, toolOrder[i]);
                // toolButtons[i].interactable = hasTool && !hasUsedToolThisTurn;
                toolButtons[i].interactable = hasTool; // <-- CHANGED
                
                // Update button text
                Text buttonText = toolButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = hasTool ? GetToolShortName(toolOrder[i]) : "---";
                }
            }
        }
    }
    
    string GetToolShortName(ToolSystem.ToolType toolType)
    {
        return toolType switch
        {
            ToolSystem.ToolType.BurnerPhone => "Phone",
            ToolSystem.ToolType.MagnifyingGlass => "Glass",
            ToolSystem.ToolType.Beer => "Beer",
            ToolSystem.ToolType.Pills => "Pills",
            ToolSystem.ToolType.HandSaw => "Saw",
            ToolSystem.ToolType.Adrenaline => "Adren",
            _ => "???"
        };
    }
    
    void OnToolSelected(int toolIndex)
    {
        // if (!isSelectingTool || hasUsedToolThisTurn) return;
        if (!isSelectingTool) return;
        
        ToolSystem.ToolType[] toolOrder = {
            ToolSystem.ToolType.BurnerPhone,
            ToolSystem.ToolType.MagnifyingGlass,
            ToolSystem.ToolType.Beer,
            ToolSystem.ToolType.Pills,
            ToolSystem.ToolType.HandSaw,
            ToolSystem.ToolType.Adrenaline
        };
        
        if (toolIndex < toolOrder.Length)
        {
            ToolSystem.ToolType selectedTool = toolOrder[toolIndex];
            
            if (gameState.HasTool(true, selectedTool))
            {
                // UseTool(true, selectedTool);
                UseSpecificTool(true, selectedTool);
                // hasUsedToolThisTurn = true;

                // After using tool, update buttons but DON'T hide selection
                // Player can use another tool if they want
                UpdateToolButtons();
                
                // Ask if player wants to use another tool
                // if (gameState.playerTools.Count > 0)
                // {
                //     StartCoroutine(AskForAnotherTool());
                // }
                // else
                // {
                //     HideToolSelection();
                // }

                //
                //
                //
                // Check if player still has tools
                if (gameState.playerTools.Count == 0)
                {
                    // No tools left, hide selection
                    HideToolSelection();
                }
            }
        }
    }
    
    // IEnumerator AskForAnotherTool()
    // {
    //     if (toolInfoText != null)
    //         toolInfoText.text = "Use another tool? (Click tool or Space to shoot)";
        
    //     yield return new WaitForSeconds(0.5f);
    //     UpdateToolButtons(); // Re-enable buttons for another tool
    //     // hasUsedToolThisTurn = false;
    // }
    
    void HideToolSelection()
    {
        isSelectingTool = false;
        if (toolSelectionPanel != null)
            toolSelectionPanel.SetActive(false);
    }
    
    // ========== TOOL ACTIONS ==========
    
    // void UseTool(bool isPlayer, ToolSystem.ToolType toolType)
    // {
    //     if (!gameState.HasTool(isPlayer, toolType))
    //     {
    //         Debug.Log($"{(isPlayer ? "Player" : "Dealer")} doesn't have {toolType}!");
    //         return;
    //     }
        
    //     switch (toolType)
    //     {
    //         case ToolSystem.ToolType.BurnerPhone:
    //             UseBurnerPhone(isPlayer);
    //             break;
                
    //         case ToolSystem.ToolType.MagnifyingGlass:
    //             UseMagnifyingGlass(isPlayer);
    //             break;
                
    //         case ToolSystem.ToolType.Beer:
    //             UseBeer(isPlayer);
    //             break;
                
    //         case ToolSystem.ToolType.Pills:
    //             UsePills(isPlayer);
    //             break;
                
    //         case ToolSystem.ToolType.HandSaw:
    //             // Just mark it as used - effect applies on next shot
    //             gameState.UseTool(isPlayer, toolType);
    //             break;
                
    //         case ToolSystem.ToolType.Adrenaline:
    //             UseAdrenaline(isPlayer);
    //             break;
    //     }
        
    //     UpdateToolUI();
    // }
    
    // 
    // 
    // 
    // ADD THIS NEW HELPER FUNCTION:
    void UseSpecificTool(bool isPlayer, ToolSystem.ToolType toolType)
    {
        if (!gameState.HasTool(isPlayer, toolType)) return;
        
        switch (toolType)
        {
            case ToolSystem.ToolType.BurnerPhone:
                UseBurnerPhone(isPlayer);
                break;
            case ToolSystem.ToolType.MagnifyingGlass:
                UseMagnifyingGlass(isPlayer);
                break;
            case ToolSystem.ToolType.Beer:
                UseBeer(isPlayer);
                break;
            case ToolSystem.ToolType.Pills:
                UsePills(isPlayer);
                break;
            case ToolSystem.ToolType.HandSaw:
                UseHandSaw(isPlayer);
                break;
            case ToolSystem.ToolType.Adrenaline:
                UseAdrenaline(isPlayer);
                break;
        }
        
        UpdateToolUI();
    }

    public void UseBurnerPhone(bool isPlayer)
    {
        Debug.Log($"{(isPlayer ? "Player" : "Dealer")} uses Burner Phone");
        
        // string result = chamberManager.RevealRandomShell();

        var (success, chamberIndex, shellType) = chamberManager.RevealRandomShell();
        if (success)
        {
            string result = $"Revealed: Chamber {chamberIndex} = " + (shellType == GameState.ShellType.Live ? "LIVE" : "BLANK");
            // Use chamberIndex and shellType here
            if (toolInfoText != null && isPlayer)
                toolInfoText.text = result;
            gameState.RemoveTool(isPlayer, ToolSystem.ToolType.BurnerPhone);
            UpdateToolUI();
        
        }

        // if (toolInfoText != null && isPlayer)
        //     toolInfoText.text = result;
        
        // gameState.UseTool(isPlayer, ToolSystem.ToolType.BurnerPhone);
    }
    
    public void UseMagnifyingGlass(bool isPlayer)
    {
        Debug.Log($"{(isPlayer ? "Player" : "Dealer")} uses Magnifying Glass");

        var (success, shellType) = chamberManager.InspectCurrentChamber();
        if (success)
        {
            string shellName = shellType == GameState.ShellType.Live ? "LIVE" : "BLANK";
            string result = $"Current chamber: {shellName}";

            if (toolInfoText != null && isPlayer)
                toolInfoText.text = $"Current chamber: {result}";
                
            gameState.RemoveTool(isPlayer, ToolSystem.ToolType.MagnifyingGlass);
            UpdateToolUI();
        }
        
        // string result = chamberManager.InspectCurrentChamber();
        
        // if (toolInfoText != null && isPlayer)
        //     toolInfoText.text = $"Current chamber: {result}";
        
        // gameState.UseTool(isPlayer, ToolSystem.ToolType.MagnifyingGlass);
    }
    
    public void UseBeer(bool isPlayer)
    {
        Debug.Log($"{(isPlayer ? "Player" : "Dealer")} uses Beer");
        
        chamberManager.SkipCurrentChamber();
        gameState.RemoveTool(isPlayer, ToolSystem.ToolType.Beer);
        UpdateToolUI();
        
        // If player used beer, their turn continues
        if (isPlayer)
        {
            StartCoroutine(PlayerTurnSequence());
        }
    }
    
    public void UsePills(bool isPlayer)
    {
        Debug.Log($"{(isPlayer ? "Player" : "Dealer")} uses Pills");
        
        bool heal = Random.value < 0.4f;
        
        if (isPlayer)
        {
            if (heal)
            {
                gameState.playerHP = Mathf.Min(gameState.playerHP + 1, gameState.GetCurrentRoundHP());
                Debug.Log("💊 Pills healed 1 HP!");
                if (toolInfoText != null)
                    toolInfoText.text = "Pills healed 1 HP!";
            }
            else
            {
                gameState.playerHP = Mathf.Max(0, gameState.playerHP - 1);
                Debug.Log("💊 Pills caused 1 damage!");
                if (toolInfoText != null)
                    toolInfoText.text = "Pills caused 1 damage!";
                gameState.CheckRoundEnd();
            }
        }
        else
        {
            if (heal)
            {
                gameState.dealerHP = Mathf.Min(gameState.dealerHP + 1, gameState.GetCurrentRoundHP());
                Debug.Log("💊 Dealer's pills healed 1 HP!");
            }
            else
            {
                gameState.dealerHP = Mathf.Max(0, gameState.dealerHP - 1);
                Debug.Log("💊 Dealer's pills caused 1 damage!");
                gameState.CheckRoundEnd();
            }
        }
        
        gameState.RemoveTool(isPlayer, ToolSystem.ToolType.Pills);
        UpdateHPUI();
        UpdateToolUI();
    }
    
    public void UseAdrenaline(bool isPlayer)
    {
        Debug.Log($"{(isPlayer ? "Player" : "Dealer")} uses Adrenaline");
        
        if (isPlayer && gameState.dealerTools.Count > 0)
        {
            int randomIndex = Random.Range(0, gameState.dealerTools.Count);
            var stolenTool = gameState.dealerTools[randomIndex];
            
            gameState.playerTools.Add(stolenTool);
            gameState.dealerTools.RemoveAt(randomIndex);
            
            Debug.Log($"⚡ Stole {stolenTool.name} from dealer!");
            
            if (toolInfoText != null)
                toolInfoText.text = $"Stole {stolenTool.name}!";
            
            // Use it immediately
            gameState.RemoveTool(isPlayer, stolenTool.type);
            UpdateToolUI();
        }
        else if (!isPlayer && gameState.playerTools.Count > 0)
        {
            int randomIndex = Random.Range(0, gameState.playerTools.Count);
            var stolenTool = gameState.playerTools[randomIndex];
            
            gameState.dealerTools.Add(stolenTool);
            gameState.playerTools.RemoveAt(randomIndex);
            
            Debug.Log($"⚡ Dealer stole {stolenTool.name} from player!");
            
            // Dealer uses it immediately
            gameState.RemoveTool(false, stolenTool.type);
            UpdateToolUI();
        }
        else
        {
            Debug.Log("⚡ No tools to steal!");
            if (toolInfoText != null)
                toolInfoText.text = "No tools to steal!";
        }
        
        gameState.RemoveTool(isPlayer, ToolSystem.ToolType.Adrenaline);
        UpdateToolUI();
    }
    
    // ################### 
    // UPDATE: add missing function 
    // ################### 
    public void UseHandSaw(bool isPlayer)
    {
        Debug.Log($"{(isPlayer ? "Player" : "Dealer")} uses Hand Saw");
        
        // Mark hand saw as active - damage multiplier will be applied on next shot
        gameState.RemoveTool(isPlayer, ToolSystem.ToolType.HandSaw);
        
        string message = "⚔️ Hand Saw ready! Next shot deals DOUBLE DAMAGE!";
        Debug.Log(message);
        
        if (toolInfoText != null && isPlayer)
            toolInfoText.text = message;
        
        // Update UI to show hand saw is active
        if (isPlayer)
        {
            // You might want to add a visual indicator
            if (instructionText != null)
                instructionText.text = "HAND SAW ACTIVE! Next shot = 2x damage";
        }
    }

    // 
    // 
    // 
    // In GameFlowController.cs - Add this method anywhere in the class
    public void OnToolPlacementComplete()
    {
        Debug.Log("✓ Tool placement finished!");
        
        // Just make sure game continues normally
        UpdateAllUI();
    }


    // ========== UI MANAGEMENT ==========
    
    void UpdateAllUI()
    {
        UpdateTurnUI();
        UpdateHPUI();
        UpdateRoundUI();
        UpdateBulletUI();
        UpdateToolUI();
        UpdateInstructionUI();
    }
    
    void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = gameState.playerTurn ? 
                "👤 PLAYER'S TURN" : "🤖 DEALER'S TURN";
            turnText.color = gameState.playerTurn ? Color.green : Color.yellow;
        }
        
        if (playerTurnPanel != null)
        {
            bool shouldShow = gameState.gameActive && 
                            gameState.playerTurn && 
                            !isProcessingTurn;
            playerTurnPanel.SetActive(shouldShow);
        }
        
        if (dealerTurnPanel != null)
        {
            bool shouldShow = gameState.gameActive && 
                            !gameState.playerTurn && 
                            !isProcessingTurn;
            dealerTurnPanel.SetActive(shouldShow);
        }
        
        if (shootSelfButton != null)
            shootSelfButton.interactable = gameState.gameActive && gameState.playerTurn && !isProcessingTurn;
        
        if (shootOpponentButton != null)
            shootOpponentButton.interactable = gameState.gameActive && gameState.playerTurn && !isProcessingTurn;
    }
    
    void UpdateHPUI()
    {
        if (playerHPText != null)
        {
            int maxHP = gameState.GetCurrentRoundHP();
            playerHPText.text = $"Player: {gameState.playerHP}/{maxHP}";
            playerHPText.color = gameState.playerHP <= 1 ? Color.red : 
                                gameState.playerHP <= maxHP/2 ? Color.yellow : Color.green;
        }
        
        if (dealerHPText != null)
        {
            int maxHP = gameState.GetCurrentRoundHP();
            dealerHPText.text = $"Dealer: {gameState.dealerHP}/{maxHP}";
            dealerHPText.color = gameState.dealerHP <= 1 ? Color.red : 
                                gameState.dealerHP <= maxHP/2 ? Color.yellow : Color.yellow;
        }
    }
    
    void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = $"Round {gameState.currentRound}/{gameState.totalRounds}";
            roundText.color = gameState.currentRound == 3 ? Color.red : 
                             gameState.currentRound == 2 ? Color.yellow : Color.white;
        }
    }
    
    void UpdateBulletUI()
    {
        if (bulletCountText != null)
        {
            int remaining = gameState.GetRemainingBullets();
            int total = gameState.totalBulletsLoaded;
            
            bulletCountText.text = $"Bullets: {remaining}/{total}";
            
            if (gameState.NeedToReload())
            {
                bulletCountText.text += " (RELOAD!)";
                bulletCountText.color = Color.red;
            }
            else if (remaining <= 2)
            {
                bulletCountText.color = Color.yellow;
            }
            else
            {
                bulletCountText.color = Color.white;
            }
        }
    }
    
    void UpdateToolUI()
    {
        if (playerToolsText != null)
        {
            string toolText = "Player Tools: ";
            if (gameState.playerTools.Count == 0)
            {
                toolText += "None";
            }
            else
            {
                foreach (var tool in gameState.playerTools)
                {
                    toolText += GetToolShortName(tool.type) + " ";
                }
            }
            playerToolsText.text = toolText;
            playerToolsText.color = gameState.playerTools.Count > 0 ? Color.green : Color.gray;
        }
        
        if (dealerToolsText != null)
        {
            string toolText = "Dealer Tools: ";
            if (gameState.dealerTools.Count == 0)
            {
                toolText += "None";
            }
            else
            {
                toolText += $"{gameState.dealerTools.Count} hidden";
            }
            dealerToolsText.text = toolText;
        }
        
        // Update hand saw indicator
        if (instructionText != null && gameState.nextShotDoubleDamage)
        {
            string sawUser = gameState.handSawUsedByPlayer ? "Player" : "Dealer";
            instructionText.text += $"\n⚔️ {sawUser}'s next shot deals 2x damage!";
        }
    }
    
    void UpdateInstructionUI()
    {
        if (instructionText != null)
        {
            string instructions = "";
            
            if (!gameState.gameActive)
            {
                if (gameState.gameOver)
                {
                    instructions = "Game Over! Press R to restart.";
                }
                else
                {
                    instructions = "Round Over! Starting next round...";
                }
            }
            else if (gameState.playerTurn)
            {
                instructions = "Your turn!\n";
                instructions += "1: Shoot Self | 2: Shoot Dealer\n";
                
                if (gameState.playerTools.Count > 0)
                {
                    instructions += "T: Use Tool | ";
                }
                
                if (chamberManager.NeedToReload())
                {
                    instructions += "\n⚠️ Need to reload!";
                }
            }
            else
            {
                instructions = "Dealer's turn... thinking...";
            }
            
            instructionText.text = instructions;
        }
    }
    
    // ========== INPUT HANDLING ==========
    
    void Update()
    {
        if (!gameState.gameActive || isProcessingTurn) return;
        
        HandleGameInput();
        HandleDebugInput();
    }

    // 
    // 
    // 
    // Add this method to handle Shift+Number quick tool selection
    void HandleQuickToolSelection()
    {
        // if (hasUsedToolThisTurn)
        // {
        //     Debug.Log("Already used a tool this turn!");
        //     if (toolInfoText != null)
        //         toolInfoText.text = "Already used a tool this turn!";
        //     return;
        // }
        
        // Map number keys to tool types
        Dictionary<int, ToolSystem.ToolType> numberToTool = new Dictionary<int, ToolSystem.ToolType>()
        {
            {3, ToolSystem.ToolType.BurnerPhone},
            {4, ToolSystem.ToolType.MagnifyingGlass},
            {5, ToolSystem.ToolType.Beer},
            {6, ToolSystem.ToolType.Pills},
            {7, ToolSystem.ToolType.HandSaw},
            {8, ToolSystem.ToolType.Adrenaline}
        };
        
        // Check Shift + Number keys 1-6
        for (int number = 3; number <= 8; number++)
        {
            bool numberKeyPressed = Input.GetKeyDown(KeyCode.Alpha0 + number) || 
                                (number <= 5 && Input.GetKeyDown(KeyCode.Keypad0 + number));
            
            if (numberKeyPressed && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                if (numberToTool.TryGetValue(number, out ToolSystem.ToolType toolType))
                {
                    if (gameState.HasTool(true, toolType))
                    {
                        Debug.Log($"Quick using {toolType} (Shift+{number})");
                        UseSpecificTool(true, toolType);
                        // hasUsedToolThisTurn = true;
                        
                        // Show feedback
                        if (toolInfoText != null)
                            toolInfoText.text = $"Used {toolType} (Shift+{number})";
                            
                        return;
                    }
                    else
                    {
                        Debug.Log($"Don't have {toolType}!");
                        if (toolInfoText != null)
                            toolInfoText.text = $"No {toolType} available!";
                        return;
                    }
                }
            }
        }
    }
    
    // void HandleGameInput()
    // {
    //     // Player controls
    //     if (gameState.playerTurn)
    //     {
    //         // Shoot commands
    //         if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
    //         {
    //             OnShootChoice(true);
    //         }
    //         else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
    //         {
    //             OnShootChoice(false);
    //         }
            
    //         // Tool controls
    //         if (Input.GetKeyDown(KeyCode.T))
    //         {
    //             if (!isSelectingTool)
    //             {
    //                 ShowToolSelection();
    //             }
    //             else
    //             {
    //                 HideToolSelection();
    //             }
    //         }
            
    //         // Shoot with space (skip tool selection)
    //         if (Input.GetKeyDown(KeyCode.Space) && isSelectingTool)
    //         {
    //             HideToolSelection();
    //         }
    //     }
        
    //     // Global controls
    //     if (Input.GetKeyDown(KeyCode.N))
    //     {
    //         if (gameState.ShouldStartNewRound())
    //         {
    //             StartNewRound();
    //         }
    //     }
        
    //     if (Input.GetKeyDown(KeyCode.R))
    //     {
    //         RestartGame();
    //     }
    // }

    // 
    // 
    // 
    void HandleGameInput()
{
    if (!gameState.gameActive || isProcessingTurn) return;
    
    // Player controls
    if (gameState.playerTurn)
    {
        // Shoot commands
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            OnShootChoice(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            OnShootChoice(false);
        }
        
        // Tool menu toggle
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!isSelectingTool)
            {
                ShowToolSelection();
            }
            else
            {
                HideToolSelection();
            }
        }
        
        // QUICK TOOL SELECTION: Shift + Number (3-8)
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            HandleQuickToolSelection();
        }
        
        // Direct tool selection when menu is open
        if (isSelectingTool)
        {
            // Number keys 3-8 for tool selection
            for (int i = 3; i <= 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || 
                    (i <= 5 && Input.GetKeyDown(KeyCode.Keypad0 + i)))
                {
                    OnToolSelected(i - 1); // Convert to 0-based index
                    return;
                }
            }
            
            // Cancel with ESC or Space
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                HideToolSelection();
            }
        }
    }
    
    // Global controls
    if (Input.GetKeyDown(KeyCode.N))
    {
        if (gameState.ShouldStartNewRound())
        {
            StartNewRound();
        }
    }
    
    if (Input.GetKeyDown(KeyCode.R))
    {
        RestartGame();
    }
}
    
    void HandleDebugInput()
    {
        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                gameState.DebugBulletStatus();
            }
            
            if (Input.GetKeyDown(KeyCode.F2))
            {
                gameState.DebugGameStatus();
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                chamberManager.DebugChamberState();
            }
            
            if (Input.GetKeyDown(KeyCode.F4))
            {
                // Debug tool info
                Debug.Log($"Player tools: {gameState.playerTools.Count}");
                Debug.Log($"Dealer tools: {gameState.dealerTools.Count}");
                Debug.Log($"Hand saw active: {gameState.handSawActive}");
                Debug.Log($"Next shot double damage: {gameState.nextShotDoubleDamage}");
            }
            
            // Force reload
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("Force reloading...");
                chamberManager.ReloadChamber();
                UpdateBulletUI();
            }
            
            // Force dealer to use specific tool
            if (Input.GetKeyDown(KeyCode.F6) && !gameState.playerTurn)
            {
                StartCoroutine(DealerUseTool());
            }
        }
    }
    
    // ========== GAME FLOW ==========
    
    IEnumerator AutoNextRound()
    {
        Debug.Log("Waiting for next round...");
        yield return new WaitForSeconds(roundTransitionTime);
        
        StartNewRound();
    }
    
    public void RestartGame()
    {
        Debug.Log("\n🔄 RESTARTING GAME...");
        
        // Reset game state
        gameState.currentRound = 1;
        gameState.playerWins = 0;
        gameState.totalRoundsPlayed = 0;
        gameState.playerWonLastRound = false;
        gameState.gameActive = false;
        gameState.gameOver = false;
        gameState.playerTurn = true;
        gameState.getsExtraTurn = false;
        
        // Clear tools
        gameState.ClearTools();
        
        // Clear chamber
        foreach (var shell in gameState.chamberShells)
        {
            shell.type = GameState.ShellType.Empty;
            shell.fired = false;
        }
        gameState.currentChamberIndex = 0;
        gameState.totalBulletsLoaded = 0;
        gameState.bulletsFired = 0;
        
        // Start fresh
        StartNewRound();
    }
    
    // ========== UTILITY METHODS ==========
    
    public bool IsGameActive()
    {
        return gameState.gameActive && !gameState.gameOver && !isProcessingTurn;
    }
    
    public void SetDealerThinkTime(float time)
    {
        dealerThinkTime = Mathf.Clamp(time, 0.5f, 5f);
    }
}

// Extension method for string repeating
public static class StringExtensions
{
    public static string Repeat(this char ch, int count)
    {
        return new string(ch, count);
    }
}
