

// ####################
// UPDATE:  Check if chamber empty before choice
// #################### 

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameFlowController : MonoBehaviour
{
    [Header("CORE REFERENCES")]
    public GameState gameState;
    public BulletSystem bulletSystem;
    public ChamberManager chamberManager;
    public DealerAnimationController dealerAnim;
    public DealerGunInteractor dealerGun;
    public CanvasGroup fadeCanvasGroup;



    [Header("UI REFERENCES")]
    public Text turnText;
    public Text instructionText;
    public Text roundText;
    public Text playerHPText;
    public Text dealerHPText;
    public Text bulletCountText;
    public GameObject playerTurnPanel;
    public GameObject dealerTurnPanel;

    [Header("GAME SETTINGS")]
    public bool autoStart = true;
    public float dealerThinkTime = 1.5f;
    public float roundTransitionTime = 2f;
    public float originalAlpha = 0.7f; // 初始透明度（0~1）

    [Header("DEBUG")]
    public bool debugMode = false;
    public bool isProcessingTurn = false; // Prevent multiple clicks

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        // Start the game
        if (autoStart)
        {
            StartCoroutine(StartNewRound());
        }
    }

    // ========== ROUND MANAGEMENT ==========

    IEnumerator StartNewRound()
    {
        if (gameState.gameOver)
        {
            Debug.Log("Game is over! Press R to restart.");
            UpdateAllUI();
            yield break;
        }

        Debug.Log($"\n{'='.Repeat(40)}");
        Debug.Log($"=== STARTING ROUND {gameState.currentRound} of {gameState.totalRounds} ===");
        Debug.Log($"{'='.Repeat(40)}");

        // Reset game state for new round
        gameState.ResetHPForRound();
        gameState.getsExtraTurn = false;
        isProcessingTurn = false;
        if (dealerAnim.isdead)
        {
            yield return StartCoroutine(dealerAnim.PlayRecoverIfAlive());
            dealerAnim.isdead = false;
        }

        // Initial reload
        yield return StartCoroutine(chamberManager.ReloadChamber());

        // Update UI
        UpdateAllUI();

        gameState.playerTurn = true; // Player starts
        Debug.Log($"Round {gameState.currentRound}: {gameState.GetCurrentRoundHP()} HP each");
        Debug.Log("Player's turn first!");
    }

    // ========== PLAYER ACTIONS ==========

    // Called when player chooses who to shoot
    public void OnShootChoice(bool shootSelf)
    {
        if (isProcessingTurn) return; // Prevent double clicks
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

        StartCoroutine(ExecuteShot(shootSelf));
    }

    // ========== SHOT EXECUTION ==========

    IEnumerator ExecuteShot(bool shootSelf)
    {
        // Fire the chamber
        bool isLive = chamberManager.FireCurrentChamber();

        bool dealerHitbyPlayer = isLive && !shootSelf && gameState.playerTurn;
        bool dealerHitSelf = isLive && shootSelf && !gameState.playerTurn;

        // Apply damage
        if (isLive)
        {
            if (shootSelf)
            {
                // Player or dealer hit themselves
                Debug.Log("💥 BANG! Shooter hit themselves!");
                gameState.ApplyDamage(gameState.playerTurn);
            }
            else
            {
                // Hit opponent
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

        // Record last shot target
        gameState.lastShotTarget = shootSelf ?
            GameState.ShootTarget.Self : GameState.ShootTarget.Opponent;

        // --- 插入 Dealer 动画 ---
        // ⭐ 特殊情况：Dealer 实弹打自己
        bool dealerDead = gameState.dealerHP <= 0;
        if (dealerHitSelf && dealerAnim != null)
        {
            // 1️ 手瞬间回位 & 释放枪
            if (dealerGun != null)
                dealerGun.EmergencyPutDownGun();
            yield return dealerAnim.PlayHitFlyBack();

            yield return new WaitForSeconds(0.5f);

            if (!dealerDead)
                yield return dealerAnim.PlayRecoverIfAlive();

            else
                dealerAnim.isdead = true;

        }
        else if (dealerHitbyPlayer && dealerAnim != null)
        {
            // ✅ 原有逻辑（打玩家 / 空枪 / 玩家回合）
            yield return dealerAnim.PlayHitFlyBack();

            yield return new WaitForSeconds(0.5f);

            if (!dealerDead)
                yield return dealerAnim.PlayRecoverIfAlive();
            else
                dealerAnim.isdead = true;
        }

        else if ((gameState.playerTurn && shootSelf && isLive) || (!gameState.playerTurn && !shootSelf && isLive))
        {
            // 玩家受到攻击
            fadeCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(2f);
            float counter = 0f;
            while (counter < 2f)
            {
                counter += Time.deltaTime * 2f;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, originalAlpha, counter / 2f);
                yield return null;
            }
            fadeCanvasGroup.alpha = originalAlpha;
        }


        bool shouldPutDownGun =((!shootSelf) || (shootSelf && !isLive)) && !gameState.playerTurn;        

        if (shouldPutDownGun && dealerGun != null)
        {
            yield return dealerGun.PlayPutDownGun();
        }

        // End turn based on rules
        gameState.EndTurn(shootSelf, isLive);

        // Check if round ended
        if (!gameState.gameActive)
        {
            yield return StartCoroutine(HandleRoundEnd());
            yield break;
        }

        // Handle turn continuation
        yield return StartCoroutine(HandleTurnContinuation());
    }


    // ========== TURN MANAGEMENT ==========

    IEnumerator HandleTurnContinuation()
    {
        // If it's still the same player's turn (blank self-shot)
        if (gameState.getsExtraTurn)
        {
            Debug.Log("🎯 Same player gets another turn!");
            gameState.getsExtraTurn = false; // Reset for next decision

            if (gameState.playerTurn)
            {
                // Player gets another turn - CHECK IF NEED TO RELOAD FIRST
                yield return StartCoroutine(PlayerTurnSequence());
            }
            else
            {
                // Dealer gets another turn
                yield return StartCoroutine(DealerTurnSequence());
            }
        }
        else
        {
            // Switch to other player
            if (!gameState.playerTurn)
            {
                // Dealer's turn
                yield return StartCoroutine(DealerTurnSequence());
            }
            else
            {
                // Player's turn - CHECK IF NEED TO RELOAD FIRST
                yield return StartCoroutine(PlayerTurnSequence());
            }
        }
    }

    // NEW: Player turn sequence with reload check
    IEnumerator PlayerTurnSequence()
    {
        // Check if need to reload BEFORE showing buttons
        if (chamberManager.NeedToReload())
        {
            Debug.Log("🔁 Reloading before player's turn...");
            yield return StartCoroutine(chamberManager.ReloadChamber());
            UpdateBulletUI();
            yield return new WaitForSeconds(0.5f); // Brief pause
        }

        UpdateAllUI();

        isProcessingTurn = false;

        Debug.Log("Player's turn - waiting for choice...");
    }

    // NEW: Dealer turn sequence with reload check
    IEnumerator DealerTurnSequence()
    {
        // Check if need to reload BEFORE dealer acts
        if (chamberManager.NeedToReload())
        {
            Debug.Log("🔁 Reloading before dealer's turn...");
            yield return StartCoroutine(chamberManager.ReloadChamber());
            UpdateBulletUI();
            yield return new WaitForSeconds(0.5f);
        }

        UpdateAllUI();
        yield return new WaitForSeconds(0.5f);

        // Now dealer takes action
        yield return StartCoroutine(DealerTakeTurn());
    }

    IEnumerator HandleRoundEnd()
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
            yield return StartCoroutine(AutoNextRound());
        }
        else if (gameState.gameOver)
        {
            Debug.Log("\n🎮 GAME OVER!");
            UpdateAllUI();
        }
    }

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

        // UI
        if (dealerTurnPanel != null)
            dealerTurnPanel.SetActive(true);
        if (playerTurnPanel != null)
            playerTurnPanel.SetActive(false);

        // Dealer 思考
        yield return new WaitForSeconds(dealerThinkTime);

        // ===============================
        // 🧠 决策逻辑（你原来的）
        // ===============================
        var (liveCount, blankCount) = GetRemainingBulletStats();
        Debug.Log($"Remaining bullets: {liveCount} LIVE, {blankCount} BLANK");

        bool shootSelf;

        if (blankCount >= liveCount)
        {
            shootSelf = true;
            Debug.Log($"Dealer logic: {blankCount} blanks >= {liveCount} lives → SHOOT SELF");
        }
        else
        {
            shootSelf = false;
            Debug.Log($"Dealer logic: {blankCount} blanks < {liveCount} lives → SHOOT PLAYER");
        }

        if (Random.value < 0.1f)
        {
            shootSelf = !shootSelf;
            Debug.Log($"Dealer goes against logic! Will shoot {(shootSelf ? "self" : "player")}");
        }

        Debug.Log($"Dealer chooses to shoot {(shootSelf ? "themselves" : "the player")}...");

        isProcessingTurn = true;

        // ===============================
        // 🎬 Dealer 动画开始
        // ===============================

        // 1️⃣ 拿枪
        yield return StartCoroutine(dealerGun.PlayPickupAnimation());

        // 2️⃣ 瞄准目标
        if (shootSelf)
            yield return StartCoroutine(dealerGun.AimGunAtSelf());
        else
            yield return StartCoroutine(dealerGun.AimGunAtPlayer());

        // 3️⃣ 停顿一拍（心理压迫感，很重要）
        yield return new WaitForSeconds(1.0f);

        // ===============================
        // 🔫 真正执行开枪逻辑
        // ===============================
        GameState.ShellType shellType = gameState.GetCurrentChamberType();
        bool isLive = (shellType == GameState.ShellType.Live);
        dealerGun.DealerActivateGun(isLive);
        yield return StartCoroutine(ExecuteShot(shootSelf));

        isProcessingTurn = false;
    }

    // ========== UI MANAGEMENT ==========

    void UpdateAllUI()
    {
        UpdateTurnUI();
        UpdateHPUI();
        UpdateRoundUI();
        UpdateBulletUI();
        UpdateInstructionUI();
    }

    void UpdateTurnUI()
    {
        // Update turn text
        if (turnText != null)
        {
            turnText.text = gameState.playerTurn ?
                "👤 PLAYER'S TURN" : "🤖 DEALER'S TURN";
            turnText.color = gameState.playerTurn ? Color.green : Color.yellow;
        }
    }

    void UpdateHPUI()
    {
        if (playerHPText != null)
        {
            int maxHP = gameState.GetCurrentRoundHP();
            playerHPText.text = $"Player: {gameState.playerHP}/{maxHP}";
            playerHPText.color = gameState.playerHP <= 1 ? Color.red :
                                gameState.playerHP <= maxHP / 2 ? Color.yellow : Color.green;
        }

        if (dealerHPText != null)
        {
            int maxHP = gameState.GetCurrentRoundHP();
            dealerHPText.text = $"Dealer: {gameState.dealerHP}/{maxHP}";
            dealerHPText.color = gameState.dealerHP <= 1 ? Color.red :
                                gameState.dealerHP <= maxHP / 2 ? Color.yellow : Color.yellow;
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
                bulletCountText.color = Color.red;
                bulletCountText.text += " (EMPTY)";
            }
            else if (remaining <= 2)
            {
                bulletCountText.color = Color.yellow;
                bulletCountText.text += " (LOW)";
            }
            else
            {
                bulletCountText.color = Color.green;
                bulletCountText.text += " (READY)";
            }
        }
    }

    void UpdateInstructionUI()
    {
        if (instructionText != null)
        {
            if (!gameState.gameActive)
            {
                if (gameState.gameOver)
                {
                    instructionText.text = gameState.playerWins >= 2 ?
                        "🏆 YOU WIN! Press R to restart" :
                        "😞 DEALER WINS! Press R to restart";
                }
                else
                {
                    instructionText.text = "Round over! Next round starting...";
                }
            }
            else if (gameState.playerTurn)
            {
                if (chamberManager.NeedToReload())
                {
                    instructionText.text = "Reloading chamber...";
                }
                else if (isProcessingTurn)
                {
                    instructionText.text = "Processing...";
                }
                else
                {
                    instructionText.text = "Choose who to shoot:";
                }
            }
            else
            {
                if (isProcessingTurn)
                {
                    instructionText.text = "Dealer is acting...";
                }
                else
                {
                    instructionText.text = "Dealer is thinking...";
                }
            }
        }
    }

    // ========== COROUTINES ==========

    IEnumerator AutoNextRound()
    {
        Debug.Log($"Waiting {roundTransitionTime} seconds before next round...");
        yield return new WaitForSeconds(roundTransitionTime);
        StartCoroutine(StartNewRound());
    }

    // ========== INPUT HANDLING ==========

    void Update()
    {
        // HandleDebugInput();
        // HandleGameInput();
    }

    // void HandleGameInput()
    // {
    //     // Don't process if we're in the middle of something
    //     if (isProcessingTurn) return;

    //     // Player input (keyboard alternative to buttons)
    //     if (gameState.gameActive && gameState.playerTurn && !chamberManager.NeedToReload())
    //     {
    //         if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
    //         {
    //             Debug.Log("Player (keyboard) shoots self");
    //             OnShootChoice(true);
    //         }

    //         if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
    //         {
    //             Debug.Log("Player (keyboard) shoots dealer");
    //             OnShootChoice(false);
    //         }
    //     }

    //     // Next round
    //     if (Input.GetKeyDown(KeyCode.N) && gameState.ShouldStartNewRound())
    //     {
    //         Debug.Log("Manual next round");
    //         StartNewRound();
    //     }

    //     // Restart game
    //     if (Input.GetKeyDown(KeyCode.R))
    //     {
    //         RestartGame();
    //     }

    //     // Force reload (debug)
    //     if (Input.GetKeyDown(KeyCode.L) && debugMode)
    //     {
    //         Debug.Log("Force reload");
    //         chamberManager.ReloadChamber();
    //         UpdateBulletUI();
    //     }
    // }

    // void HandleDebugInput()
    // {
    //     if (!debugMode) return;

    //     if (Input.GetKeyDown(KeyCode.F1))
    //     {
    //         Debug.Log("=== DEBUG INFO ===");
    //         Debug.Log($"Game Active: {gameState.gameActive}");
    //         Debug.Log($"Player Turn: {gameState.playerTurn}");
    //         Debug.Log($"Gets Extra Turn: {gameState.getsExtraTurn}");
    //         Debug.Log($"Need Reload: {chamberManager.NeedToReload()}");
    //         Debug.Log($"Is Processing: {isProcessingTurn}");
    //         gameState.DebugBulletStatus();
    //     }

    //     if (Input.GetKeyDown(KeyCode.F2))
    //     {
    //         gameState.DebugGameStatus();
    //     }
    // }

    // ========== GAME CONTROL ==========

    void RestartGame()
    {
        Debug.Log("\n" + "🔄".Repeat(20));
        Debug.Log("🔄 RESTARTING ENTIRE GAME");
        Debug.Log("🔄".Repeat(20));

        gameState.currentRound = 1;
        gameState.playerWins = 0;
        gameState.totalRoundsPlayed = 0;
        gameState.playerWonLastRound = false;
        gameState.gameOver = false;
        gameState.gameActive = false;
        isProcessingTurn = false;

        StartCoroutine(StartNewRound());
    }
}

// Extension method for string repetition
public static class StringExtensions
{
    public static string Repeat(this char chatToRepeat, int repeat)
    {
        return new string(chatToRepeat, repeat);
    }

    public static string Repeat(this string stringToRepeat, int repeat)
    {
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < repeat; i++)
        {
            builder.Append(stringToRepeat);
        }
        return builder.ToString();
    }
}