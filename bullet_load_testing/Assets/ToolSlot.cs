// ToolSlot.cs - Enhanced with drag and drop
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
public class ToolSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
                       IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("References")]
    public Image slotImage;
    public Image iconImage;
    public GameObject highlight;
    public GameObject emptyIndicator;
    public Text slotLabel; // Optional: shows slot number
    
    [Header("Settings")]
    public int slotIndex = 0;
    public bool isCenterSlot = false;
    public bool isPlayerSlot = true;
    
    [HideInInspector] public ToolSystem.Tool currentTool = null;
    [HideInInspector] public bool isOccupied = false;
    
    // private ToolPlacementUI placementUI;
    public ToolPlacementUI placementUI;
    
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;

    void Start()
    {
        // placementUI = FindObjectOfType<ToolPlacementUI>();
        placementUI = FindAnyObjectByType<ToolPlacementUI>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        UpdateVisuals();
        
        // Set slot label
        if (slotLabel != null && !isCenterSlot)
        {
            slotLabel.text = (slotIndex + 1).ToString();
        }
    }
    
    public void SetTool(ToolSystem.Tool tool)
    {
        currentTool = tool;
        isOccupied = (tool != null);
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        // Icon visibility
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(isOccupied);
            if (isOccupied && currentTool != null && currentTool.icon != null)
            {
                iconImage.sprite = currentTool.icon;
                iconImage.color = Color.white;
            }
        }
        
        // Empty indicator
        if (emptyIndicator != null)
            emptyIndicator.SetActive(!isOccupied && isCenterSlot);
            
        // Highlight
        if (highlight != null)
            highlight.SetActive(false);
            
        // Background color
        if (slotImage != null)
        {
            if (isCenterSlot)
                slotImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Dark gray
            else if (isOccupied)
                slotImage.color = new Color(0.2f, 0.5f, 0.2f, 0.8f); // Green
            else
                slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gray
        }
    }
    
    public void SetHighlight(bool active, Color? color = null)
    {
        if (highlight != null)
        {
            highlight.SetActive(active);
            if (active && color.HasValue)
            {
                highlight.GetComponent<Image>().color = color.Value;
            }
        }
    }
    
    // Hover effects
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (placementUI == null || !placementUI.isPlacingTools) return;
        
        if (!isOccupied && !isCenterSlot)
        {
            SetHighlight(true, Color.yellow);
            transform.localScale = Vector3.one * 1.05f;
        }
        else if (isOccupied && !isCenterSlot)
        {
            SetHighlight(true, Color.red);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
        transform.localScale = Vector3.one;
    }
    
    // DRAG & DROP ============================================
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!placementUI.isPlacingTools || !isOccupied || isCenterSlot) return;
        
        isDragging = true;
        originalPosition = transform.position;
        originalParent = transform.parent;
        
        // Make draggable and semi-transparent
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Move to top of hierarchy while dragging
        transform.SetParent(transform.root);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Follow mouse position
        transform.position = eventData.position;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // Reset visual properties
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Return to original position if not dropped on valid slot
        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (placementUI == null || !placementUI.isPlacingTools) return;
        
        ToolSlot draggedSlot = eventData.pointerDrag?.GetComponent<ToolSlot>();
        if (draggedSlot == null || !draggedSlot.isOccupied) return;
        
        // Can't drop on center slot or occupied slot
        if (isCenterSlot || (isOccupied && !isCenterSlot)) return;
        
        // Move tool from dragged slot to this slot
        placementUI.MoveToolBetweenSlots(draggedSlot, this);
    }
    
    // Click to select/use (alternative to drag & drop)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (placementUI == null) return;
        
        // During placement phase
        if (placementUI.isPlacingTools)
        {
            if (isOccupied && !isCenterSlot)
            {
                // Return tool to center
                placementUI.ReturnToolToCenter(this);
            }
            else if (!isOccupied && isCenterSlot && placementUI.HasToolInCenter())
            {
                // Assign center tool to clicked empty slot
                placementUI.AssignCenterToolToEmptySlot();
            }
        }
        // During gameplay (tool usage)
        else if (placementUI.CanUseTools() && isOccupied && !isCenterSlot)
        {
            placementUI.AttemptToUseTool(this);
        }
    }
}
