// ToolPlacementUI.cs - Updated for 8 slots
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ToolPlacementUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject toolPlacementPanel;
    public GameObject toolUsagePanel;
    public Text placementInstructionText;
    public Text toolInfoText;
    public Button confirmPlacementButton;
    
    [Header("Center Area")]
    public ToolSlot centerSlot;
    
    [Header("Left Slots (4 slots - 2x2)")]
    public List<ToolSlot> leftSlots = new List<ToolSlot>();
    
    [Header("Right Slots (4 slots - 2x2)")]
    public List<ToolSlot> rightSlots = new List<ToolSlot>();
    
    [Header("Tool Icons")]
    public Sprite burnerPhoneIcon;
    public Sprite magnifyingGlassIcon;
    public Sprite beerIcon;
    public Sprite pillsIcon;
    public Sprite handSawIcon;
    public Sprite adrenalineIcon;
    
    [Header("Settings")]
    public float toolRevealDelay = 0.5f;
    public Color validDropColor = Color.green;
    public Color invalidDropColor = Color.red;
    
    // Combined list of all player slots
    private List<ToolSlot> allPlayerSlots = new List<ToolSlot>();
    
    // State
    [HideInInspector] public bool isPlacingTools = false;
    private List<ToolSystem.Tool> toolsToPlace = new List<ToolSystem.Tool>();
    private int currentToolIndex = 0;
    private GameFlowController gameFlowController;
    private GameState gameState;
    private ToolSlot lastHoveredSlot = null;
    
    void Start()
    {
        // gameFlowController = FindObjectOfType<GameFlowController>();
        // gameState = FindObjectOfType<GameState>();
        gameFlowController = FindAnyObjectByType<GameFlowController>();
        gameState = FindAnyObjectByType<GameState>();
        
        // Combine all player slots
        allPlayerSlots.Clear();
        allPlayerSlots.AddRange(leftSlots);
        allPlayerSlots.AddRange(rightSlots);
        
        // Validate we have exactly 8 player slots
        if (allPlayerSlots.Count != 8)
        {
            Debug.LogError($"Need exactly 8 player slots! Currently have: {allPlayerSlots.Count}");
        }
        
        // Set slot indices
        for (int i = 0; i < allPlayerSlots.Count; i++)
        {
            allPlayerSlots[i].slotIndex = i;
        }
        
        if (confirmPlacementButton != null)
            confirmPlacementButton.onClick.AddListener(ConfirmPlacement);
        
        HideAllUI();
    }
    
    public void StartToolPlacement(List<ToolSystem.Tool> playerTools)
    {
        toolsToPlace = new List<ToolSystem.Tool>(playerTools);
        currentToolIndex = 0;
        isPlacingTools = true;
        
        // Clear all slots
        ClearAllSlots();
        
        // Show UI
        toolPlacementPanel.SetActive(true);
        toolUsagePanel.SetActive(false);
        confirmPlacementButton.interactable = false;
        
        // Show first tool
        StartCoroutine(PlaceNextTool());
    }
    
    IEnumerator PlaceNextTool()
    {
        if (currentToolIndex >= toolsToPlace.Count)
        {
            // All tools placed
            placementInstructionText.text = "All tools placed! Click Confirm when ready.";
            confirmPlacementButton.interactable = true;
            
            // Update game state with final positions
            UpdateGameStateFromSlots();
            yield break;
        }
        
        // Get next tool
        ToolSystem.Tool tool = toolsToPlace[currentToolIndex];
        
        // Assign icon based on tool type
        tool.icon = GetToolIcon(tool.type);
        
        // Place in center slot
        centerSlot.SetTool(tool);
        
        // Update instruction
        placementInstructionText.text = $"Drag {tool.name} to an empty slot\nTool {currentToolIndex + 1} of {toolsToPlace.Count}";
        
        // Highlight empty slots
        HighlightEmptySlots(true, Color.yellow);
        
        // Wait for player to place it
        while (centerSlot.isOccupied && isPlacingTools)
        {
            yield return null;
        }
    }
    
    // Called when dragging a tool from player slot back to center
    public void ReturnToolToCenter(ToolSlot fromSlot)
    {
        if (!isPlacingTools || !fromSlot.isOccupied || fromSlot.isCenterSlot) return;
        
        // Move tool to center
        centerSlot.SetTool(fromSlot.currentTool);
        fromSlot.SetTool(null);
        
        // Move back one step
        currentToolIndex = Mathf.Max(0, currentToolIndex - 1);
        
        // Update instruction
        placementInstructionText.text = $"Re-placing {centerSlot.currentTool?.name}\nTool {currentToolIndex + 1} of {toolsToPlace.Count}";
        
        // Highlight empty slots
        HighlightEmptySlots(true, Color.yellow);
    }
    
    // Called when dragging from center to empty slot
    public void MoveToolBetweenSlots(ToolSlot fromSlot, ToolSlot toSlot)
    {
        if (!isPlacingTools) return;
        
        // If dragging from center to empty player slot
        if (fromSlot.isCenterSlot && !toSlot.isCenterSlot && !toSlot.isOccupied)
        {
            toSlot.SetTool(fromSlot.currentTool);
            fromSlot.SetTool(null);
            
            // Move to next tool
            currentToolIndex++;
            HighlightEmptySlots(false);
            StartCoroutine(PlaceNextTool());
        }
        // If dragging between player slots (swap)
        else if (!fromSlot.isCenterSlot && !toSlot.isCenterSlot)
        {
            if (toSlot.isOccupied)
            {
                // Swap tools
                ToolSystem.Tool temp = toSlot.currentTool;
                toSlot.SetTool(fromSlot.currentTool);
                fromSlot.SetTool(temp);
            }
            else
            {
                // Move tool to empty slot
                toSlot.SetTool(fromSlot.currentTool);
                fromSlot.SetTool(null);
            }
            
            UpdateGameStateFromSlots();
        }
    }
    
    // Alternative: Click-based assignment
    public void AssignCenterToolToEmptySlot()
    {
        if (!isPlacingTools || !centerSlot.isOccupied) return;
        
        // Find first empty slot
        ToolSlot emptySlot = allPlayerSlots.Find(slot => !slot.isOccupied);
        if (emptySlot != null)
        {
            emptySlot.SetTool(centerSlot.currentTool);
            centerSlot.SetTool(null);
            
            currentToolIndex++;
            HighlightEmptySlots(false);
            StartCoroutine(PlaceNextTool());
        }
    }
    
    public bool HasToolInCenter()
    {
        return centerSlot != null && centerSlot.isOccupied;
    }
    
    void UpdateGameStateFromSlots()
    {
        // Update gameState.playerTools based on slot positions
        // Note: Tools might be null in some slots
        List<ToolSystem.Tool> placedTools = new List<ToolSystem.Tool>();
        
        foreach (var slot in allPlayerSlots)
        {
            if (slot.isOccupied)
            {
                placedTools.Add(slot.currentTool);
            }
        }
        
        // Update game state
        if (gameState != null)
        {
            gameState.playerTools = placedTools;
        }
    }
    
    void HighlightEmptySlots(bool highlight, Color color = default)
    {
        foreach (var slot in allPlayerSlots)
        {
            if (!slot.isOccupied)
            {
                slot.SetHighlight(highlight, color);
            }
        }
    }
    
    void ClearAllSlots()
    {
        centerSlot.SetTool(null);
        foreach (var slot in allPlayerSlots)
            slot.SetTool(null);
    }
    
    public void ConfirmPlacement()
    {
        if (!isPlacingTools) return;
        
        // Final update
        UpdateGameStateFromSlots();
        
        // Hide placement UI
        isPlacingTools = false;
        toolPlacementPanel.SetActive(false);
        
        // Show usage UI
        toolUsagePanel.SetActive(true);
        
        // Notify game flow that placement is done
        if (gameFlowController != null)
            gameFlowController.OnToolPlacementComplete();
        
        Debug.Log("Tool placement confirmed!");
    }
    
    // TOOL USAGE PHASE ==========================================
    
    public bool CanUseTools()
    {
        return !isPlacingTools && gameState != null && gameState.playerTurn && gameState.gameActive;
    }
    
    public void AttemptToUseTool(ToolSlot slot)
    {
        if (!CanUseTools() || slot.currentTool == null || slot.isCenterSlot) return;
        
        // Show tool info
        toolInfoText.text = $"Use {slot.currentTool.name}?\n{slot.currentTool.description}";
        
        // Start coroutine to use tool
        StartCoroutine(UseToolWithConfirmation(slot));
    }
    
    IEnumerator UseToolWithConfirmation(ToolSlot slot)
    {
        // Highlight the tool
        slot.SetHighlight(true, Color.cyan);
        
        // Wait for confirmation
        bool confirmed = false;
        bool cancelled = false;
        float timeout = 3f;
        float timer = 0f;
        
        toolInfoText.text += "\n\nPress SPACE to use, ESC to cancel";
        
        while (timer < timeout && !confirmed && !cancelled)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                confirmed = true;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                cancelled = true;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        slot.SetHighlight(false);
        
        if (confirmed)
        {
            // Actually use the tool
            UseTool(slot);
        }
        else
        {
            toolInfoText.text = cancelled ? "Tool use cancelled" : "Time expired";
            yield return new WaitForSeconds(1f);
            toolInfoText.text = "";
        }
    }
    
    void UseTool(ToolSlot slot)
    {
        if (slot.currentTool == null || gameFlowController == null) return;
        
        // Call appropriate tool function in GameFlowController
        switch (slot.currentTool.type)
        {
            case ToolSystem.ToolType.BurnerPhone:
                gameFlowController.UseBurnerPhone(true);
                break;
            case ToolSystem.ToolType.MagnifyingGlass:
                gameFlowController.UseMagnifyingGlass(true);
                break;
            case ToolSystem.ToolType.Beer:
                gameFlowController.UseBeer(true);
                break;
            case ToolSystem.ToolType.Pills:
                gameFlowController.UsePills(true);
                break;
            case ToolSystem.ToolType.HandSaw:
                gameFlowController.UseHandSaw(true);
                break;
            case ToolSystem.ToolType.Adrenaline:
                gameFlowController.UseAdrenaline(true);
                break;
        }
        
        // Remove tool from slot (all tools are consumable)
        slot.SetTool(null);
        
        // Update tool info
        toolInfoText.text = $"Used {slot.currentTool?.name}!";
        StartCoroutine(ClearToolInfoAfterDelay(2f));
    }
    
    // UTILITY FUNCTIONS ==========================================
    
    Sprite GetToolIcon(ToolSystem.ToolType toolType)
    {
        return toolType switch
        {
            ToolSystem.ToolType.BurnerPhone => burnerPhoneIcon,
            ToolSystem.ToolType.MagnifyingGlass => magnifyingGlassIcon,
            ToolSystem.ToolType.Beer => beerIcon,
            ToolSystem.ToolType.Pills => pillsIcon,
            ToolSystem.ToolType.HandSaw => handSawIcon,
            ToolSystem.ToolType.Adrenaline => adrenalineIcon,
            _ => null
        };
    }
    
    public void HideAllUI()
    {
        toolPlacementPanel.SetActive(false);
        toolUsagePanel.SetActive(false);
        toolInfoText.text = "";
    }
    
    public void ShowToolUsagePanel()
    {
        toolPlacementPanel.SetActive(false);
        toolUsagePanel.SetActive(true);
    }
    
    IEnumerator ClearToolInfoAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        toolInfoText.text = "";
    }
}
